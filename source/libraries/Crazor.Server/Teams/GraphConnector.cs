using Newtonsoft.Json;

namespace Crazor.Server.Teams
{
    public class GraphConnector
    {
        /// <summary>
        /// The url where Graph-connector notifications for the application should be sent.
        /// </summary>
        [JsonProperty("notificationUrl", Required = Required.Always)]
        public string NotificationUrl { get; set; }


    }
}