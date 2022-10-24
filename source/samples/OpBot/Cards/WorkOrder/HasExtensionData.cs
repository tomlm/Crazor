using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OpBot.Cards.WorkOrder
{
    public abstract class HasExtensionData
    {
        [JsonExtensionData]
        public JObject ExtensionData { get; set; }
    }
}
