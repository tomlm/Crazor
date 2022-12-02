using Crazor;
using Microsoft.Bot.Schema.Teams;

namespace CrazorTests.Cards.Search
{
    public class SearchApp : CardApp
    {
        private static List<string> Names = new List<string>()
        {
            "john", "george", "lili", "tom", "scott", "bob", "steve", "frank", "stacia","leslie","emma"
        };

        public SearchApp(IServiceProvider services) : base(services) { }

        /// <summary>
        /// Implement search 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task<SearchResult[]> OnSearchQueryAsync(MessagingExtensionQuery query, CancellationToken cancellationToken)
        {
            // do the search
            var searchTerm = query.Parameters.SingleOrDefault(p => p.Name == "search")?.Value.ToString() ?? String.Empty;
            var names = Names.Where(name => name.ToLower().Contains(searchTerm.ToLower())).Skip(query.QueryOptions.Skip ?? 0).Take(query.QueryOptions.Count ?? 10);

            return Task.FromResult(names.Select(name =>
            {
                return new SearchResult()
                {
                    Title = name,
                    ImageUrl = new Uri(this.GetCurrentCardUri(), "/images/card.png").AbsoluteUri,
                    Route = $"/Cards/Search/{name}"
                };
            }).ToArray());
        }
    }
}
