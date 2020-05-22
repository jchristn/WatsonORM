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
        /// <summary>
        /// Enable or disable debugging of database queries.
        /// </summary>
        public bool DatabaseQueries { get; set; }

        /// <summary>
        /// Enable or disable debugging of database results.
        /// </summary>
        public bool DatabaseResults { get; set; }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public DebugSettings()
        {
            DatabaseQueries = false;
            DatabaseResults = false;
        }
    }
}
