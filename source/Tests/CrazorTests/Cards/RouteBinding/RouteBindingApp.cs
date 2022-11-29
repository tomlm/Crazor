using Crazor;
using Crazor.Attributes;

namespace CrazorTests.Cards.RouteBinding
{
    public class RouteBindingApp : CardApp
    {
        public RouteBindingApp(IServiceProvider services) : base(services)
        {
        }

        public string? Id { get; set; }
    }
}