using Newtonsoft.Json;

namespace Crazor.Server.Teams
{
    public class LocalizationInfo
    {
        /// <summary>
        /// The language tag of the strings in this top level manifest file.
        /// </summary>
        [JsonProperty("defaultLanguageTag", Required = Required.Always)]
        public string DefaultLanguageTag { get; set; }

        [JsonProperty("additionalLanguages", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<AdditionalLanguages> AdditionalLanguages { get; set; }


    }
}