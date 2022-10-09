// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace AdaptiveCards
{
    /// <summary>
    /// Represents the Column element.
    /// </summary>
#if !NETSTANDARD1_3
    [XmlType(TypeName = AdaptiveColumn.TypeName)]
#endif
    public class AdaptiveColumn : AdaptiveContainer
    {
        /// <inheritdoc />
        public new const string TypeName = "Column";

        /// <inheritdoc />
#if !NETSTANDARD1_3
        [XmlIgnore]
#endif
        [JsonProperty(Required = Required.Default)]
        public override string Type { get; set; } = TypeName;

        /// <summary>
        /// Size for the column (either ColumnSize string or number which is relative size of the column).
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [Obsolete("Column.Size has been deprecated.  Use Column.Width", false)]
        public string Size { get; set; }

        /// <summary>
        /// Width for the column (either ColumnWidth string or number which is relative size of the column).
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
#if !NETSTANDARD1_3
        [XmlIgnore]
#endif
        public AdaptiveColumnWidth Width { get; set; }
        
        /// <summary>
        /// Xml Serialization for complex type of AdaptiveColumnWidth
        /// </summary>
        [JsonIgnore]
#if !NETSTANDARD1_3
        [XmlAttribute(nameof(Width))]
#endif
        public string WidthXml { get => Width?.ToString(); set => this.Width = new AdaptiveColumnWidth(value); }

        /// <summary>
        /// Ignore Xml Serialization for complex type of AdaptiveColumnWidth when null
        /// </summary>
        public bool ShouldSerializeWidthXml() => Width != null;
    }
}
