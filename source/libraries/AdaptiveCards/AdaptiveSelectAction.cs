// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace AdaptiveCards
{
    /// <summary>
    /// Represents how a card can be refreshed by making a request to the target Bot
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
#if !NETSTANDARD1_3
    [XmlType(TypeName = "SelectAction")]
#endif
    [JsonConverter(typeof(AdaptiveSelectActionConverter))]
    public class AdaptiveSelectAction
    {
        /// <summary>
        ///    The action to be executed to refresh the card.
        ///    Clients can run this refresh action automatically or can provide an affordance for users to trigger it manually.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
#if !NETSTANDARD1_3
        [XmlElement(typeof(AdaptiveShowCardAction))]
        [XmlElement(typeof(AdaptiveExecuteAction))]
        [XmlElement(typeof(AdaptiveSubmitAction))]
        [XmlElement(typeof(AdaptiveOpenUrlAction))]
        [XmlElement(typeof(AdaptiveToggleVisibilityAction))]
        [XmlElement(typeof(AdaptiveUnknownAction))]
#endif
        public AdaptiveAction Action { get; set; }
    }
}
