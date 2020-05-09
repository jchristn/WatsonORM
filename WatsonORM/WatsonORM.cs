using System;
using DatabaseWrapper;

namespace Watson.ORM
{
    /// <summary>
    /// WatsonORM, a lightweight and easy to use object-relational mapper (ORM).
    /// </summary>
    public class WatsonORM
    {
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

        private Action<string> _Logger = null;
        private string _Header = "[WatsonORM] ";
        private DatabaseSettings _Settings = null;
        private DatabaseClient _Database = null;

        /// <summary>
        /// Instantiate the object.  Once constructed, call Initialize().
        /// </summary>
        /// <param name="settings">Database settings.</param>
        public WatsonORM(DatabaseSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            _Settings = settings; 
        }

        /// <summary>
        /// Initialize the database client.
        /// If the client is already initialized, it will first be disposed.
        /// </summary>
        public void Initialize()
        {
            if (_Database != null)
            {
                _Logger?.Invoke(_Header + "disposing existing database client");
                _Database.Dispose();
            }

            switch (_Settings.Type)
            {
                case DbTypes.MsSql:
                case DbTypes.MySql:
                case DbTypes.PgSql:
                    _Logger?.Invoke(_Header + "initializing database client: " + _Settings.Type.ToString() + " on " + _Settings.Hostname + ":" + _Settings.Port);
                    _Database = new DatabaseClient(
                        _Settings.Type,
                        _Settings.Hostname,
                        _Settings.Port,
                        _Settings.Username,
                        _Settings.Password,
                        _Settings.Instance,
                        _Settings.DatabaseName);
                    break;
                case DbTypes.Sqlite:
                    _Logger?.Invoke(_Header + "initializing database client: " + _Settings.Type.ToString() + " in file " + _Settings.Filename);
                    _Database = new DatabaseClient(_Settings.Filename);
                    break;
                default:
                    throw new InvalidOperationException("Unsupported database type: " + _Settings.Type.ToString());
            }

            _Logger?.Invoke(_Header + "initialization complete");
        }
    }
}
