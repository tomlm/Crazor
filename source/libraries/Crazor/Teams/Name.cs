using Newtonsoft.Json;

namespace Crazor.Teams
{
    public class Name
    {
        /// <summary>
        /// A short display name for the app.
        /// </summary>
        [JsonProperty("short", Required = Required.Always)]
        public string Short { get; set; }

        /// <summary>
        /// The full name of the app, used if the full app name exceeds 30 characters.
        /// </summary>
        [JsonProperty("full", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Full { get; set; }


    }
}