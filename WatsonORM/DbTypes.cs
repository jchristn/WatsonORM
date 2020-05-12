using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Watson.ORM
{
    /// <summary>
    /// Enumeration containing the supported database types.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DbTypes
    {
        /// <summary>
        /// Microsoft SQL Server
        /// </summary>
        [EnumMember(Value = "MsSql")]
        MsSql,
        /// <summary>
        /// MySQL
        /// </summary>
        [EnumMember(Value = "MySql")]
        MySql,
        /// <summary>
        /// PostgreSQL
        /// </summary>
        [EnumMember(Value = "PgSql")]
        PgSql,
        /// <summary>
        /// Sqlite
        /// </summary>
        [EnumMember(Value = "Sqlite")]
        Sqlite
    } 
}
