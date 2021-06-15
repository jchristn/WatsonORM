using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DatabaseWrapper.SqlServer;
using DatabaseWrapper.Core;
using Watson.ORM.Core;

namespace Watson.ORM.SqlServer
{
    /// <summary>
    /// WatsonORM for SQL Server, a lightweight and easy to use object-relational mapper (ORM).
    /// </summary>
    public class WatsonORM : IDisposable
    {
        #region Public-Members

        /// <summary>
        /// Database settings.
        /// </summary>
        public Watson.ORM.Core.DatabaseSettings Settings
        {
            get
            {
                return _Settings;
            }
        }

        /// <summary>
        /// Method to invoke when sending log messages.
        /// </summary>
        public Action<string> Logger
        {
            get
            {
                return _Logger;
            }
            set
            {
                _Logger = value;

                if (_Database != null)
                {
                    _Database.Logger = value;
                }
            }
        }

        /// <summary>
        /// Debug settings.
        /// </summary>
        public DebugSettings Debug
        {
            get
            {
                return _Debug;
            }
            set
            {
                if (value == null)
                {
                    _Debug = new DebugSettings();
                    if (_Database != null)
                    {
                        _Database.LogQueries = false;
                        _Database.LogResults = false;
                    }
                }
                else
                {
                    _Debug = value;
                    if (_Database != null)
                    {
                        _Database.LogQueries = value.DatabaseQueries;
                        _Database.LogResults = value.DatabaseResults;
                    }
                }
            }
        }

        /// <summary>
        /// Direct access to the underlying database client.
        /// </summary>
        public DatabaseClient Database
        {
            get
            {
                return _Database;
            }
        }

        /// <summary>
        /// Indicates if the database has been initialized.
        /// </summary>
        public bool IsInitialized
        {
            get
            {
                return _Initialized;
            }
        }

        #endregion

        #region Private-Members

        private bool _Initialized = false;
        private Action<string> _Logger = null;
        private DebugSettings _Debug = new DebugSettings();
        private string _Header = "[WatsonORM] ";
        private CancellationTokenSource _TokenSource = new CancellationTokenSource();
        private CancellationToken _Token;
        private Watson.ORM.Core.DatabaseSettings _Settings = null;
        private DatabaseClient _Database = null;
        private TypeMetadataManager _TypeMetadataMgr = new TypeMetadataManager();

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.  Once constructed, call InitializeDatabase() and InitializeTable() for each table if needed.
        /// </summary>
        /// <param name="settings">Database settings.</param>
        public WatsonORM(Watson.ORM.Core.DatabaseSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            _Settings = settings;
            _Token = _TokenSource.Token;

            if (_Settings.Type != Core.DbTypes.SqlServer) throw new ArgumentException("Database type in settings must be of type 'SqlServer'.");
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Dispose of the object and release background workers.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Initialize the database client.
        /// If the client is already initialized, it will first be disposed.
        /// </summary>
        public void InitializeDatabase()
        {
            if (_Database != null)
            {
                _Logger?.Invoke(_Header + "disposing existing database client");
                _Database.Dispose();
            }

            _Logger?.Invoke(_Header + "initializing database client: " + _Settings.Type.ToString() + " on " + _Settings.Hostname + ":" + _Settings.Port);

            _Database = new DatabaseClient(
                _Settings.Hostname,
                _Settings.Port,
                _Settings.Username,
                _Settings.Password,
				_Settings.Instance,
                _Settings.DatabaseName);

            _Logger?.Invoke(_Header + "initialization complete");

            _Initialized = true;
        }

        /// <summary>
        /// Validate a table to determine if any errors or warnings exist.
        /// </summary>
        /// <param name="t">Class for which a table should be validated.</param>
        /// <param name="errors">List of human-readable errors.</param>
        /// <param name="warnings">List of human-readable warnings.</param>
        /// <returns>True if the table will initialize successfully.</returns>
        public bool ValidateTable(Type t, out List<string> errors, out List<string> warnings)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));

            errors = new List<string>();
            warnings = new List<string>();

            string tableName = ReflectionHelper.GetTableNameFromType(t);
            string primaryKeyPropertyName = ReflectionHelper.GetPrimaryKeyPropertyName(t);
            List<Column> columns = ReflectionHelper.GetColumnsFromType(t);

            bool success = true;

            if (String.IsNullOrEmpty(tableName))
            {
                errors.Add("Type '" + t.Name + "' does not have a 'Table' attribute.");
                success = false;
            }

            if (String.IsNullOrEmpty(primaryKeyPropertyName))
            {
                errors.Add("Type '" + t.Name + "' with table name '" + tableName + "' does not have a property with the 'PrimaryKey' attribute.");
                success = false;
            }

            if (columns == null || columns.Count < 1)
            {
                errors.Add("Type '" + t.Name + "' with table name '" + tableName + "' does not have any properties with a 'Column' attribute.");
                success = false;
            }

            if (!_Database.TableExists(tableName))
            {
                warnings.Add("Type '" + t.Name + "' with table name '" + tableName + "' has not yet been created and will be created upon initialization.");
            }
            else
            {
                List<Column> existingColumns = _Database.DescribeTable(tableName);

                foreach (Column column in columns)
                {
                    if (!existingColumns.Exists(c => c.Name.Equals(column.Name)))
                    {
                        errors.Add("Type '" + t.Name + "' with table name '" + tableName + "' exists but column '" + column.Name + "' does not.");
                        success = false;
                    }
                }

                List<string> existingColumnNames = existingColumns.Select(c => c.Name).ToList();
                List<string> definedColumnNames = columns.Select(c => c.Name).ToList();
                List<string> delta = existingColumnNames.Except(definedColumnNames).ToList();
                if (delta != null && delta.Count > 0)
                {
                    foreach (string curr in delta)
                    {
                        warnings.Add("Type '" + t.Name + "' with table name '" + tableName + "' contains additional column '" + curr + "' which is not annotated in the type.");
                    }
                }
            }

            return success;
        }

        /// <summary>
        /// Create table (if it doesn't exist) for a given class.
        /// Adding a table that has already been added will throw an ArgumentException.
        /// </summary>
        /// <param name="t">Class for which a table should be created.</param>
        public void InitializeTable(Type t)
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");
            if (t == null) throw new ArgumentNullException(nameof(t));

            string tableName = ReflectionHelper.GetTableNameFromType(t);
            string primaryKeyPropertyName = ReflectionHelper.GetPrimaryKeyPropertyName(t);
            List<Column> columns = ReflectionHelper.GetColumnsFromType(t);

            if (String.IsNullOrEmpty(tableName))
                throw new InvalidOperationException("Type '" + t.Name + "' does not have a 'Table' attribute.");

            if (String.IsNullOrEmpty(primaryKeyPropertyName))
                throw new InvalidOperationException("Type '" + t.Name + "' with table name '" + tableName + "' does not have a property with the 'PrimaryKey' attribute.");

            if (columns == null || columns.Count < 1)
                throw new InvalidOperationException("Type '" + t.Name + "' with table name '" + tableName + "' does not have any properties with a 'Column' attribute.");

            if (!_Database.TableExists(tableName))
            {
                _Database.CreateTable(tableName, columns);
            }
            else
            {
                List<Column> existingColumns = _Database.DescribeTable(tableName);

                foreach (Column column in columns)
                {
                    if (!existingColumns.Exists(c => c.Name.Equals(column.Name)))
                    {
                        throw new InvalidOperationException("Type '" + t.Name + " with table name '" + tableName + "' exists but column '" + column.Name + "' does not.");
                    }
                }

                List<string> existingColumnNames = existingColumns.Select(c => c.Name).ToList();
                List<string> definedColumnNames = columns.Select(c => c.Name).ToList();
                List<string> delta = existingColumnNames.Except(definedColumnNames).ToList();
                if (delta != null && delta.Count > 0)
                {
                    string deltaStr = "";
                    foreach (string currDelta in delta)
                    {
                        if (!String.IsNullOrEmpty(deltaStr)) deltaStr = deltaStr + ", ";
                        deltaStr += currDelta;
                    }

                    _Logger?.Invoke(_Header + "table " + tableName + " contains additional columns not decorated in the class definition: " + deltaStr);
                }
            }

            _TypeMetadataMgr.Add(t, new TypeMetadata(tableName, primaryKeyPropertyName, columns));

            _Logger?.Invoke(_Header + "initialized table " + tableName + " for type " + t.Name + " with " + columns.Count + " column(s)");
        }

        /// <summary>
        /// Drop table if it exists for a given class.
        /// </summary>
        /// <param name="t">Class for which a table should be dropped.</param>
        public void DropTable(Type t)
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");
            if (t == null) throw new ArgumentNullException(nameof(t));

            string tableName = _TypeMetadataMgr.GetTableNameFromType(t);
            _Logger?.Invoke(_Header + "dropping table " + tableName + " for type " + t.Name);
            _Database.DropTable(tableName);
            _Logger?.Invoke(_Header + "dropped table " + tableName);
        }

        /// <summary>
        /// Truncate table if it exists for a given class.
        /// </summary>
        /// <param name="t">Class for which a table should be dropped.</param>
        public void TruncateTable(Type t)
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");
            if (t == null) throw new ArgumentNullException(nameof(t));

            string tableName = _TypeMetadataMgr.GetTableNameFromType(t);
            _Logger?.Invoke(_Header + "truncating table " + tableName + " for type " + t.Name);
            _Database.Truncate(tableName);
            _Logger?.Invoke(_Header + "truncated table " + tableName);
        }

        /// <summary>
        /// INSERT an object.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="obj">Object to INSERT.</param>
        /// <returns>INSERTed object.</returns>
        public T Insert<T>(T obj) where T : class, new()
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            string tableName = _TypeMetadataMgr.GetTableNameFromObject(obj);

            Dictionary<string, object> insertVals = ObjectToDictionary(obj);
            DataTable result = _Database.Insert(tableName, insertVals);
            return DataTableToObject<T>(result);
        }

        /// <summary>
        /// INSERT multiple records.
        /// This operation will iteratively call Insert on each individual object.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="objs">List of objects.</param>
        /// <returns>List of objects.</returns>
        public List<T> InsertMany<T>(List<T> objs) where T : class, new()
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");
            if (objs == null || objs.Count < 1) throw new ArgumentNullException(nameof(objs));

            string tableName = _TypeMetadataMgr.GetTableNameFromType(typeof(T));

            List<T> ret = new List<T>();
            foreach (T obj in objs)
            {
                Dictionary<string, object> insertVals = ObjectToDictionary(obj);
                DataTable result = _Database.Insert(tableName, insertVals);
                ret.Add(DataTableToObject<T>(result));
            }

            return ret;
        }

        /// <summary>
        /// INSERT multiple records.
        /// This operation performs the INSERT using a single database query, and does not return a list of inserted objects.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="objs">List of objects.</param>
        public void InsertMultiple<T>(List<T> objs) where T : class, new()
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");
            if (objs == null || objs.Count < 1) throw new ArgumentNullException(nameof(objs));

            string tableName = _TypeMetadataMgr.GetTableNameFromType(typeof(T));

            List<Dictionary<string, object>> dicts = new List<Dictionary<string, object>>();
            foreach (T obj in objs)
            {
                dicts.Add(ObjectToDictionary(obj));
            }

            _Database.InsertMultiple(tableName, dicts);
        }

        /// <summary>
        /// UPDATE an object.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="obj">Object to UPDATE.</param>
        /// <returns>UPDATEd object.</returns>
        public T Update<T>(T obj) where T : class, new()
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            string tableName = _TypeMetadataMgr.GetTableNameFromObject(obj);
            string primaryKeyColumnName = _TypeMetadataMgr.GetPrimaryKeyColumnName(typeof(T));
            string primaryKeyPropertyName = _TypeMetadataMgr.GetPrimaryKeyPropertyName(typeof(T));
            object primaryKeyValue = _TypeMetadataMgr.GetPrimaryKeyValue(obj, primaryKeyPropertyName);

            Dictionary<string, object> updateVals = ObjectToDictionary(obj);
            Expression e = new Expression(primaryKeyColumnName, WatsonORMCommon.DbOperatorsConverter(DbOperators.Equals), primaryKeyValue);
            _Database.Update(tableName, updateVals, e);
            DataTable result = _Database.Select(tableName, null, null, null, e, null);
            return DataTableToObject<T>(result);
        }

        /// <summary>
        /// UPDATE multiple rows.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="expr">Expression.</param>
        /// <param name="updateVals">Update values.</param>
        public void UpdateMany<T>(DbExpression expr, Dictionary<string, object> updateVals)
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");
            if (expr == null) throw new ArgumentNullException(nameof(expr));
            if (updateVals == null || updateVals.Count < 1) throw new ArgumentNullException(nameof(updateVals));

            string tableName = _TypeMetadataMgr.GetTableNameFromType(typeof(T));
            string primaryKeyColumnName = _TypeMetadataMgr.GetPrimaryKeyColumnName(typeof(T));

            Expression e = WatsonORMCommon.DbExpressionConverter(expr);
            e.PrependAnd(primaryKeyColumnName, WatsonORMCommon.DbOperatorsConverter(DbOperators.IsNotNull), null);

            _Database.Update(tableName, updateVals, e);
        }

        /// <summary>
        /// DELETE an object.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="obj">Object to DELETE.</param>
        public void Delete<T>(T obj) where T : class, new()
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            string tableName = _TypeMetadataMgr.GetTableNameFromObject(obj);
            string primaryKeyColumnName = _TypeMetadataMgr.GetPrimaryKeyColumnName(typeof(T));
            string primaryKeyPropertyName = _TypeMetadataMgr.GetPrimaryKeyPropertyName(typeof(T));
            object primaryKeyValue = _TypeMetadataMgr.GetPrimaryKeyValue(obj, primaryKeyPropertyName);

            Expression e = new Expression(primaryKeyColumnName, WatsonORMCommon.DbOperatorsConverter(DbOperators.Equals), primaryKeyValue);
            _Database.Delete(tableName, e);
        }

        /// <summary>
        /// DELETE an object by its primary key.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="id">Id value.</param>
        public void DeleteByPrimaryKey<T>(object id) where T : class, new()
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");
            if (id == null) throw new ArgumentNullException(nameof(id));

            string tableName = _TypeMetadataMgr.GetTableNameFromType(typeof(T));
            string primaryKeyColumnName = _TypeMetadataMgr.GetPrimaryKeyColumnName(typeof(T));
            string primaryKeyPropertyName = _TypeMetadataMgr.GetPrimaryKeyPropertyName(typeof(T));

            Expression e = new Expression(primaryKeyColumnName, WatsonORMCommon.DbOperatorsConverter(DbOperators.Equals), id);
            _Database.Delete(tableName, e);
        }

        /// <summary>
        /// DELETE objects by an Expression..
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="expr">Expression.</param>
        public void DeleteMany<T>(DbExpression expr) where T : class, new()
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");
            if (expr == null) throw new ArgumentNullException(nameof(expr));
            string tableName = _TypeMetadataMgr.GetTableNameFromType(typeof(T));
            _Database.Delete(tableName, WatsonORMCommon.DbExpressionConverter(expr));
        }

        /// <summary>
        /// SELECT an object by id.
        /// This operation will return null if the object does not exist.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="id">Id.</param>
        /// <returns>Object.</returns>
        public T SelectByPrimaryKey<T>(object id) where T : class, new()
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");
            if (id == null) throw new ArgumentNullException(nameof(id));

            string tableName = _TypeMetadataMgr.GetTableNameFromType(typeof(T));
            string primaryKeyColumnName = _TypeMetadataMgr.GetPrimaryKeyColumnName(typeof(T));
            string primaryKeyPropertyName = _TypeMetadataMgr.GetPrimaryKeyPropertyName(typeof(T));

            Expression e = new Expression(primaryKeyColumnName, WatsonORMCommon.DbOperatorsConverter(DbOperators.Equals), id);
            DataTable result = _Database.Select(tableName, null, null, null, e, null);
            return DataTableToObject<T>(result);
        }

        /// <summary>
        /// SELECT the first instance of an object matching a given expression.
        /// This operation will return null if the object does not exist.
        /// The ordering used in the underlying query is ascending based on primary key column.
        /// </summary>
        /// <typeparam name="T">Type of filter.</typeparam>
        /// <param name="expr">Expression by which SELECT should be filtered (i.e. WHERE clause).</param> 
        /// <returns>Object.</returns>
        public T SelectFirst<T>(DbExpression expr) where T : class, new()
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");
            if (expr == null) throw new ArgumentNullException(nameof(expr));

            string tableName = _TypeMetadataMgr.GetTableNameFromType(typeof(T));
            string primaryKeyColumnName = _TypeMetadataMgr.GetPrimaryKeyColumnName(typeof(T));
            string primaryKeyPropertyName = _TypeMetadataMgr.GetPrimaryKeyPropertyName(typeof(T));

            Expression e = WatsonORMCommon.DbExpressionConverter(expr);
            e.PrependAnd(primaryKeyColumnName, WatsonORMCommon.DbOperatorsConverter(DbOperators.IsNotNull), null);

            ResultOrder[] resultOrder = new ResultOrder[1];
            resultOrder[0] = new ResultOrder(primaryKeyColumnName, OrderDirection.Ascending);

            DataTable result = _Database.Select(tableName, null, 1, null, e, resultOrder);
            if (result == null || result.Rows.Count < 1) return null;
            return DataTableToObject<T>(result);
        }

        /// <summary>
        /// SELECT multiple rows.
        /// This operation will return an empty list if no matching objects are found.
        /// The ordering used in the underlying query is ascending based on primary key column.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="expr">Expression.</param>
        /// <returns>List of objects.</returns>
        public List<T> SelectMany<T>(DbExpression expr)where T : class, new()
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");
            if (expr == null) throw new ArgumentNullException(nameof(expr));

            string tableName = _TypeMetadataMgr.GetTableNameFromType(typeof(T));
            string primaryKeyColumnName = _TypeMetadataMgr.GetPrimaryKeyColumnName(typeof(T));

            Expression e = WatsonORMCommon.DbExpressionConverter(expr);
            e.PrependAnd(primaryKeyColumnName, WatsonORMCommon.DbOperatorsConverter(DbOperators.IsNotNull), null);

            ResultOrder[] resultOrder = new ResultOrder[1];
            resultOrder[0] = new ResultOrder(primaryKeyColumnName, OrderDirection.Ascending);

            DataTable result = _Database.Select(tableName, null, null, null, e, resultOrder);
            return DataTableToObjectList<T>(result);
        }

        /// <summary>
        /// SELECT multiple rows with pagination.
        /// This operation will return an empty list if no matching objects are found.
        /// The ordering used in the underlying query is ascending based on primary key column.
        /// </summary> 
        /// <param name="indexStart">Index start.</param>
        /// <param name="maxResults">Maximum number of results to retrieve.</param> 
        /// <param name="expr">Filter to apply when SELECTing rows (i.e. WHERE clause).</param>
        /// <returns>List of objects.</returns>
        public List<T> SelectMany<T>(int? indexStart, int? maxResults, DbExpression expr) where T : class, new()
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");
            if (expr == null) throw new ArgumentNullException(nameof(expr));

            string tableName = _TypeMetadataMgr.GetTableNameFromType(typeof(T));
            string primaryKeyColumnName = _TypeMetadataMgr.GetPrimaryKeyColumnName(typeof(T));
            string primaryKeyPropertyName = _TypeMetadataMgr.GetPrimaryKeyPropertyName(typeof(T));

            Expression e = WatsonORMCommon.DbExpressionConverter(expr);
            e.PrependAnd(primaryKeyColumnName, WatsonORMCommon.DbOperatorsConverter(DbOperators.IsNotNull), null);

            DbResultOrder[] resultOrder = new DbResultOrder[1];
            resultOrder[0] = new DbResultOrder(primaryKeyColumnName, DbOrderDirection.Ascending);

            DataTable result = _Database.Select(tableName, indexStart, maxResults, null, e, DbResultOrder.ConvertToResultOrder(resultOrder));
            return DataTableToObjectList<T>(result);
        }

        /// <summary>
        /// SELECT multiple rows with pagination.
        /// This operation will return an empty list if no matching objects are found. 
        /// </summary> 
        /// <param name="indexStart">Index start.</param>
        /// <param name="maxResults">Maximum number of results to retrieve.</param> 
        /// <param name="expr">Filter to apply when SELECTing rows (i.e. WHERE clause).</param>
        /// <param name="resultOrder">Specify on which columns and in which direction results should be ordered.</param>
        /// <returns>List of objects.</returns>
        public List<T> SelectMany<T>(int? indexStart, int? maxResults, DbExpression expr, DbResultOrder[] resultOrder) where T : class, new()
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");
            if (expr == null) throw new ArgumentNullException(nameof(expr));
            if (resultOrder == null || resultOrder.Length < 1) throw new ArgumentNullException(nameof(resultOrder));

            string tableName = _TypeMetadataMgr.GetTableNameFromType(typeof(T));
            string primaryKeyColumnName = _TypeMetadataMgr.GetPrimaryKeyColumnName(typeof(T));
            string primaryKeyPropertyName = _TypeMetadataMgr.GetPrimaryKeyPropertyName(typeof(T));

            Expression e = WatsonORMCommon.DbExpressionConverter(expr);
            e.PrependAnd(primaryKeyColumnName, WatsonORMCommon.DbOperatorsConverter(DbOperators.IsNotNull), null);
             
            DataTable result = _Database.Select(tableName, indexStart, maxResults, null, e, DbResultOrder.ConvertToResultOrder(resultOrder));
            return DataTableToObjectList<T>(result);
        }

        /// <summary>
        /// Retrieve the table name for a given type.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <returns>Table name.</returns>
        public string GetTableName(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            return _TypeMetadataMgr.GetTableNameFromType(type);
        }

        /// <summary>
        /// Retrieve the column name for a given property.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="propName">Property name.</param>
        /// <returns>Column name.</returns>
        public string GetColumnName<T>(string propName)
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");
            if (String.IsNullOrEmpty(propName)) throw new ArgumentNullException(nameof(propName));
            return _TypeMetadataMgr.GetColumnNameForPropertyName<T>(propName);
        }

        /// <summary>
        /// Check if objects of a given type exist that match the supplied expression.
        /// </summary> 
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="expr">Expression.</param>
        /// <returns>True if exists.</returns>
        public bool Exists<T>(DbExpression expr)
        {
            string tableName = _TypeMetadataMgr.GetTableNameFromType(typeof(T));
            Expression e = WatsonORMCommon.DbExpressionConverter(expr);
            return _Database.Exists(tableName, e);
        }

        /// <summary>
        /// Determine the number of objects of a given type that exist that match the supplied expression.
        /// </summary> 
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="expr">Expression.</param>
        /// <returns>Number of matching records.</returns>
        public long Count<T>(DbExpression expr)
        {
            string tableName = _TypeMetadataMgr.GetTableNameFromType(typeof(T));
            Expression e = WatsonORMCommon.DbExpressionConverter(expr);
            return _Database.Count(tableName, e);
        }

        /// <summary>
        /// Add the contents of the specified column from objects that match the supplied expression.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="columnName"></param>
        /// <param name="expr">Expression.</param>
        /// <returns></returns>
        public decimal Sum<T>(string columnName, DbExpression expr)
        {
            if (String.IsNullOrEmpty(columnName)) throw new ArgumentNullException(nameof(columnName));
            string tableName = _TypeMetadataMgr.GetTableNameFromType(typeof(T));
            Expression e = WatsonORMCommon.DbExpressionConverter(expr);
            return _Database.Sum(tableName, columnName, e);
        }

        /// <summary>
        /// Execute a query directly against the database.
        /// </summary>
        /// <param name="query">Query.</param>
        /// <returns>DataTable.</returns>
        public DataTable Query(string query)
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");
            if (String.IsNullOrEmpty(query)) throw new ArgumentNullException(nameof(query));
            return _Database.Query(query);
        }

        /// <summary>
        /// Retrieve a timestamp formatted for the database.
        /// </summary>
        /// <param name="dt">DateTime.</param>
        /// <returns>Formatted DateTime string.</returns>
        public string Timestamp(DateTime dt)
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");
            return _Database.Timestamp(dt);
        }

        /// <summary>
        /// Retrieve a timestamp with offset formatted for the database.
        /// </summary>
        /// <param name="dt">DateTimeOffset.</param>
        /// <returns>Formatted DateTime string.</returns>
        public string TimestampOffset(DateTimeOffset dt)
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");
            return _Database.TimestampOffset(dt);
        }

        /// <summary>
        /// Returns a sanitized string useful in a raw query.
        /// If null is supplied, null is returned.
        /// </summary>
        /// <param name="str">String.</param>
        /// <returns>String.</returns>
        public string Sanitize(string str)
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");
            if (String.IsNullOrEmpty(str)) return null;
            return _Database.SanitizeString(str);
        }

        /// <summary>
        /// Dispose of the object and release background workers.
        /// </summary>
        /// <param name="disposing">Indicate if child resources should be disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            _Logger?.Invoke(_Header + "dispose requested");

            if (disposing)
            {
                if (_Database != null) _Database.Dispose();

                _TokenSource.Cancel();
            }

            _Logger?.Invoke(_Header + "dispose complete");
            _Initialized = false;
        }

        /// <summary>
        /// Convert a DataTable to an object list for initialized tables and types.  
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="table">DataTable.</param>
        /// <returns>List of the specified type.</returns>
        public List<T> DataTableToObjectList<T>(DataTable table) where T : class, new()
        {
            List<T> ret = new List<T>();

            if (table == null) return null;
            if (table.Rows == null || table.Rows.Count < 1) return ret;

            foreach (DataRow row in table.Rows)
            {
                T obj = DataRowToObject<T>(row);
                if (obj != null) ret.Add(obj);
            }

            return ret;
        }

        /// <summary>
        /// Convert a DataTable to an object for an initialized table and type.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="table">DataTable.</param>
        /// <returns>Object of the specified type.</returns>
        public T DataTableToObject<T>(DataTable table) where T : class, new()
        {
            if (table == null || table.Rows == null || table.Rows.Count < 1) return null;
            return DataRowToObject<T>(table.Rows[0]);
        }

        /// <summary>
        /// Convert a DataRow to an object for an initialized table and type.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="row">DataRow.</param>
        /// <returns>Object of the specified type.</returns>
        public T DataRowToObject<T>(DataRow row) where T : class, new()
        {
            if (row == null) return null;

            T ret = new T();
            TypeMetadata md = _TypeMetadataMgr.GetTypeMetadata(typeof(T));

            foreach (DataColumn dc in row.Table.Columns)
            {
                if (md.Columns.Any(c => c.Name.Equals(dc.ColumnName)))
                {
                    Column col = md.Columns.Where(c => c.Name.Equals(dc.ColumnName)).First();
                    object val = row[dc.ColumnName];
                    string propName = _TypeMetadataMgr.GetPropertyNameFromColumnName(typeof(T), dc.ColumnName);
                    if (String.IsNullOrEmpty(propName)) throw new ArgumentException("Unable to find property in type '" + typeof(T).Name + "' for column '" + dc.ColumnName + "'.");

                    PropertyInfo property = typeof(T).GetProperty(propName);
                    if (val != null && val != DBNull.Value)
                    {
                        // Remap the object to the property type since some databases misalign
                        // Example: sqlite uses int64 when it should be int32
                        // Console.WriteLine("| Column data type [" + col.Type.ToString() + "], property data type [" + property.PropertyType.ToString() + "]");

                        Type propType = property.PropertyType;
                        Type underlyingType = Nullable.GetUnderlyingType(propType);

                        if (underlyingType != null)
                        {
                            if (underlyingType == typeof(bool))
                            {
                                property.SetValue(ret, Convert.ToBoolean(val));
                            }
                            else if (underlyingType.IsEnum)
                            {
                                property.SetValue(ret, (Enum.Parse(underlyingType, val.ToString())));
                            }
                            else
                            {
                                property.SetValue(ret, Convert.ChangeType(val, underlyingType));
                            }
                        }
                        else
                        {
                            if (propType == typeof(bool))
                            {
                                property.SetValue(ret, Convert.ToBoolean(val));
                            }
                            else if (propType.IsEnum)
                            {
                                property.SetValue(ret, (Enum.Parse(propType, val.ToString())));
                            }
                            else
                            {
                                property.SetValue(ret, Convert.ChangeType(val, propType));
                            }
                        }
                    }
                    else
                    {
                        property.SetValue(ret, null);
                    }
                }
            }

            return ret;
        }

        #endregion

        #region Private-Conversion-Methods

        private Dictionary<string, object> ObjectToDictionary(object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            string tableName = _TypeMetadataMgr.GetTableNameFromObject(obj);
            if (String.IsNullOrEmpty(tableName)) throw new InvalidOperationException("Type '" + obj.GetType().Name + "' does not have a 'Table' attribute.");
            List<Column> columns = new List<Column>();

            Dictionary<string, object> ret = new Dictionary<string, object>();

            PropertyInfo[] properties = obj.GetType().GetProperties();
            foreach (PropertyInfo prop in properties)
            {
                object[] attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    ColumnAttribute colAttr = attr as ColumnAttribute;
                    if (colAttr != null)
                    {
                        object val = prop.GetValue(obj);
                        if (val != null && !colAttr.PrimaryKey)
                        {
                            switch (colAttr.Type)
                            {
                                case DataTypes.Enum:
                                case DataTypes.Boolean:
                                case DataTypes.Int:
                                    ret.Add(colAttr.Name, Convert.ToInt32(val));
                                    break;
                                case DataTypes.DateTime:
                                    ret.Add(colAttr.Name, _Database.Timestamp(Convert.ToDateTime(val)));
                                    break;
                                case DataTypes.DateTimeOffset:
                                    ret.Add(colAttr.Name, _Database.TimestampOffset((DateTimeOffset)(val)));
                                    break;
                                case DataTypes.Blob:
                                    ret.Add(colAttr.Name, val);
                                    break;
                                case DataTypes.Double:
                                    ret.Add(colAttr.Name, Convert.ToDouble(val));
                                    break;
                                case DataTypes.Decimal:
                                    ret.Add(colAttr.Name, Convert.ToDecimal(val));
                                    break;
                                case DataTypes.Long:
                                    ret.Add(colAttr.Name, Convert.ToInt64(val));
                                    break;
                                case DataTypes.Nvarchar:
                                case DataTypes.Varchar:
                                    ret.Add(colAttr.Name, val.ToString());
                                    break;
                                default:
                                    throw new ArgumentException("Unknown data type '" + colAttr.Type.ToString() + "'.");
                            }
                        }
                        else if (val == null && !colAttr.PrimaryKey)
                        {
                            ret.Add(colAttr.Name, null);
                        }
                    }
                }
            }

            if (ret.Count < 1) throw new InvalidOperationException("Type '" + obj.GetType().Name + "' does not have any properties with a 'Column' attribute.");
            return ret;
        }
         
        #endregion 
    }
}
