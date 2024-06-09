// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace Crazor.Rendering
{
    /// <summary>
    /// Base class for configuration-holding renderer classes.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public abstract class AdaptiveConfigBase
    {
        /// <summary>
        /// Holds additional data in a configuration that doesn't map to known properties.
        /// </summary>
        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; } = new Dictionary<string, JToken>();
    }
}
