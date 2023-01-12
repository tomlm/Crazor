using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Crazor.Teams
{
    public class ResourceSpecific
    {
        /// <summary>
        /// The name of the resource-specific permission.
        /// </summary>
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        /// <summary>
        /// The type of the resource-specific permission.
        /// </summary>
        [JsonProperty("type", Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ResourceSpecificType Type { get; set; }


    }
}