using Newtonsoft.Json;

namespace Crazor.Server.Teams
{
    public class Connector
    {
        /// <summary>
        /// A unique identifier for the connector which matches its ID in the Connectors Developer Portal.
        /// </summary>
        [JsonProperty("connectorId", Required = Required.Always)]
        public string ConnectorId { get; set; }

        /// <summary>
        /// The url to use for configuring the connector using the inline configuration experience.
        /// </summary>
        [JsonProperty("configurationUrl", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string ConfigurationUrl { get; set; }

        /// <summary>
        /// Specifies whether the connector offers an experience in the context of a channel in a team, or an experience scoped to an individual user alone. Currently, only the team scope is supported.
        /// </summary>
        [JsonProperty("scopes", Required = Required.Always)]
        public List<ConnectorScope> Scopes { get; set; } = new List<ConnectorScope>();


    }
}