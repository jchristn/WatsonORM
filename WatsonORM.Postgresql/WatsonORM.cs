using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DatabaseWrapper.Postgresql;
using DatabaseWrapper.Core;
using Watson.ORM.Core;

namespace Watson.ORM.Postgresql
{
    /// <summary>
    /// WatsonORM for PostgreSQL, a lightweight and easy to use object-relational mapper (ORM).
    /// </summary>
    public class WatsonORM : IDisposable
    {
        #region Public-Members

        /// <summary>
        /// Database settings.
        /// </summary>
        public DatabaseSettings Settings
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

        #endregion

        #region Private-Members

        private Action<string> _Logger = null;
        private DebugSettings _Debug = new DebugSettings();
        private string _Header = "[WatsonORM] ";
        private CancellationTokenSource _TokenSource = new CancellationTokenSource();
        private CancellationToken _Token;
        private DatabaseSettings _Settings = null;
        private DatabaseClient _Database = null; 
        private readonly object _MetadataLock = new object();
        private Dictionary<Type, TypeMetadata> _Metadata = new Dictionary<Type, TypeMetadata>();

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.  Once constructed, call InitializeDatabase() and InitializeTable() for each table if needed.
        /// </summary>
        /// <param name="settings">Database settings.</param>
        public WatsonORM(DatabaseSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            _Settings = settings;
            _Token = _TokenSource.Token;

            if (_Settings.Type != Core.DbTypes.Postgresql) throw new ArgumentException("Database type in settings must be of type 'Postgresql'.");
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
                _Settings.DatabaseName); 

            _Logger?.Invoke(_Header + "initialization complete");
        }

        /// <summary>
        /// Create table (if it doesn't exist) for a given class.
        /// </summary>
        /// <param name="t">Class for which a table should be created.</param>
        public void InitializeTable(Type t)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));

            string tableName = ReflectionHelper.GetTableNameFromType(t);
            string primaryKeyPropertyName = ReflectionHelper.GetPrimaryKeyPropertyName(t);
            List<Column> columns = ReflectionHelper.GetColumnsFromType(t);

            if (String.IsNullOrEmpty(tableName)) 
                throw new InvalidOperationException("Type '" + t.Name + "' does not have a 'Table' attribute.");

            if (String.IsNullOrEmpty(primaryKeyPropertyName))
                throw new InvalidOperationException("Type '" + t.Name + "' does not have a property with the 'PrimaryKey' attribute.");

            if (columns == null || columns.Count < 1) 
                throw new InvalidOperationException("Type '" + t.Name + "' does not have any properties with a 'Column' attribute.");

            if (!_Database.TableExists(tableName)) _Database.CreateTable(tableName, columns);
            _Metadata.Add(t, new TypeMetadata(tableName, primaryKeyPropertyName, columns));

            _Logger?.Invoke(_Header + "initialized table " + tableName + " for type " + t.Name + " with " + columns.Count + " column(s)");
        }

        /// <summary>
        /// Drop table if it exists for a given class.
        /// </summary>
        /// <param name="t">Class for which a table should be dropped.</param>
        public void DropTable(Type t)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));

            string tableName = GetTableNameFromType(t);
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
            if (t == null) throw new ArgumentNullException(nameof(t));

            string tableName = GetTableNameFromType(t);
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
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            string tableName = GetTableNameFromObject(obj); 
            string primaryKeyColumnName = GetPrimaryKeyColumnName(typeof(T));
            string primaryKeyPropertyName = GetPrimaryKeyPropertyName(typeof(T)); 

            Dictionary<string, object> insertVals = ObjectToDictionary(obj);
            DataTable result = _Database.Insert(tableName, insertVals);
            return DataTableToObject<T>(result);
        }

        /// <summary>
        /// INSERT multiple records.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="objs">List of objects.</param>
        /// <returns>List of objects.</returns>
        public List<T> InsertMany<T>(List<T> objs) where T : class, new()
        {
            if (objs == null || objs.Count < 1) throw new ArgumentNullException(nameof(objs));

            string tableName = GetTableNameFromType(typeof(T));
            string primaryKeyColumnName = GetPrimaryKeyColumnName(typeof(T));
            string primaryKeyPropertyName = GetPrimaryKeyPropertyName(typeof(T));

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
        /// UPDATE an object.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="obj">Object to UPDATE.</param>
        /// <returns>UPDATEd object.</returns>
        public T Update<T>(T obj) where T : class, new()
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            string tableName = GetTableNameFromObject(obj); 
            string primaryKeyColumnName = GetPrimaryKeyColumnName(typeof(T));
            string primaryKeyPropertyName = GetPrimaryKeyPropertyName(typeof(T)); 
            object primaryKeyValue = GetPrimaryKeyValue(obj, primaryKeyPropertyName);
            
            Dictionary<string, object> updateVals = ObjectToDictionary(obj);
            Expression e = new Expression(primaryKeyColumnName, DbOperatorsConverter(DbOperators.Equals), primaryKeyValue);
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
            if (expr == null) throw new ArgumentNullException(nameof(expr));
            if (updateVals == null || updateVals.Count < 1) throw new ArgumentNullException(nameof(updateVals));

            string tableName = GetTableNameFromType(typeof(T));
            string primaryKeyColumnName = GetPrimaryKeyColumnName(typeof(T));

            Expression e = DbExpressionConverter(expr);
            e.PrependAnd(primaryKeyColumnName, DbOperatorsConverter(DbOperators.IsNotNull), null);

            _Database.Update(tableName, updateVals, e);
        }

        /// <summary>
        /// DELETE an object.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="obj">Object to DELETE.</param>
        public void Delete<T>(T obj) where T : class, new()
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            string tableName = GetTableNameFromObject(obj); 
            string primaryKeyColumnName = GetPrimaryKeyColumnName(typeof(T));
            string primaryKeyPropertyName = GetPrimaryKeyPropertyName(typeof(T)); 
            object primaryKeyValue = GetPrimaryKeyValue(obj, primaryKeyPropertyName);
            
            Expression e = new Expression(primaryKeyColumnName, DbOperatorsConverter(DbOperators.Equals), primaryKeyValue);
            _Database.Delete(tableName, e);
        }

        /// <summary>
        /// DELETE an object by its primary key.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="id">Id value.</param>
        public void DeleteByPrimaryKey<T>(object id) where T : class, new()
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            string tableName = GetTableNameFromType(typeof(T)); 
            string primaryKeyColumnName = GetPrimaryKeyColumnName(typeof(T));
            string primaryKeyPropertyName = GetPrimaryKeyPropertyName(typeof(T));

            Expression e = new Expression(primaryKeyColumnName, DbOperatorsConverter(DbOperators.Equals), id);
            _Database.Delete(tableName, e);
        }

        /// <summary>
        /// DELETE objects by an Expression..
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="expr">Expression.</param>
        public void DeleteMany<T>(DbExpression expr) where T : class, new()
        {
            if (expr == null) throw new ArgumentNullException(nameof(expr)); 
            string tableName = GetTableNameFromType(typeof(T)); 
            _Database.Delete(tableName, DbExpressionConverter(expr));
        }

        /// <summary>
        /// SELECT an object by id.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="id">Id.</param>
        /// <returns>Object.</returns>
        public T SelectByPrimaryKey<T>(object id) where T : class, new()
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            string tableName = GetTableNameFromType(typeof(T));
            string primaryKeyColumnName = GetPrimaryKeyColumnName(typeof(T));
            string primaryKeyPropertyName = GetPrimaryKeyPropertyName(typeof(T));

            Expression e = new Expression(primaryKeyColumnName, DbOperatorsConverter(DbOperators.Equals), id);
            DataTable result = _Database.Select(tableName, null, null, null, e, null);
            return DataTableToObject<T>(result);
        }

        /// <summary>
        /// SELECT an object using a filter.
        /// </summary>
        /// <typeparam name="T">Type of filter.</typeparam>
        /// <param name="expr">Expression by which SELECT should be filtered (i.e. WHERE clause).</param> 
        /// <returns>Object.</returns>
        public T SelectFirst<T>(DbExpression expr) where T : class, new()
        {
            if (expr == null) throw new ArgumentNullException(nameof(expr));

            string tableName = GetTableNameFromType(typeof(T));
            string primaryKeyColumnName = GetPrimaryKeyColumnName(typeof(T));
            string primaryKeyPropertyName = GetPrimaryKeyPropertyName(typeof(T));

            Expression e = DbExpressionConverter(expr);
            e.PrependAnd(primaryKeyColumnName, DbOperatorsConverter(DbOperators.IsNotNull), null);
            string orderByClause = "ORDER BY " + primaryKeyColumnName + " ASC";
            DataTable result = _Database.Select(tableName, null, 1, null, e, orderByClause);
            if (result == null || result.Rows.Count < 1) return null;
            return DataTableToObject<T>(result);
        }

        /// <summary>
        /// SELECT multiple rows.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="expr">Expression.</param>
        /// <returns>List of objects.</returns>
        public List<T> SelectMany<T>(DbExpression expr) where T : class, new()
        {
            if (expr == null) throw new ArgumentNullException(nameof(expr));

            string tableName = GetTableNameFromType(typeof(T)); 
            string primaryKeyColumnName = GetPrimaryKeyColumnName(typeof(T));

            Expression e = DbExpressionConverter(expr);
            e.PrependAnd(primaryKeyColumnName, DbOperatorsConverter(DbOperators.IsNotNull), null);
            string orderByClause = "ORDER BY " + primaryKeyColumnName + " ASC";

            DataTable result = _Database.Select(tableName, null, null, null, e, orderByClause);
            return DataTableToObjectList<T>(result);
        }

        /// <summary>
        /// SELECT multiple rows.
        /// </summary> 
        /// <param name="indexStart">Index start.</param>
        /// <param name="maxResults">Maximum number of results to retrieve.</param> 
        /// <param name="expr">Filter to apply when SELECTing rows (i.e. WHERE clause).</param>
        /// <returns>List of objects.</returns>
        public List<T> SelectMany<T>(int? indexStart, int? maxResults, DbExpression expr) where T : class, new()
        {
            if (expr == null) throw new ArgumentNullException(nameof(expr));

            string tableName = GetTableNameFromType(typeof(T)); 
            string primaryKeyColumnName = GetPrimaryKeyColumnName(typeof(T));
            string primaryKeyPropertyName = GetPrimaryKeyPropertyName(typeof(T));

            Expression e = DbExpressionConverter(expr);
            e.PrependAnd(primaryKeyColumnName, DbOperatorsConverter(DbOperators.IsNotNull), null);
            string orderByClause = "ORDER BY " + primaryKeyColumnName + " ASC";

            DataTable result = _Database.Select(tableName, indexStart, maxResults, null, e, orderByClause);
            return DataTableToObjectList<T>(result);
        }

        /// <summary>
        /// Retrieve the column name for a given property.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="propName">Property name.</param>
        /// <returns>Column name.</returns>
        public string GetColumnName<T>(string propName)
        {
            if (String.IsNullOrEmpty(propName)) throw new ArgumentNullException(nameof(propName));
            return GetColumnNameForPropertyName<T>(propName);
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
        }

        #endregion

        #region Private-Conversion-Methods

        private Operators DbOperatorsConverter(DbOperators oper)
        {
            switch (oper)
            {
                case DbOperators.And:
                    return Operators.And;
                case DbOperators.Or:
                    return Operators.Or;
                case DbOperators.Equals:
                    return Operators.Equals;
                case DbOperators.NotEquals:
                    return Operators.NotEquals;
                case DbOperators.In:
                    return Operators.In;
                case DbOperators.NotIn:
                    return Operators.NotIn;
                case DbOperators.Contains:
                    return Operators.Contains;
                case DbOperators.ContainsNot:
                    return Operators.ContainsNot;
                case DbOperators.StartsWith:
                    return Operators.StartsWith;
                case DbOperators.EndsWith:
                    return Operators.EndsWith;
                case DbOperators.GreaterThan:
                    return Operators.GreaterThan;
                case DbOperators.GreaterThanOrEqualTo:
                    return Operators.GreaterThanOrEqualTo;
                case DbOperators.LessThan:
                    return Operators.LessThan;
                case DbOperators.LessThanOrEqualTo:
                    return Operators.LessThanOrEqualTo;
                case DbOperators.IsNull:
                    return Operators.IsNull;
                case DbOperators.IsNotNull:
                    return Operators.IsNotNull;
                default:
                    throw new ArgumentException("Unknown operator '" + oper.ToString() + "'.");
            }
        }

        private Expression DbExpressionConverter(DbExpression expr)
        {
            if (expr == null) return null;
              
            return new Expression(
                (expr.LeftTerm is DbExpression ? DbExpressionConverter((DbExpression)expr.LeftTerm) : expr.LeftTerm),
                DbOperatorsConverter(expr.Operator),
                (expr.RightTerm is DbExpression ? DbExpressionConverter((DbExpression)expr.RightTerm) : expr.RightTerm));
        }

        private Dictionary<string, object> ObjectToDictionary(object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            string tableName = GetTableNameFromObject(obj); 
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
                        if (!colAttr.PrimaryKey)
                        {
                            switch (colAttr.Type)
                            {
                                case DataTypes.Enum:
                                case DataTypes.Boolean:
                                case DataTypes.Int:
                                    ret.Add(colAttr.Name, Convert.ToInt32(prop.GetValue(obj)));
                                    break;
                                case DataTypes.DateTime:
                                    ret.Add(colAttr.Name, _Database.Timestamp(Convert.ToDateTime(prop.GetValue(obj))));
                                    break;
                                case DataTypes.Blob:
                                    ret.Add(colAttr.Name, prop.GetValue(obj));
                                    break;
                                case DataTypes.Double:
                                    ret.Add(colAttr.Name, Convert.ToDouble(prop.GetValue(obj)));
                                    break;
                                case DataTypes.Decimal:
                                    ret.Add(colAttr.Name, Convert.ToDecimal(prop.GetValue(obj)));
                                    break;
                                case DataTypes.Long: 
                                    ret.Add(colAttr.Name, Convert.ToInt64(prop.GetValue(obj)));
                                    break;
                                case DataTypes.Nvarchar:
                                case DataTypes.Varchar:
                                    ret.Add(colAttr.Name, prop.GetValue(obj).ToString());
                                    break;
                                default:
                                    throw new ArgumentException("Unknown data type '" + colAttr.Type.ToString() + "'.");
                            }                            
                        }
                    }
                }
            }

            if (ret.Count < 1) throw new InvalidOperationException("Type '" + obj.GetType().Name + "' does not have any properties with a 'Column' attribute.");
            return ret;
        }

        private List<T> DataTableToObjectList<T>(DataTable table) where T : class, new()
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

        private T DataTableToObject<T>(DataTable table) where T : class, new()
        {
            if (table == null || table.Rows == null || table.Rows.Count < 1) return null;
            return DataRowToObject<T>(table.Rows[0]);
        }

        private T DataRowToObject<T>(DataRow row) where T : class, new()
        {
            if (row == null) return null;

            T ret = new T();
            TypeMetadata md = GetTypeMetadata(typeof(T));
             
            foreach (DataColumn dc in row.Table.Columns)
            {
                if (md.Columns.Any(c => c.Name.Equals(dc.ColumnName)))
                {
                    Column col = md.Columns.Where(c => c.Name.Equals(dc.ColumnName)).First(); 
                    object val = row[dc.ColumnName];
                    string propName = GetPropertyNameFromColumnName(typeof(T), dc.ColumnName);
                    if (String.IsNullOrEmpty(propName)) throw new ArgumentException("Unable to find property in type '" + typeof(T).Name + "' for column '" + dc.ColumnName + "'.");

                    PropertyInfo property = typeof(T).GetProperty(propName);
                    if (val != null)
                    {
                        // Remap the object to the property type since some databases misalign
                        // Example: sqlite uses int64 when it should be int32
                        // Console.WriteLine("| Column data type [" + col.DataType.ToString() + "], property data type [" + property.PropertyType.ToString() + "]");

                        if (property.PropertyType == typeof(bool))
                        {
                            property.SetValue(ret, Convert.ToBoolean(val));
                        }
                        else if (property.PropertyType.IsEnum)
                        {
                            Type propType = property.PropertyType;
                            property.SetValue(ret, (Enum.Parse(property.PropertyType, val.ToString())));
                        }
                        else
                        {
                            property.SetValue(ret, Convert.ChangeType(val, property.PropertyType));
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
         
        private DataType DbTypeConverter(DataTypes dt)
        {
            switch (dt)
            {
                case DataTypes.Varchar:
                    return DataType.Varchar;
                case DataTypes.Nvarchar:
                    return DataType.Nvarchar;
                case DataTypes.Int:
                    return DataType.Int;
                case DataTypes.Long:
                    return DataType.Long;
                case DataTypes.Decimal:
                    return DataType.Decimal;
                case DataTypes.Double:
                    return DataType.Double;
                case DataTypes.DateTime:
                    return DataType.DateTime;
                case DataTypes.Blob:
                    return DataType.Blob;
                default:
                    throw new ArgumentException("Unknown data type '" + dt.ToString() + "'.");
            }
        }

        #endregion

        #region Private-Metadata-Methods

        private TypeMetadata GetTypeMetadata(Type t)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));

            lock (_MetadataLock)
            {
                if (_Metadata.ContainsKey(t)) return _Metadata[t];
            }

            throw new InvalidOperationException("Type '" + t.Name + "' has not been initialized into the ORM.");
        }

        private string GetTableNameFromType(Type t)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));

            TypeMetadata md = GetTypeMetadata(t);

            return md.TableName;
        }

        private string GetTableNameFromObject(object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            return GetTableNameFromType(obj.GetType());
        }

        private string GetPrimaryKeyColumnName(Type t)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));

            TypeMetadata md = GetTypeMetadata(t);
            
            foreach (Column col in md.Columns)
            {
                if (col.PrimaryKey) return col.Name;
            }
            
            throw new InvalidOperationException("Type '" + t.Name + "' does not have a property with the 'PrimaryKey' attribute."); 
        }

        private string GetPrimaryKeyPropertyName(Type t)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));

            TypeMetadata md = GetTypeMetadata(t);

            return md.PrimaryKeyPropertyName; 
        }

        private object GetPrimaryKeyValue(object obj, string propName)
        {
            object val = GetPropertyValue(obj, propName);
            if (val == null) throw new InvalidOperationException("Property '" + propName + "' cannot be null if decorated as a primary key.");
            return val;
        }

        private object GetPropertyValue(object obj, string propName)
        {
            return obj.GetType().GetProperty(propName).GetValue(obj, null);
        }

        private string GetColumnNameForPropertyName<T>(string propName)
        {
            if (String.IsNullOrEmpty(propName)) throw new ArgumentNullException(nameof(propName));

            PropertyInfo[] properties = typeof(T).GetProperties();
            foreach (PropertyInfo prop in properties)
            {
                if (prop.Name.Equals(propName))
                {
                    object[] attrs = prop.GetCustomAttributes(true);
                    foreach (object attr in attrs)
                    {
                        ColumnAttribute colAttr = attr as ColumnAttribute;
                        if (colAttr != null)
                        {
                            return colAttr.Name;
                        }
                    }
                }
            }

            throw new ArgumentException("No 'Column' attribute found for property name '" + propName + "'.");
        }
         
        private string GetPropertyNameFromColumnName(Type t, string columnName)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));
            if (String.IsNullOrEmpty(columnName)) throw new ArgumentNullException(nameof(columnName));

            PropertyInfo[] properties = t.GetProperties();
            foreach (PropertyInfo prop in properties)
            {
                object[] attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    ColumnAttribute colAttr = attr as ColumnAttribute;
                    if (colAttr != null)
                    {
                        if (colAttr.Name.Equals(columnName)) return prop.Name;
                    }
                }
            }

            throw new ArgumentException("No property found for column name '" + columnName + "'.");
        }

        #endregion
    }
}
