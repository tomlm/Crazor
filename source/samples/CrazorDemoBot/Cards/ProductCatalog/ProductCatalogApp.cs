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
    }
}
