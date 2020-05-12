using System;
using System.Collections.Generic;
using System.Text;
using DatabaseWrapper;

namespace Watson.ORM
{
    internal class TypeMetadata
    {
        internal string TableName { get; set; }
        internal string PrimaryKeyPropertyName { get; set; }
        internal List<Column> Columns { get; set; }

        internal TypeMetadata(string tableName, string primaryKeyPropertyName, List<Column> columns)
        {
            TableName = tableName;
            PrimaryKeyPropertyName = primaryKeyPropertyName;
            Columns = columns;
        }
    }
}
