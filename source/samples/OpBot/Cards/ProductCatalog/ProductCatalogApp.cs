using AdaptiveCards;
using Microsoft.Bot.Builder;
using Crazor;
using Crazor.Attributes;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using OpBot.Cards.HelloWorld;
using OpBot.Cards.ProductCatalog;

namespace OpBot.Cards.ProductCatalog
{
    public class ProductCatalogApp : CardApp
    {
        public ProductCatalogApp(IServiceProvider services)
            : base(services)
        {
        }

        public override async Task LoadAppAsync(string? resourceId, string? sessionId, Activity activity, CancellationToken cancellationToken)
        {
            var request = "https://ordersapi.azurewebsites.net/api/orders";
            using (var client = new HttpClient())
            {
                var response = await client.GetStringAsync(request);

                CatalogItems = JsonConvert.DeserializeObject<List<ProductCatalogItem>>(response)!;
            }

            await base.LoadAppAsync(resourceId, sessionId, activity, cancellationToken);      
        }

        [SharedMemory]
        public List<ProductCatalogItem> CatalogItems { get; set; } = new List<ProductCatalogItem>();
    }
}
