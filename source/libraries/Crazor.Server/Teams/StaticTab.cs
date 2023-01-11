using Newtonsoft.Json;

namespace Crazor.Server.Teams
{
    public class StaticTab
    {
        /// <summary>
        /// A unique identifier for the entity which the tab displays.
        /// </summary>
        [JsonProperty("entityId", Required = Required.Always)]
        public string EntityId { get; set; }

        /// <summary>
        /// The display name of the tab.
        /// </summary>
        [JsonProperty("name", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        /// <summary>
        /// The url which points to the entity UI to be displayed in the canvas.
        /// </summary>
        [JsonProperty("contentUrl", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string ContentUrl { get; set; }

        /// <summary>
        /// The Microsoft App ID specified for the bot in the Bot Framework portal (https://dev.botframework.com/bots)
        /// </summary>
        [JsonProperty("contentBotId", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string ContentBotId { get; set; }

        /// <summary>
        /// The url to point at if a user opts to view in a browser.
        /// </summary>
        [JsonProperty("websiteUrl", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string WebsiteUrl { get; set; }

        /// <summary>
        /// The url to direct a user's search queries.
        /// </summary>
        [JsonProperty("searchUrl", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string SearchUrl { get; set; }

        /// <summary>
        /// Specifies whether the tab offers an experience in the context of a channel in a team, or an experience scoped to an individual user alone. These options are non-exclusive. Currently static tabs are only supported in the 'personal' scope.
        /// </summary>
        [JsonProperty("scopes", Required = Required.Always)]
        public List<StaticTabScope> Scopes { get; set; } = new List<StaticTabScope>();

        /// <summary>
        /// The set of contextItem scopes that a tab belong to
        /// </summary>
        [JsonProperty("context", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<TabContext> Context { get; set; }


    }
}