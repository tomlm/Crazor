using Newtonsoft.Json;

namespace Crazor.Teams
{
    public class ActivityType
    {
        [JsonProperty("type", Required = Required.Always)]
        public string Type { get; set; }

        [JsonProperty("description", Required = Required.Always)]
        public string Description { get; set; }

        [JsonProperty("templateText", Required = Required.Always)]
        public string TemplateText { get; set; }


    }
}