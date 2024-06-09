// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Crazor
{
    /// <summary>
    /// AdaptiveCard element with content alignment properties
    /// </summary>
    public abstract class AdaptiveCollectionWithContentAlignment : AdaptiveCollectionElement
    {


        /// <summary>
        /// The content alignment for the TableCells inside the TableRow.
        /// </summary>
        [JsonProperty("verticalCellContentAlignment", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [XmlAttribute]
        [DefaultValue(typeof(AdaptiveVerticalContentAlignment), "top")]
        public AdaptiveVerticalContentAlignment VerticalCellContentAlignment { get; set; }

        /// <summary>
        /// The content alignment for the TableCells inside the TableRow.
        /// </summary>
        [JsonProperty("horizontalCellContentAlignment", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [XmlAttribute]
        [DefaultValue(typeof(AdaptiveHorizontalContentAlignment), "left")]
        public AdaptiveHorizontalContentAlignment HorizontalCellContentAlignment { get; set; }
    }
}
