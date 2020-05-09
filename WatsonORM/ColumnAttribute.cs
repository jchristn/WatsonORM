using System;
using System.Collections.Generic;
using System.Text;

namespace Watson.ORM
{
    /// <summary>
    /// Links a class property to a column in a WatsonORM-managed database table.
    /// </summary>
    public class ColumnAttribute : Attribute
    {
        public string ColumnName 
        { 
            get
            {
                return _ColumnName;
            }
        }

        /// <summary>
        /// Links a class property to a column in a WatsonORM-managed database table.
        /// </summary>
        /// <param name="columnName">The name of the corresponding table column.</param>
        public ColumnAttribute(string columnName)
        {
            if (String.IsNullOrEmpty(columnName)) throw new ArgumentNullException(nameof(columnName));

            _ColumnName = columnName;
        }

        private string _ColumnName = null;
    }
}
