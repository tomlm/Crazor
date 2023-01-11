using Newtonsoft.Json;

namespace Crazor.Server.Teams
{
    public class Scene
    {
        /// <summary>
        /// A unique identifier for this scene. This id must be a GUID.
        /// </summary>
        [JsonProperty("id", Required = Required.Always)]
        public string Id { get; set; }

        /// <summary>
        /// Scene name.
        /// </summary>
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        /// <summary>
        /// A relative file path to a scene metadata json file.
        /// </summary>
        [JsonProperty("file", Required = Required.Always)]
        public string File { get; set; }

        /// <summary>
        /// A relative file path to a scene PNG preview icon.
        /// </summary>
        [JsonProperty("preview", Required = Required.Always)]
        public string Preview { get; set; }

        /// <summary>
        /// Maximum audiences supported in scene.
        /// </summary>
        [JsonProperty("maxAudience", Required = Required.Always)]
        public int MaxAudience { get; set; }

        /// <summary>
        /// Number of seats reserved for organizers or presenters.
        /// </summary>
        [JsonProperty("seatsReservedForOrganizersOrPresenters", Required = Required.Always)]
        public int SeatsReservedForOrganizersOrPresenters { get; set; }


    }
}