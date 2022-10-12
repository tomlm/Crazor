// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AdaptiveCards
{
    /// <summary>
    /// Represents a TableRow structure in a table
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
#if !NETSTANDARD1_3
    [XmlType(TypeName = TypeName)]
#endif
    public class AdaptiveTableRow 
    {
        public const string TypeName = "TableRow";


        /// <inheritdoc/>
        [JsonProperty(Order = -10, DefaultValueHandling = DefaultValueHandling.Include)]
#if !NETSTANDARD1_3
        [XmlIgnore]
#endif
        public string Type { get; set; } = TypeName;

        /// <summary>
        /// Initializes an empty Fact.
        /// </summary>
        public AdaptiveTableRow()
        {
        }

        /// <summary>
        /// Initializes a Fact with the given properties.
        /// </summary>
        /// <param name="title">The title of the Fact.</param>
        /// <param name="value">The value of the Fact.</param>
        public AdaptiveTableRow(string title, string value)
        {
        }

        /// <summary>
        /// The amount of space the element should be separated from the previous element. Default value is <see cref="AdaptiveSpacing.Default"/>.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
#if !NETSTANDARD1_3
        [XmlAttribute]
#endif
        [DefaultValue(typeof(AdaptiveSpacing), "default")]
        public AdaptiveSpacing Spacing { get; set; }

        /// <summary>
        /// Indicates whether there should be a visible separator (e.g. a line) between this element and the one before it.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
#if !NETSTANDARD1_3
        [XmlAttribute]
#endif
        [DefaultValue(false)]
        public bool Separator { get; set; }

        /// <summary>
        /// The style used to display this element. See <see cref="AdaptiveContainerStyle" />.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
#if !NETSTANDARD1_3
        [XmlAttribute]
#endif
        [DefaultValue(typeof(AdaptiveContainerStyle), "default")]
        public AdaptiveContainerStyle Style { get; set; }

        /// <summary>
        /// Indicates whether the element should be visible when the card has been rendered.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
#if !NETSTANDARD1_3
        [XmlAttribute]
#endif
        [DefaultValue(true)]
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// Determines how to align the content horizontally.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
#if !NETSTANDARD1_3
        [XmlAttribute]
#endif
        [DefaultValue(typeof(AdaptiveHorizontalAlignment), "left")]
        public AdaptiveHorizontalAlignment HorizontalAlignment { get; set; }

        /// <summary>
        /// Determines how to align the content horizontally.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
#if !NETSTANDARD1_3
        [XmlAttribute]
#endif
        [DefaultValue(typeof(AdaptiveHorizontalAlignment), "left")]
        public AdaptiveHorizontalAlignment HorizontalCellContentAlignment { get; set; }

        /// <summary>
        /// Determines how to align the content horizontally.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
#if !NETSTANDARD1_3
        [XmlAttribute]
#endif
        [DefaultValue(typeof(AdaptiveVerticalAlignment), "top")]
        public AdaptiveVerticalAlignment VerticalCellContentAlignment { get; set; }

        [JsonProperty]
#if !NETSTANDARD1_3
        [XmlElement("TableCell")]
#endif
        public List<AdaptiveTableCell> Cells { get; set; } = new List<AdaptiveTableCell>();
    }
}
