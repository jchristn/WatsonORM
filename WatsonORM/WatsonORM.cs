using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DatabaseWrapper;
using DatabaseWrapper.Core;
using ExpressionTree;
using Watson.ORM.Core;
using Watson.ORM.Mysql;
using Watson.ORM.Postgresql;
using Watson.ORM.Sqlite;
using Watson.ORM.SqlServer;

namespace Watson.ORM
{
    /// <summary>
    /// WatsonORM, a lightweight and easy to use object-relational mapper (ORM).
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

                if (_Mysql != null) _Mysql.Logger = _Logger;
                else if (_Postgresql != null) _Postgresql.Logger = _Logger; 
                else if (_Sqlite != null) _Sqlite.Logger = _Logger; 
                else if (_SqlServer != null) _SqlServer.Logger = _Logger; 
                else
                {
                    throw new InvalidOperationException("No database client is initialized.");
                }
            }
        }

        /// <summary>
        /// Direct access to the underlying database client.
        /// </summary>
        public object Database
        {
            get
            {
                if (_Mysql != null)
                {
                    return _Mysql.Database;
                }
                if (_Postgresql != null)
                {
                    return _Postgresql.Database;
                }
                if (_Sqlite != null)
                {
                    return _Sqlite.Database;
                }
                if (_SqlServer != null)
                {
                    return _SqlServer.Database;
                }
                else
                {
                    throw new InvalidOperationException("No database client is initialized.");
                }
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
        private string _Header = "[WatsonORM] ";
        private CancellationTokenSource _TokenSource = new CancellationTokenSource();
        private CancellationToken _Token;
        private DatabaseSettings _Settings = null;

        private Watson.ORM.Mysql.WatsonORM _Mysql = null;
        private Watson.ORM.Postgresql.WatsonORM _Postgresql = null;
        private Watson.ORM.Sqlite.WatsonORM _Sqlite = null;
        private Watson.ORM.SqlServer.WatsonORM _SqlServer = null;
          
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
            if (_Mysql != null)
            {
                _Mysql.Dispose();
            }
            else if (_Postgresql != null)
            {
                _Postgresql.Dispose();
            }
            else if (_Sqlite != null)
            {
                _Sqlite.Dispose();
            }
            else if (_SqlServer != null)
            {
                _SqlServer.Dispose();
            }

            _Logger?.Invoke(_Header + "initializing database client: " + _Settings.Type.ToString() + " on " + _Settings.Hostname + ":" + _Settings.Port);

            switch (_Settings.Type)
            {
                case DbTypes.Mysql:
                    _Mysql = new Mysql.WatsonORM(_Settings);
                    _Mysql.InitializeDatabase();
                    break;
                case DbTypes.Postgresql:
                    _Postgresql = new Postgresql.WatsonORM(_Settings);
                    _Postgresql.InitializeDatabase();
                    break;
                case DbTypes.SqlServer:
                    _SqlServer = new SqlServer.WatsonORM(_Settings);
                    _SqlServer.InitializeDatabase();
                    break;
                case DbTypes.Sqlite:
                    _Sqlite = new Sqlite.WatsonORM(_Settings);
                    _Sqlite.InitializeDatabase();
                    break;
                default:
                    throw new InvalidOperationException("Unsupported database type: " + _Settings.Type.ToString());
            }

            _Logger?.Invoke(_Header + "initialization complete");

            _Initialized = true;
        }

        /// <summary>
        /// Validate tables to determine if any errors or warnings exist.
        /// </summary>
        /// <param name="types">List of classes for which tables should be validated.</param>
        /// <param name="errors">List of human-readable errors.</param>
        /// <param name="warnings">List of human-readable warnings.</param>
        /// <returns>True if the table will initialize successfully.</returns>
        public bool ValidateTables(List<Type> types, out List<string> errors, out List<string> warnings)
        {
            if (types == null) throw new ArgumentNullException(nameof(types));

            errors = new List<string>();
            warnings = new List<string>();

            if (_Mysql != null)
            {
                return _Mysql.ValidateTables(types, out errors, out warnings);
            }
            else if (_Postgresql != null)
            {
                return _Postgresql.ValidateTables(types, out errors, out warnings);
            }
            else if (_SqlServer != null)
            {
                return _SqlServer.ValidateTables(types, out errors, out warnings);
            }
            else if (_Sqlite != null)
            {
                return _Sqlite.ValidateTables(types, out errors, out warnings);
            }
            else
            {
                throw new InvalidOperationException("Unsupported database type: " + _Settings.Type.ToString());
            }
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

            if (_Mysql != null)
            {
                return _Mysql.ValidateTable(t, out errors, out warnings);
            }
            else if (_Postgresql != null)
            {
                return _Postgresql.ValidateTable(t, out errors, out warnings);
            }
            else if (_SqlServer != null)
            {
                return _SqlServer.ValidateTable(t, out errors, out warnings);
            }
            else if (_Sqlite != null)
            {
                return _Sqlite.ValidateTable(t, out errors, out warnings);
            }
            else
            {
                throw new InvalidOperationException("Unsupported database type: " + _Settings.Type.ToString());
            }
        }

        /// <summary>
        /// Create tables (if they don't exist) for a given set of classes.
        /// Adding a table that has already been added will throw an ArgumentException.
        /// </summary>
        /// <param name="types">List of classes for which tables should be created.</param>
        public void InitializeTables(List<Type> types)
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");

            if (_Mysql != null)
            {
                _Mysql.InitializeTables(types);
            }
            else if (_Postgresql != null)
            {
                _Postgresql.InitializeTables(types);
            }
            else if (_SqlServer != null)
            {
                _SqlServer.InitializeTables(types);
            }
            else if (_Sqlite != null)
            {
                _Sqlite.InitializeTables(types);
            }
            else
            {
                throw new InvalidOperationException("Unsupported database type: " + _Settings.Type.ToString());
            }
        }

        /// <summary>
        /// Create table (if it doesn't exist) for a given class.
        /// Adding a table that has already been added will throw an ArgumentException.
        /// </summary>
        /// <param name="t">Class for which a table should be created.</param>
        public void InitializeTable(Type t)
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");

            if (_Mysql != null)
            {
                _Mysql.InitializeTable(t);
            }
            else if (_Postgresql != null)
            {
                _Postgresql.InitializeTable(t);
            }
            else if (_SqlServer != null)
            {
                _SqlServer.InitializeTable(t);
            }
            else if (_Sqlite != null)
            {
                _Sqlite.InitializeTable(t);
            }
            else
            {
                throw new InvalidOperationException("Unsupported database type: " + _Settings.Type.ToString());
            }
        }

        /// <summary>
        /// Drop table if it exists for a given class.
        /// </summary>
        /// <param name="t">Class for which a table should be dropped.</param>
        public void DropTable(Type t)
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");

            if (_Mysql != null)
            {
                _Mysql.DropTable(t);
            }
            else if (_Postgresql != null)
            {
                _Postgresql.DropTable(t);
            }
            else if (_SqlServer != null)
            {
                _SqlServer.DropTable(t);
            }
            else if (_Sqlite != null)
            {
                _Sqlite.DropTable(t);
            }
            else
            {
                throw new InvalidOperationException("Unsupported database type: " + _Settings.Type.ToString());
            }
        }

        /// <summary>
        /// Truncate table if it exists for a given class.
        /// </summary>
        /// <param name="t">Class for which a table should be dropped.</param>
        public void TruncateTable(Type t)
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");

            if (_Mysql != null)
            {
                _Mysql.TruncateTable(t);
            }
            else if (_Postgresql != null)
            {
                _Postgresql.TruncateTable(t);
            }
            else if (_SqlServer != null)
            {
                _SqlServer.TruncateTable(t);
            }
            else if (_Sqlite != null)
            {
                _Sqlite.TruncateTable(t);
            }
            else
            {
                throw new InvalidOperationException("Unsupported database type: " + _Settings.Type.ToString());
            }
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

            if (_Mysql != null)
            {
                return _Mysql.Insert<T>(obj);
            }
            else if (_Postgresql != null)
            {
                return _Postgresql.Insert<T>(obj); 
            }
            else if (_SqlServer != null)
            {
                return _SqlServer.Insert<T>(obj);
            }
            else if (_Sqlite != null)
            {
                return _Sqlite.Insert<T>(obj);
            }
            else
            {
                throw new InvalidOperationException("Unsupported database type: " + _Settings.Type.ToString());
            }
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

            if (_Mysql != null)
            {
                return _Mysql.InsertMany<T>(objs);
            }
            else if (_Postgresql != null)
            {
                return _Postgresql.InsertMany<T>(objs);
            }
            else if (_SqlServer != null)
            {
                return _SqlServer.InsertMany<T>(objs);
            }
            else if (_Sqlite != null)
            {
                return _Sqlite.InsertMany<T>(objs);
            }
            else
            {
                throw new InvalidOperationException("Unsupported database type: " + _Settings.Type.ToString());
            }
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

            if (_Mysql != null)
            {
                _Mysql.InsertMultiple<T>(objs);
            }
            else if (_Postgresql != null)
            {
                _Postgresql.InsertMultiple<T>(objs);
            }
            else if (_SqlServer != null)
            {
                _SqlServer.InsertMultiple<T>(objs);
            }
            else if (_Sqlite != null)
            {
                _Sqlite.InsertMultiple<T>(objs);
            }
            else
            {
                throw new InvalidOperationException("Unsupported database type: " + _Settings.Type.ToString());
            }

            return;
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

            if (_Mysql != null)
            {
                return _Mysql.Update<T>(obj);
            }
            else if (_Postgresql != null)
            {
                return _Postgresql.Update<T>(obj);
            }
            else if (_SqlServer != null)
            {
                return _SqlServer.Update<T>(obj);
            }
            else if (_Sqlite != null)
            {
                return _Sqlite.Update<T>(obj);
            }
            else
            {
                throw new InvalidOperationException("Unsupported database type: " + _Settings.Type.ToString());
            }
        }

        /// <summary>
        /// UPDATE multiple rows.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="expr">Expression.</param>
        /// <param name="updateVals">Update values.</param>
        public void UpdateMany<T>(Expr expr, Dictionary<string, object> updateVals)
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");

            if (_Mysql != null)
            {
                _Mysql.UpdateMany<T>(expr, updateVals);
            }
            else if (_Postgresql != null)
            {
                _Postgresql.UpdateMany<T>(expr, updateVals);
            }
            else if (_SqlServer != null)
            {
                _SqlServer.UpdateMany<T>(expr, updateVals);
            }
            else if (_Sqlite != null)
            {
                _Sqlite.UpdateMany<T>(expr, updateVals);
            }
            else
            {
                throw new InvalidOperationException("Unsupported database type: " + _Settings.Type.ToString());
            }
        }

        /// <summary>
        /// DELETE an object.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="obj">Object to DELETE.</param>
        public void Delete<T>(T obj) where T : class, new()
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");

            if (_Mysql != null)
            {
                _Mysql.Delete<T>(obj);
            }
            else if (_Postgresql != null)
            {
                _Postgresql.Delete<T>(obj);
            }
            else if (_SqlServer != null)
            {
                _SqlServer.Delete<T>(obj);
            }
            else if (_Sqlite != null)
            {
                _Sqlite.Delete<T>(obj);
            }
            else
            {
                throw new InvalidOperationException("Unsupported database type: " + _Settings.Type.ToString());
            }
        }

        /// <summary>
        /// DELETE an object by its primary key.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="id">Id value.</param>
        public void DeleteByPrimaryKey<T>(object id) where T : class, new()
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");

            if (_Mysql != null)
            {
                _Mysql.DeleteByPrimaryKey<T>(id);
            }
            else if (_Postgresql != null)
            {
                _Postgresql.DeleteByPrimaryKey<T>(id);
            }
            else if (_SqlServer != null)
            {
                _SqlServer.DeleteByPrimaryKey<T>(id);
            }
            else if (_Sqlite != null)
            {
                _Sqlite.DeleteByPrimaryKey<T>(id);
            }
            else
            {
                throw new InvalidOperationException("Unsupported database type: " + _Settings.Type.ToString());
            }
        }

        /// <summary>
        /// DELETE objects by an Expression..
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="expr">Expression.</param>
        public void DeleteMany<T>(Expr expr)where T : class, new()
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");

            if (_Mysql != null)
            {
                _Mysql.DeleteMany<T>(expr);
            }
            else if (_Postgresql != null)
            {
                _Postgresql.DeleteMany<T>(expr);
            }
            else if (_SqlServer != null)
            {
                _SqlServer.DeleteMany<T>(expr);
            }
            else if (_Sqlite != null)
            {
                _Sqlite.DeleteMany<T>(expr);
            }
            else
            {
                throw new InvalidOperationException("Unsupported database type: " + _Settings.Type.ToString());
            }
        }

        /// <summary>
        /// SELECT an object by id.
        /// This operation will return null if the object does not exist.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="id">Id.</param>
        /// <returns>Object.</returns>
        public T SelectByPrimaryKey<T>(object id)where T : class, new()
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");

            if (_Mysql != null)
            {
                return _Mysql.SelectByPrimaryKey<T>(id);
            }
            else if (_Postgresql != null)
            {
                return _Postgresql.SelectByPrimaryKey<T>(id);
            }
            else if (_SqlServer != null)
            {
                return _SqlServer.SelectByPrimaryKey<T>(id);
            }
            else if (_Sqlite != null)
            {
                return _Sqlite.SelectByPrimaryKey<T>(id);
            }
            else
            {
                throw new InvalidOperationException("Unsupported database type: " + _Settings.Type.ToString());
            }
        }

        /// <summary>
        /// SELECT the first instance of an object matching a given expression.
        /// This operation will return null if the object does not exist.
        /// </summary>
        /// <typeparam name="T">Type of filter.</typeparam>
        /// <param name="expr">Expression by which SELECT should be filtered (i.e. WHERE clause).</param> 
        /// <param name="ro">Result ordering, if not set, results will be ordered ascending by primary key.</param>
        /// <returns>Object.</returns>
        public T SelectFirst<T>(Expr expr, ResultOrder[] ro = null) where T : class, new()
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");

            if (_Mysql != null)
            {
                return _Mysql.SelectFirst<T>(expr, ro);
            }
            else if (_Postgresql != null)
            {
                return _Postgresql.SelectFirst<T>(expr, ro);
            }
            else if (_SqlServer != null)
            {
                return _SqlServer.SelectFirst<T>(expr, ro);
            }
            else if (_Sqlite != null)
            {
                return _Sqlite.SelectFirst<T>(expr, ro);
            }
            else
            {
                throw new InvalidOperationException("Unsupported database type: " + _Settings.Type.ToString());
            }
        }

        /// <summary>
        /// SELECT multiple rows.
        /// This operation will return an empty list if no matching objects are found.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="expr">Expression.</param>
        /// <param name="ro">Result ordering, if not set, results will be ordered ascending by primary key.</param>
        /// <returns>List of objects.</returns>
        public List<T> SelectMany<T>(Expr expr, ResultOrder[] ro = null) where T : class, new()
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");

            if (_Mysql != null)
            {
                return _Mysql.SelectMany<T>(expr, ro);
            }
            else if (_Postgresql != null)
            {
                return _Postgresql.SelectMany<T>(expr, ro);
            }
            else if (_SqlServer != null)
            {
                return _SqlServer.SelectMany<T>(expr, ro);
            }
            else if (_Sqlite != null)
            {
                return _Sqlite.SelectMany<T>(expr, ro);
            }
            else
            {
                throw new InvalidOperationException("Unsupported database type: " + _Settings.Type.ToString());
            }
        }

        /// <summary>
        /// SELECT multiple rows with pagination.
        /// This operation will return an empty list if no matching objects are found.
        /// </summary> 
        /// <param name="indexStart">Index start.</param>
        /// <param name="maxResults">Maximum number of results to retrieve.</param> 
        /// <param name="expr">Filter to apply when SELECTing rows (i.e. WHERE clause).</param>
        /// <param name="ro">Result ordering, if not set, results will be ordered ascending by primary key.</param>
        /// <returns>List of objects.</returns>
        public List<T> SelectMany<T>(int? indexStart, int? maxResults, Expr expr, ResultOrder[] ro = null) where T : class, new()
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");

            if (_Mysql != null)
            {
                return _Mysql.SelectMany<T>(indexStart, maxResults, expr, ro);
            }
            else if (_Postgresql != null)
            {
                return _Postgresql.SelectMany<T>(indexStart, maxResults, expr, ro);
            }
            else if (_SqlServer != null)
            {
                return _SqlServer.SelectMany<T>(indexStart, maxResults, expr, ro);
            }
            else if (_Sqlite != null)
            {
                return _Sqlite.SelectMany<T>(indexStart, maxResults, expr, ro);
            }
            else
            {
                throw new InvalidOperationException("Unsupported database type: " + _Settings.Type.ToString());
            }
        }

        /// <summary>
        /// Retrieve the table name for a given type.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <returns>Table name.</returns>
        public string GetTableName(Type type)
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");

            if (_Mysql != null)
            {
                return _Mysql.GetTableName(type);
            }
            else if (_Postgresql != null)
            {
                return _Postgresql.GetTableName(type);
            }
            else if (_SqlServer != null)
            {
                return _SqlServer.GetTableName(type);
            }
            else if (_Sqlite != null)
            {
                return _Sqlite.GetTableName(type);
            }
            else
            {
                throw new InvalidOperationException("Unsupported database type: " + _Settings.Type.ToString());
            }
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

            if (_Mysql != null)
            {
                return _Mysql.GetColumnName<T>(propName);
            }
            else if (_Postgresql != null)
            {
                return _Postgresql.GetColumnName<T>(propName);
            }
            else if (_SqlServer != null)
            {
                return _SqlServer.GetColumnName<T>(propName);
            }
            else if (_Sqlite != null)
            {
                return _Sqlite.GetColumnName<T>(propName);
            }
            else
            {
                throw new InvalidOperationException("Unsupported database type: " + _Settings.Type.ToString());
            }
        }

        /// <summary>
        /// Check if objects of a given type exist that match the supplied expression.
        /// </summary> 
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="expr">Expression.</param>
        /// <returns>True if exists.</returns>
        public bool Exists<T>(Expr expr)
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");

            if (_Mysql != null)
            {
                return _Mysql.Exists<T>(expr);
            }
            else if (_Postgresql != null)
            {
                return _Postgresql.Exists<T>(expr);
            }
            else if (_SqlServer != null)
            {
                return _SqlServer.Exists<T>(expr);
            }
            else if (_Sqlite != null)
            {
                return _Sqlite.Exists<T>(expr);
            }
            else
            {
                throw new InvalidOperationException("Unsupported database type: " + _Settings.Type.ToString());
            }
        }

        /// <summary>
        /// Determine the number of objects of a given type that exist that match the supplied expression.
        /// </summary> 
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="expr">Expression.</param>
        /// <returns>Number of matching records.</returns>
        public long Count<T>(Expr expr)
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");

            if (_Mysql != null)
            {
                return _Mysql.Count<T>(expr);
            }
            else if (_Postgresql != null)
            {
                return _Postgresql.Count<T>(expr);
            }
            else if (_SqlServer != null)
            {
                return _SqlServer.Count<T>(expr);
            }
            else if (_Sqlite != null)
            {
                return _Sqlite.Count<T>(expr);
            }
            else
            {
                throw new InvalidOperationException("Unsupported database type: " + _Settings.Type.ToString());
            }
        }

        /// <summary>
        /// Add the contents of the specified column from objects that match the supplied expression.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="columnName"></param>
        /// <param name="expr">Expression.</param>
        /// <returns></returns>
        public decimal Sum<T>(string columnName, Expr expr)
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");

            if (_Mysql != null)
            {
                return _Mysql.Sum<T>(columnName, expr);
            }
            else if (_Postgresql != null)
            {
                return _Postgresql.Sum<T>(columnName, expr);
            }
            else if (_SqlServer != null)
            {
                return _SqlServer.Sum<T>(columnName, expr);
            }
            else if (_Sqlite != null)
            {
                return _Sqlite.Sum<T>(columnName, expr);
            }
            else
            {
                throw new InvalidOperationException("Unsupported database type: " + _Settings.Type.ToString());
            }
        }

        /// <summary>
        /// Execute a query directly against the database.
        /// </summary>
        /// <param name="query">Query.</param>
        /// <returns>DataTable.</returns>
        public DataTable Query(string query)
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");  

            if (_Mysql != null)
            {
                return _Mysql.Query(query);
            }
            else if (_Postgresql != null)
            {
                return _Postgresql.Query(query);
            }
            else if (_SqlServer != null)
            {
                return _SqlServer.Query(query);
            }
            else if (_Sqlite != null)
            {
                return _Sqlite.Query(query);
            }
            else
            {
                throw new InvalidOperationException("Unsupported database type: " + _Settings.Type.ToString());
            }
        }

        /// <summary>
        /// Retrieve a timestamp formatted for the database.
        /// </summary>
        /// <param name="dt">DateTime.</param>
        /// <returns>Formatted DateTime string.</returns>
        public string Timestamp(DateTime dt)
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");

            if (_Mysql != null)
            {
                return _Mysql.Timestamp(dt);
            }
            else if (_Postgresql != null)
            {
                return _Postgresql.Timestamp(dt);
            }
            else if (_SqlServer != null)
            {
                return _SqlServer.Timestamp(dt);
            }
            else if (_Sqlite != null)
            {
                return _Sqlite.Timestamp(dt);
            }
            else
            {
                throw new InvalidOperationException("Unsupported database type: " + _Settings.Type.ToString());
            }
        }

        /// <summary>
        /// Retrieve a timestamp with offset formatted for the database.
        /// </summary>
        /// <param name="dt">DateTimeOffset.</param>
        /// <returns>Formatted DateTime string.</returns>
        public string TimestampOffset(DateTimeOffset dt)
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");

            if (_Mysql != null)
            {
                return _Mysql.TimestampOffset(dt);
            }
            else if (_Postgresql != null)
            {
                return _Postgresql.TimestampOffset(dt);
            }
            else if (_SqlServer != null)
            {
                return _SqlServer.TimestampOffset(dt);
            }
            else if (_Sqlite != null)
            {
                return _Sqlite.TimestampOffset(dt);
            }
            else
            {
                throw new InvalidOperationException("Unsupported database type: " + _Settings.Type.ToString());
            }
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

            if (_Mysql != null)
            {
                return _Mysql.Sanitize(str);
            }
            else if (_Postgresql != null)
            {
                return _Postgresql.Sanitize(str);
            }
            else if (_SqlServer != null)
            {
                return _SqlServer.Sanitize(str);
            }
            else if (_Sqlite != null)
            {
                return _Sqlite.Sanitize(str);
            }
            else
            {
                throw new InvalidOperationException("Unsupported database type: " + _Settings.Type.ToString());
            }
        }

        /// <summary>
        /// Dispose of the object and release background workers.
        /// </summary>
        /// <param name="disposing">Indicate if child resources should be disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_Mysql != null)
            {
                _Mysql.Dispose();
            }
            else if (_Postgresql != null)
            {
                _Postgresql.Dispose();
            }
            else if (_SqlServer != null)
            {
                _SqlServer.Dispose();
            }
            else if (_Sqlite != null)
            {
                _Sqlite.Dispose();
            }
            else
            {
                throw new InvalidOperationException("Unsupported database type: " + _Settings.Type.ToString());
            }

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
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");

            if (_Mysql != null)
            {
                return _Mysql.DataTableToObjectList<T>(table);
            }
            else if (_Postgresql != null)
            {
                return _Postgresql.DataTableToObjectList<T>(table);
            }
            else if (_SqlServer != null)
            {
                return _SqlServer.DataTableToObjectList<T>(table);
            }
            else if (_Sqlite != null)
            {
                return _Sqlite.DataTableToObjectList<T>(table);
            }
            else
            {
                throw new InvalidOperationException("Unsupported database type: " + _Settings.Type.ToString());
            }
        }

        /// <summary>
        /// Convert a DataTable to an object for an initialized table and type.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="table">DataTable.</param>
        /// <returns>Object of the specified type.</returns>
        public T DataTableToObject<T>(DataTable table) where T : class, new()
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");

            if (_Mysql != null)
            {
                return _Mysql.DataTableToObject<T>(table);
            }
            else if (_Postgresql != null)
            {
                return _Postgresql.DataTableToObject<T>(table);
            }
            else if (_SqlServer != null)
            {
                return _SqlServer.DataTableToObject<T>(table);
            }
            else if (_Sqlite != null)
            {
                return _Sqlite.DataTableToObject<T>(table);
            }
            else
            {
                throw new InvalidOperationException("Unsupported database type: " + _Settings.Type.ToString());
            }
        }

        /// <summary>
        /// Convert a DataRow to an object for an initialized table and type.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="row">DataRow.</param>
        /// <returns>Object of the specified type.</returns>
        public T DataRowToObject<T>(DataRow row) where T : class, new()
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");

            if (_Mysql != null)
            {
                return _Mysql.DataRowToObject<T>(row);
            }
            else if (_Postgresql != null)
            {
                return _Postgresql.DataRowToObject<T>(row);
            }
            else if (_SqlServer != null)
            {
                return _SqlServer.DataRowToObject<T>(row);
            }
            else if (_Sqlite != null)
            {
                return _Sqlite.DataRowToObject<T>(row);
            }
            else
            {
                throw new InvalidOperationException("Unsupported database type: " + _Settings.Type.ToString());
            }
        }

        #endregion
    }
}
