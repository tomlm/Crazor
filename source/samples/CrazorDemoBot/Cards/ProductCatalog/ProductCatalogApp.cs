using AdaptiveCards;
using Microsoft.Bot.Builder;
using Crazor;
using Crazor.Attributes;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using CrazorDemoBot.Cards.HelloWorld;
using CrazorDemoBot.Cards.ProductCatalog;

namespace CrazorDemoBot.Cards.ProductCatalog
{
    public class ProductCatalogApp : CardApp
    {
        public ProductCatalogApp(IServiceProvider services)
            : base(services)
        {
        }

        public override async Task LoadAppAsync(string? sharedId, string? sessionId, Activity activity, CancellationToken cancellationToken)
        {
            var request = "https://ordersapi.azurewebsites.net/api/orders";
            using (var client = new HttpClient())
            {
                var response = await client.GetStringAsync(request);

                CatalogItems = JsonConvert.DeserializeObject<List<ProductCatalogItem>>(response)!;
            }

            await base.LoadAppAsync(sharedId, sessionId, activity, cancellationToken);      
        }

        public List<ProductCatalogItem> CatalogItems { get; set; } = new List<ProductCatalogItem>();
    }
}
