using Crazor;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;

namespace SharedCards.Cards.NugetSearch
{
    public class NugetApp : CardApp
    {
        private static HttpClient _httpClient = new HttpClient();

        public NugetApp(CardAppContext context)
            : base(context)
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

        /// <summary>
        /// Implement search 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async override Task<SearchResult[]> OnSearchQueryAsync(MessagingExtensionQuery query, CancellationToken cancellationToken)
        {
            var searchTerm = query.Parameters.SingleOrDefault(p => p.Name == "search")?.Value.ToString() ?? String.Empty;
            var packages = await SearchNugetPackages(searchTerm, query.QueryOptions.Skip ?? 0, query.QueryOptions.Count ?? 20, cancellationToken);
            return packages.Select(package => new SearchResult()
            {
                Title = package.Title!,
                Subtitle = package.Version!,
                Text = package.Description!,
                ImageUrl = package.IconUrl! ?? "https://raw.githubusercontent.com/github/explore/80688e429a7d4ef2fca1e82350fe8e3517d3494d/topics/nuget/nuget.png",
                Route = $"/Cards/Nuget/Packages/{package.Id}"
            }).ToArray();
        }
    }
}
