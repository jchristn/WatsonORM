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
    /// Type of data contained in the column.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DataTypes
    {
        /// <summary>
        /// Variable-length character.
        /// </summary>
        [EnumMember(Value = "Varchar")]
        Varchar,
        /// <summary>
        /// Variable-length unicode character.
        /// </summary>
        [EnumMember(Value = "Nvarchar")]
        Nvarchar,
        /// <summary>
        /// Integer.
        /// </summary>
        [EnumMember(Value = "Int")]
        Int,
        /// <summary>
        /// Boolean.
        /// </summary>
        [EnumMember(Value = "Boolean")]
        Boolean,
        /// <summary>
        /// Enum.
        /// </summary>
        [EnumMember(Value = "Enum")]
        Enum,
        /// <summary>
        /// Long
        /// </summary>
        [EnumMember(Value = "Long")]
        Long,
        /// <summary>
        /// Decimal
        /// </summary>
        [EnumMember(Value = "Decimal")]
        Decimal,
        /// <summary>
        /// Double
        /// </summary>
        [EnumMember(Value = "Double")]
        Double,
        /// <summary>
        /// Timestamp
        /// </summary>
        [EnumMember(Value = "DateTime")]
        DateTime,
        /// <summary>
        /// Timestamp with offset
        /// </summary>
        [EnumMember(Value = "DateTimeOffset")]
        DateTimeOffset,
        /// <summary>
        /// Blob
        /// </summary>
        [EnumMember(Value = "Blob")]
        Blob
    }
}
