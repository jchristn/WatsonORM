using System;
using System.Collections.Generic;
using System.Text;
using DatabaseWrapper.Core;

namespace Watson.ORM.Core
{
    /// <summary>
    /// Metadata about a given type.
    /// </summary>
    public class TypeMetadata
    {
        /// <summary>
        /// The name of the database table.
        /// </summary>
        public string TableName { get; set; }
        
        /// <summary>
        /// Property name of the primary key field.
        /// </summary>
        public string PrimaryKeyPropertyName { get; set; }

        /// <summary>
        /// Columns in the table.
        /// </summary>
        public List<Column> Columns { get; set; }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="tableName">The name of the database table.</param>
        /// <param name="primaryKeyPropertyName">Property name of the primary key field.</param>
        /// <param name="columns">Columns in the table.</param>
        public TypeMetadata(string tableName, string primaryKeyPropertyName, List<Column> columns)
        {
            TableName = tableName;
            PrimaryKeyPropertyName = primaryKeyPropertyName;
            Columns = columns;
        }
    }
}
