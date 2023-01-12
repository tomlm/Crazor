using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Crazor.Teams
{
    public class MessageHandler
    {
        /// <summary>
        /// Type of the message handler
        /// </summary>
        [JsonProperty("type", Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public MessageHandlerType Type { get; set; }

        [JsonProperty("value", Required = Required.Always)]
        public Value Value { get; set; } = new Value();


    }
}