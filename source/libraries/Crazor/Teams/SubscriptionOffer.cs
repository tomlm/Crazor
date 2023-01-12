using Newtonsoft.Json;

namespace Crazor.Teams
{
    public class SubscriptionOffer
    {
        /// <summary>
        /// A unique identifier for the Commercial Marketplace Software as a Service Offer.
        /// </summary>
        [JsonProperty("offerId", Required = Required.Always)]
        public string OfferId { get; set; }


    }
}