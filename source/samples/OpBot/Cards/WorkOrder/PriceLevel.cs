using Newtonsoft.Json;

namespace OpBot.Cards.WorkOrder
{
    public class PriceLevel : HasExtensionData
    {
        [JsonProperty("pricelevelid")]
        public Guid? PriceLevelId { get; set; }
    }
}
