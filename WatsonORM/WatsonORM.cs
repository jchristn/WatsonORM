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

                    if (_Mysql != null)
                    {
                        _Mysql.Debug = null;
                    }
                    else if (_Postgresql != null)
                    {
                        _Postgresql.Debug = null;
                    }
                    else if (_Sqlite != null)
                    {
                        _Sqlite.Debug = null;
                    }
                    else if (_SqlServer != null)
                    {
                        _SqlServer.Debug = null;
                    }
                    else
                    {
                        throw new InvalidOperationException("No database client is initialized.");
                    }
                }
                else
                {
                    _Debug = value;

                    if (_Mysql != null)
                    {
                        _Mysql.Debug = value;
                    }
                    else if (_Postgresql != null)
                    {
                        _Postgresql.Debug = value;
                    }
                    else if (_Sqlite != null)
                    {
                        _Sqlite.Debug = value;
                    }
                    else if (_SqlServer != null)
                    {
                        _SqlServer.Debug = value;
                    }
                    else
                    {
                        throw new InvalidOperationException("No database client is initialized.");
                    }
                }
            }
        }

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

                if (_Mysql != null)
                {
                    _Mysql.Logger = value;
                }
                else if (_Postgresql != null)
                {
                    _Postgresql.Logger = value;
                }
                else if (_Sqlite != null)
                {
                    _Sqlite.Logger = value;
                }
                else if (_SqlServer != null)
                {
                    _SqlServer.Logger = value;
                }
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
        private DebugSettings _Debug = new DebugSettings();
        private string _Header = "[WatsonORM] ";
        private CancellationTokenSource _TokenSource = new CancellationTokenSource();
        private CancellationToken _Token;
        private Watson.ORM.Core.DatabaseSettings _Settings = null;

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
        public WatsonORM(Watson.ORM.Core.DatabaseSettings settings)
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
                case Core.DbTypes.Mysql:
                    _Mysql = new Mysql.WatsonORM(_Settings);
                    _Mysql.InitializeDatabase();
                    break;
                case Core.DbTypes.Postgresql:
                    _Postgresql = new Postgresql.WatsonORM(_Settings);
                    _Postgresql.InitializeDatabase();
                    break;
                case Core.DbTypes.SqlServer:
                    _SqlServer = new SqlServer.WatsonORM(_Settings);
                    _SqlServer.InitializeDatabase();
                    break;
                case Core.DbTypes.Sqlite:
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
        public void UpdateMany<T>(DbExpression expr, Dictionary<string, object> updateVals)
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
        public void DeleteMany<T>(DbExpression expr) where T : class, new()
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
        public T SelectByPrimaryKey<T>(object id) where T : class, new()
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
        /// <returns>Object.</returns>
        public T SelectFirst<T>(DbExpression expr) where T : class, new()
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");

            if (_Mysql != null)
            {
                return _Mysql.SelectFirst<T>(expr);
            }
            else if (_Postgresql != null)
            {
                return _Postgresql.SelectFirst<T>(expr);
            }
            else if (_SqlServer != null)
            {
                return _SqlServer.SelectFirst<T>(expr);
            }
            else if (_Sqlite != null)
            {
                return _Sqlite.SelectFirst<T>(expr);
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
        /// <returns>List of objects.</returns>
        public List<T> SelectMany<T>(DbExpression expr) where T : class, new()
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");

            if (_Mysql != null)
            {
                return _Mysql.SelectMany<T>(expr);
            }
            else if (_Postgresql != null)
            {
                return _Postgresql.SelectMany<T>(expr);
            }
            else if (_SqlServer != null)
            {
                return _SqlServer.SelectMany<T>(expr);
            }
            else if (_Sqlite != null)
            {
                return _Sqlite.SelectMany<T>(expr);
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
        /// <returns>List of objects.</returns>
        public List<T> SelectMany<T>(int? indexStart, int? maxResults, DbExpression expr) where T : class, new()
        {
            if (!_Initialized) throw new InvalidOperationException("Initialize WatsonORM and database using the .InitializeDatabase() method first.");

            if (_Mysql != null)
            {
                return _Mysql.SelectMany<T>(indexStart, maxResults, expr);
            }
            else if (_Postgresql != null)
            {
                return _Postgresql.SelectMany<T>(indexStart, maxResults, expr);
            }
            else if (_SqlServer != null)
            {
                return _SqlServer.SelectMany<T>(indexStart, maxResults, expr);
            }
            else if (_Sqlite != null)
            {
                return _Sqlite.SelectMany<T>(indexStart, maxResults, expr);
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
                return DatabaseWrapper.Mysql.DatabaseClient.DbTimestamp(dt);
            }
            else if (_Postgresql != null)
            {
                return DatabaseWrapper.Postgresql.DatabaseClient.DbTimestamp(dt);
            }
            else if (_SqlServer != null)
            {
                return DatabaseWrapper.SqlServer.DatabaseClient.DbTimestamp(dt);
            }
            else if (_Sqlite != null)
            {
                return DatabaseWrapper.Sqlite.DatabaseClient.DbTimestamp(dt);
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

        #endregion
    }
}
