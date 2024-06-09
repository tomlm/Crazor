using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Crazor
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class SearchInvoke : SearchInvokeValue
    {
        [JsonProperty]
        public string? Dataset { get; set; }
    }

}
