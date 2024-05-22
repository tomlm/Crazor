#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Crazor
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class SearchResult
    {
        /// <summary>
        /// Preview title
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Preview subtitle
        /// </summary>
        public string? Subtitle { get; set; }

        /// <summary>
        /// Preview text
        /// </summary>
        public string? Text { get; set; }

        /// <summary>
        /// Preview image
        /// </summary>
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Route to the card to display if user selects it.
        /// </summary>
        public string Route { get; set; }
    }
}
