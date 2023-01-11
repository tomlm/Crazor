using Newtonsoft.Json;

namespace Crazor.Server.Teams
{
    public class TaskInfo
    {
        /// <summary>
        /// Initial dialog title
        /// </summary>
        [JsonProperty("title", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        /// <summary>
        /// Dialog width - either a number in pixels or default layout such as 'large', 'medium', or 'small'
        /// </summary>
        [JsonProperty("width", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Width { get; set; }

        /// <summary>
        /// Dialog height - either a number in pixels or default layout such as 'large', 'medium', or 'small'
        /// </summary>
        [JsonProperty("height", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Height { get; set; }

        /// <summary>
        /// Initial webview URL
        /// </summary>
        [JsonProperty("url", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }


    }
}