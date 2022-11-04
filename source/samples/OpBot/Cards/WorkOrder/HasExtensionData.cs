using Crazor.Validation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OpBot.Cards.WorkOrder
{
    public abstract class HasExtensionData
    {
        [SkipRecursiveValidation]
        [JsonExtensionData]
        public JObject? ExtensionData { get; set; }
    }
}
