using System;
using System.Collections.Generic;
using System.Text;

namespace Watson.ORM
{
    /// <summary>
    /// Links a class to a WatsonORM-managed database table.
    /// </summary>
    public class TableAttribute : Attribute
    {
        public string TableName 
        { 
            get
            {
                return _TableName;
            }
        }

        private string _TableName = null;

        /// <summary>
        /// Links a class to a WatsonORM-managed database table.
        /// </summary>
        /// <param name="tableName">The name of the corresponding database table.</param>
        public TableAttribute(string tableName)
        {
            if (String.IsNullOrEmpty(tableName)) throw new ArgumentNullException(nameof(tableName));

            _TableName = tableName;
        } 
    }
}
