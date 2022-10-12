// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace AdaptiveCards
{
    /// <summary>
    /// Represents a Cell in a row in a table
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    [JsonConverter(typeof(ActivatorConverter<AdaptiveTableCell>))]
#if !NETSTANDARD1_3
    [XmlType(TypeName = TypeName)]
#endif
    public class AdaptiveTableCell : AdaptiveContainer
    {
        /// <inheritdoc />
        public new const string TypeName = "TableCell";

        /// <summary>
        /// Initializes an empty Fact.
        /// </summary>
        public AdaptiveTableCell()
        {
        }

        /// <inheritdoc/>
#if !NETSTANDARD1_3
        [XmlIgnore]
#endif
        public override string Type { get; set; } = TypeName;
    }
}
