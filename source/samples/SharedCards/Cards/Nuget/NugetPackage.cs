using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SharedCards.Cards.NugetSearch
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class NugetSearchResponse
    {
        public int TotalHits { get; set; }

        [JsonProperty("data")]
        public NugetPackage[] Packages { get; set; }
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class NugetPackage
    {
        public string? Title { get; set; }

        public string? Id { get; set; }

        public string? Version { get; set; }

        public string? Description { get; set; }

        public string? ProjectUrl { get; set; }

        public string? IconUrl { get; set; }
    }
}
