using Newtonsoft.Json;

namespace Crazor.Teams
{
    public class AdditionalLanguages
    {
        /// <summary>
        /// The language tag of the strings in the provided file.
        /// </summary>
        [JsonProperty("languageTag", Required = Required.Always)]
        public string LanguageTag { get; set; }

        /// <summary>
        /// A relative file path to a the .json file containing the translated strings.
        /// </summary>
        [JsonProperty("file", Required = Required.Always)]
        public string File { get; set; }


    }
}