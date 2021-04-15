using System;
using System.Collections.Generic;
using System.Text;

namespace Watson.ORM.Core
{
    /// <summary>
    /// Links a class to a WatsonORM-managed database table.
    /// </summary>
    public class TableAttribute : Attribute
    {
        #region Public-Members

        /// <summary>
        /// Table name.
        /// </summary>
        public string TableName 
        { 
            get
            {
                return _TableName;
            }
        }

        #endregion

        #region Private-Members

        private string _TableName = null;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Links a class to a WatsonORM-managed database table.
        /// </summary>
        /// <param name="tableName">The name of the corresponding database table.</param>
        public TableAttribute(string tableName)
        {
            if (String.IsNullOrEmpty(tableName)) throw new ArgumentNullException(nameof(tableName));

            _TableName = tableName;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
