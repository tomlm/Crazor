using Newtonsoft.Json;

namespace Crazor.Server.Teams
{
    public class Description
    {
        /// <summary>
        /// A short description of the app used when space is limited. Maximum length is 80 characters.
        /// </summary>
        [JsonProperty("short", Required = Required.Always)]
        public string Short { get; set; }

        /// <summary>
        /// The full description of the app. Maximum length is 4000 characters.
        /// </summary>
        [JsonProperty("full", Required = Required.Always)]
        public string Full { get; set; }


    }
}