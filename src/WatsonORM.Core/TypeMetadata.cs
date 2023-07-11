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
        #region Public-Members

        /// <summary>
        /// The name of the database table.
        /// </summary>
        public string TableName { get; private set; } = null;

        /// <summary>
        /// Property name of the primary key field.
        /// </summary>
        public string PrimaryKeyPropertyName { get; private set; } = null;

        /// <summary>
        /// Columns in the table.
        /// </summary>
        public List<DatabaseWrapper.Core.Column> Columns { get; private set; } = new List<Column>();

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="tableName">The name of the database table.</param>
        /// <param name="primaryKeyPropertyName">Property name of the primary key field.</param>
        /// <param name="columns">Columns in the table.</param>
        public TypeMetadata(string tableName, string primaryKeyPropertyName, List<Column> columns)
        {
            if (String.IsNullOrEmpty(tableName)) throw new ArgumentNullException(nameof(tableName));
            if (String.IsNullOrEmpty(primaryKeyPropertyName)) throw new ArgumentNullException(nameof(primaryKeyPropertyName));
            if (columns == null) throw new ArgumentNullException(nameof(columns));

            TableName = tableName;
            PrimaryKeyPropertyName = primaryKeyPropertyName;
            Columns = columns;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
