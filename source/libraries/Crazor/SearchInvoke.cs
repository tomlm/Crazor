// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace Crazor
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class SearchInvoke : SearchInvokeValue
    {
        [JsonProperty]
        public string? Dataset { get; set; }
    }

}
