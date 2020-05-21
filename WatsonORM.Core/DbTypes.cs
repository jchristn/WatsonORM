using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Watson.ORM.Core
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
        [EnumMember(Value = "SqlServer")]
        SqlServer,
        /// <summary>
        /// MySQL
        /// </summary>
        [EnumMember(Value = "Mysql")]
        Mysql,
        /// <summary>
        /// PostgreSQL
        /// </summary>
        [EnumMember(Value = "Postgresql")]
        Postgresql,
        /// <summary>
        /// Sqlite
        /// </summary>
        [EnumMember(Value = "Sqlite")]
        Sqlite
    } 
}
