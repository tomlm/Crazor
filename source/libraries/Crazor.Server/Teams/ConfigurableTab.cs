using Newtonsoft.Json;

namespace Crazor.Server.Teams
{
    public class ConfigurableTab
    {
        /// <summary>
        /// The url to use when configuring the tab.
        /// </summary>
        [JsonProperty("configurationUrl", Required = Required.Always)]
        public string ConfigurationUrl { get; set; }

        /// <summary>
        /// A value indicating whether an instance of the tab's configuration can be updated by the user after creation.
        /// </summary>
        [JsonProperty("canUpdateConfiguration", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public bool CanUpdateConfiguration { get; set; } = true;

        /// <summary>
        /// Specifies whether the tab offers an experience in the context of a channel in a team, in a 1:1 or group chat, or in an experience scoped to an individual user alone. These options are non-exclusive. Currently, configurable tabs are only supported in the teams and groupchats scopes.
        /// </summary>
        [JsonProperty("scopes", Required = Required.Always)]
        public List<ConfigurableTabScope> Scopes { get; set; } = new List<ConfigurableTabScope>();

        /// <summary>
        /// The set of meetingSurfaceItem scopes that a tab belong to
        /// </summary>
        [JsonProperty("meetingSurfaces", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<MeetingSurface> MeetingSurfaces { get; set; }

        /// <summary>
        /// The set of contextItem scopes that a tab belong to
        /// </summary>
        [JsonProperty("context", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<Context> Context { get; set; }

        /// <summary>
        /// A relative file path to a tab preview image for use in SharePoint. Size 1024x768.
        /// </summary>
        [JsonProperty("sharePointPreviewImage", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string SharePointPreviewImage { get; set; }

        /// <summary>
        /// Defines how your tab will be made available in SharePoint.
        /// </summary>
        [JsonProperty("supportedSharePointHosts", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<SupportedSharePointHost> SupportedSharePointHosts { get; set; }


    }
}