using Newtonsoft.Json;

namespace Crazor.Teams
{
    public class WebApplicationInfo
    {
        /// <summary>
        /// AAD application id of the app. This id must be a GUID.
        /// </summary>
        [JsonProperty("id", Required = Required.Always)]
        public string Id { get; set; }

        /// <summary>
        /// Resource url of app for acquiring auth token for SSO.
        /// </summary>
        [JsonProperty("resource", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Resource { get; set; }


    }
}