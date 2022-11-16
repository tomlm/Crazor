using Crazor;
using Microsoft.Bot.Schema.Teams;

namespace CrazorDemoBot.Cards.Cards
{
    public class CardsApp : CardApp
    {
        public CardsApp(IServiceProvider services, CardAppFactory cardFactory)
            : base(services)
        {
            CardFactory = cardFactory;
        }

        public CardAppFactory CardFactory { get; }

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
            var names = CardFactory.GetNames().Where(name => name.ToLower().Contains(searchTerm.ToLower()));

            return Task.FromResult(names.Select(name =>
            {
                return new SearchResult()
                {
                    Title = name,
                    ImageUrl = new Uri(this.GetCurrentCardUri(), "/images/card.png").AbsoluteUri,
                    Route = $"/Cards/{name}"
                };
            }).ToArray());
        }
    }
}
