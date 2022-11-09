using Crazor;
using Microsoft.Bing.WebSearch.Models;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;

namespace CrazorDemoBot.Cards.NugetSearch
{
    public class NugetApp : CardApp
    {
        private static HttpClient _httpClient = new HttpClient();

        public NugetApp(IServiceProvider services)
            : base(services)
        {
        }

        public async Task<NugetPackage> GetNugetPackage(string packageId, CancellationToken cancellationToken)
        {
            var json = await _httpClient.GetStringAsync($"https://azuresearch-usnc.nuget.org/query?q=PackageId:{packageId}&prerelease=true");
            var searchResult = JsonConvert.DeserializeObject<NugetSearchResponse>(json)!;
            return searchResult.Packages.FirstOrDefault()!;
        }

        public async Task<NugetPackage[]> SearchNugetPackages(string search, int skip, int take, CancellationToken cancellationToken)
        {
            var json = await _httpClient.GetStringAsync($"https://azuresearch-usnc.nuget.org/query?q=id:{search}&prerelease=true&skip={skip}&take={take}", cancellationToken);
            var searchResult = JsonConvert.DeserializeObject<NugetSearchResponse>(json)!;
            return searchResult.Packages;
        }
    }
}
