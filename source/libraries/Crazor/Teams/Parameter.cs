using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Crazor.Teams
{
    public class Parameter
    {
        /// <summary>
        /// Name of the parameter.
        /// </summary>
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        /// <summary>
        /// Type of the parameter
        /// </summary>
        [JsonProperty("inputType", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ParametersInputType InputType { get; set; } = ParametersInputType.Text;

        /// <summary>
        /// Title of the parameter.
        /// </summary>
        [JsonProperty("title", Required = Required.Always)]
        public string Title { get; set; }

        /// <summary>
        /// Description of the parameter.
        /// </summary>
        [JsonProperty("description", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        /// <summary>
        /// Initial value for the parameter
        /// </summary>
        [JsonProperty("value", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Value { get; set; }

        /// <summary>
        /// The choice options for the parameter
        /// </summary>
        [JsonProperty("choices", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<Choice> Choices { get; set; }


    }
}