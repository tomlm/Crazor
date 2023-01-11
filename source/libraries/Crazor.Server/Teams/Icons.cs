using Newtonsoft.Json;

namespace Crazor.Server.Teams
{
    public class Icons
    {
        /// <summary>
        /// A relative file path to a transparent PNG outline icon. The border color needs to be white. Size 32x32.
        /// </summary>
        [JsonProperty("outline", Required = Required.Always)]
        public string Outline { get; set; }

        /// <summary>
        /// A relative file path to a full color PNG icon. Size 192x192.
        /// </summary>
        [JsonProperty("color", Required = Required.Always)]
        public string Color { get; set; }


    }
}