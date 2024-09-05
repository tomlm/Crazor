using Newtonsoft.Json;

namespace Crazor.Teams
{
    public class AppDeveloper
    {
        /// <summary>
        /// The display name for the developer.
        /// </summary>
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        /// <summary>
        /// The Microsoft Partner Network ID that identifies the partner organization building the app. This field is not required, and should only be used if you are already part of the Microsoft Partner Network. More info at https://aka.ms/partner
        /// </summary>
        [JsonProperty("mpnId", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string MpnId { get; set; }

        /// <summary>
        /// The url to the page that provides support information for the app.
        /// </summary>
        [JsonProperty("websiteUrl", Required = Required.Always)]
        public string WebsiteUrl { get; set; }

        /// <summary>
        /// The url to the page that provides privacy information for the app.
        /// </summary>
        [JsonProperty("privacyUrl", Required = Required.Always)]
        public string PrivacyUrl { get; set; }

        /// <summary>
        /// The url to the page that provides the terms of use for the app.
        /// </summary>
        [JsonProperty("termsOfUseUrl", Required = Required.Always)]
        public string TermsOfUseUrl { get; set; }


    }
}