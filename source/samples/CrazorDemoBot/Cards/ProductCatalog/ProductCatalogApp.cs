using Crazor;
using Newtonsoft.Json;
using System.Text;

namespace CrazorDemoBot.Cards.ProductCatalog
{
    public class ProductCatalogApp : CardApp
    {
        public ProductCatalogApp(IServiceProvider services)
            : base(services)
        {
        }

        public async Task<List<ProductCatalogItem>> GetCatalogItems(CancellationToken cancellationToken)
        {
            var request = "https://ordersapi.azurewebsites.net/api/orders";
            using (var client = new HttpClient())
            {
                var response = await client.GetStringAsync(request);

                return JsonConvert.DeserializeObject<List<ProductCatalogItem>>(response)!;
            }
        }

        public async Task<ProductCatalogItem> GetCatalogItem(string id, CancellationToken cancellationToken)
        {
            var request = $"https://ordersapi.azurewebsites.net/api/orders/{id}";
            using (var client = new HttpClient())
            {
                var response = await client.GetStringAsync(request);

                return JsonConvert.DeserializeObject<ProductCatalogItem>(response)!;
            }
        }

        public async Task AddCatalogItem(ProductCatalogItem catalogItem, CancellationToken cancellationToken)
        {
            // Update in API
            var request = "https://ordersapi.azurewebsites.net/api/orders";
            var data = JsonConvert.SerializeObject(catalogItem);
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(request, new StringContent(data, Encoding.UTF8, "application/json"), cancellationToken);
            }
        }

        public async Task UpdateCatalogItem(ProductCatalogItem catalogItem, CancellationToken cancellationToken)
        {
            var request = $"https://ordersapi.azurewebsites.net/api/orders/{catalogItem.Id}";
            var data = JsonConvert.SerializeObject(catalogItem);
            using (var client = new HttpClient())
            {
                var response = await client.PutAsync(request, new StringContent(data, Encoding.UTF8, "application/json"), cancellationToken);
            }
        }

        public async Task DeleteCatalogItem(string id, CancellationToken cancellationToken)
        {
            var request = $"https://ordersapi.azurewebsites.net/api/orders/{id}";
            using (var client = new HttpClient())
            {
                var response = await client.DeleteAsync(request, cancellationToken);
            }
        }
    }
}
