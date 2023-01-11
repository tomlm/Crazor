using Newtonsoft.Json;

namespace Crazor.Server.Teams
{
    public class MeetingExtensionDefinition
    {
        /// <summary>
        /// Meeting supported scenes.
        /// </summary>
        [JsonProperty("scenes", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<Scene> Scenes { get; set; }

        /// <summary>
        /// A boolean value indicating whether this app can stream the meeting's audio video content to an RTMP endpoint.
        /// </summary>
        [JsonProperty("supportsStreaming", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public bool SupportsStreaming { get; set; } = false;


    }
}