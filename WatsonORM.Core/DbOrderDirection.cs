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
    /// Direction by which results should be returned.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))] 
    public enum DbOrderDirection
    {
        /// <summary>
        /// Return results in ascending order.
        /// </summary>
        [EnumMember(Value = "Ascending")]
        Ascending,
        /// <summary>
        /// Return results in descending order.
        /// </summary>
        [EnumMember(Value = "Descending")]
        Descending
    }
}
