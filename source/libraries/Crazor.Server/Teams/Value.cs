using Newtonsoft.Json;

namespace Crazor.Server.Teams
{
    public class Value
    {
        /// <summary>
        /// A list of domains that the link message handler can register for, and when they are matched the app will be invoked
        /// </summary>
        [JsonProperty("domains", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Domains { get; set; }



        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties ?? (_additionalProperties = new Dictionary<string, object>()); }
            set { _additionalProperties = value; }
        }

    }
}