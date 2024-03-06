// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
#if !NETSTANDARD1_3
using System.Xml.Serialization;
#endif 

namespace Crazor.AdaptiveCards
{
    /// <summary>
    /// Represents an unknown element encountered while parsing.
    /// </summary>
#if !NETSTANDARD1_3
    [XmlType(TypeName = AdaptiveUnknownElement.TypeName)]
#endif
    public class AdaptiveUnknownElement : AdaptiveElement
    {
        /// <inheritdoc />
        public const string TypeName = "UnknownElement";

        /// <inheritdoc />
        [JsonProperty]
#if !NETSTANDARD1_3
        [XmlAttribute]
#endif
        public override string Type { get; set; }
    }

    /// <summary>
    /// Represents an unknown action encountered while parsing.
    /// </summary>
#if !NETSTANDARD1_3
    [XmlType(TypeName = "Action.Unknown")]
#endif
    public class AdaptiveUnknownAction : AdaptiveAction
    {
        /// <inheritdoc />
        [JsonProperty(Order = -10, Required = Required.Always, DefaultValueHandling = DefaultValueHandling.Include)]
#if !NETSTANDARD1_3
        [XmlAttribute]
#endif 
        public override string Type { get; set; } 
    }
}
