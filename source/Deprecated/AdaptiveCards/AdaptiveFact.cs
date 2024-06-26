// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Crazor.AdaptiveCards
{
    /// <summary>
    /// Represents a "fact" in a FactSet element.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
#if !NETSTANDARD1_3
    [XmlType(TypeName = "Fact")]
#endif
    public class AdaptiveFact
    {
        /// <summary>
        /// Initializes an empty Fact.
        /// </summary>
        public AdaptiveFact()
        { }

        /// <summary>
        /// Initializes a Fact with the given properties.
        /// </summary>
        /// <param name="title">The title of the Fact.</param>
        /// <param name="value">The value of the Fact.</param>
        public AdaptiveFact(string title, string value)
        {
            Title = title;
            Value = value;
        }

        /// <summary>
        /// The Fact's title.
        /// </summary>
        [JsonRequired]
#if !NETSTANDARD1_3
        [XmlAttribute]
#endif
        public string Title { get; set; }

        /// <summary>
        /// The Fact's value.
        /// </summary>
        [JsonRequired]
#if !NETSTANDARD1_3
        [XmlAttribute]
#endif
        public string Value { get; set; }

        /// <summary>
        /// (Optional) Specifies what should be spoken for this entire element. This is simple text or SSML fragment.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [Obsolete("FactSet.Speak has been deprecated.  Use AdaptiveCard.Speak", false)]
#if !NETSTANDARD1_3
        [XmlAttribute]
#endif
        [DefaultValue(null)]
        public string Speak { get; set; }
    }
}
