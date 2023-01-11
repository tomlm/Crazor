using Newtonsoft.Json;

namespace Crazor.Server.Teams
{
    public class Choice
    {
        /// <summary>
        /// Title of the choice
        /// </summary>
        [JsonProperty("title", Required = Required.Always)]
        public string Title { get; set; }

        /// <summary>
        /// Value of the choice
        /// </summary>
        [JsonProperty("value", Required = Required.Always)]
        public string Value { get; set; }


    }
}