using System;
using System.Collections.Generic;
using System.Text;

namespace Watson.ORM.Core
{
    /// <summary>
    /// Debug settings.
    /// </summary>
    public class DebugSettings
    {
        #region Public-Members

        /// <summary>
        /// Enable or disable debugging of database queries.
        /// </summary>
        public bool DatabaseQueries { get; set; } = false;

        /// <summary>
        /// Enable or disable debugging of database results.
        /// </summary>
        public bool DatabaseResults { get; set; } = false;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public DebugSettings()
        {

        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="dbQueries">Enable or disable debugging of database queries.</param>
        /// <param name="dbResults">Enable or disable debugging of database results.</param>
        public DebugSettings(bool dbQueries, bool dbResults)
        {
            DatabaseQueries = dbQueries;
            DatabaseResults = dbResults;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
