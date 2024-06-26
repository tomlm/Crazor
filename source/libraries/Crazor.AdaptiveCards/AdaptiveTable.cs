// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Crazor
{
    /// <summary>
    /// Represents a table
    /// </summary>

    [XmlType(TypeName = AdaptiveTable.TypeName)]
    public class AdaptiveTable : AdaptiveCollectionWithContentAlignment
    {
        /// <inheritdoc />
        public const string TypeName = "Table";

        /// <summary>
        /// type name
        /// </summary>
        public override string Type { get; set; } = TypeName;

        /// <summary>
        /// Collection of TableRows 
        /// </summary>
        [XmlElement(Type = typeof(AdaptiveTableRow), ElementName = AdaptiveTableRow.TypeName)]
        public List<AdaptiveTableRow> Rows { get; set; } = new List<AdaptiveTableRow>();

        /// <summary>
        /// Collection of TableColumnDefinitions
        /// </summary>
        [XmlElement(Type = typeof(AdaptiveTableColumnDefinition), ElementName = AdaptiveTableColumnDefinition.TypeName)]
        public List<AdaptiveTableColumnDefinition> Columns { get; set; } = new List<AdaptiveTableColumnDefinition>();

        /// <summary>
        /// Return a enumerator for TableRows
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<AdaptiveElement> GetEnumerator()
        {
            return Rows.GetEnumerator();
        }

        /// <summary>
        /// Add an AdpativeTableRow
        /// </summary>
        /// <param name="element"></param>
        public override void Add(AdaptiveElement element)
        {
            if (element is AdaptiveTableRow)
            {
                Rows.Add(element as AdaptiveTableRow);
            }
        }

        /// <summary>
        /// Defines the style of the grid. This property currently only controls the grid’s color 
        /// </summary>
        [JsonConverter(typeof(IgnoreNullEnumConverter<AdaptiveContainerStyle>), true)]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [XmlIgnore]
        [DefaultValue(null)]
        public AdaptiveContainerStyle? GridStyle { get; set; }

        /// <summary>
        /// Controls XML serialization of style.
        /// </summary>
        // The XML serializer doesn't handle nullable value types. This allows serialization if non-null.
        [JsonIgnore]
        [XmlAttribute("GridStyle")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public AdaptiveContainerStyle GridStyleXml { get { return (GridStyle.HasValue) ? GridStyle.Value : AdaptiveContainerStyle.Default; } set { GridStyle = value; } }

        /// <summary>
        /// Determines whether to serialize the style for XML.
        /// </summary>
        public bool ShouldSerializeGridStyleXml() => this.GridStyle.HasValue;

        /// <summary>
        /// Specifies whether grid lines should be displayed.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [XmlAttribute]
        [DefaultValue(true)]
        public bool ShowGridLines { get; set; } = true;

        /// <summary>
        /// Specifies whether the first row of the table should be treated as a header row, and be announced as such by accessibility software.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [XmlAttribute]
        [DefaultValue(true)]
        public bool FirstRowAsHeaders { get; set; } = true;
    }
}
