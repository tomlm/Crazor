using Newtonsoft.Json;

namespace OpBot.Cards.WorkOrder
{
    public sealed class Account : HasExtensionData
    {
        [JsonProperty("accountid")]
        public Guid? AccountId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("defaultpricelevelid@odata.bind")]
        public Uri DefaultPriceLevelId { get; set; }
    }
}
