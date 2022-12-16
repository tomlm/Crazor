using Crazor;
using Crazor.Attributes;

namespace Crazor.Mvc.Tests.Cards.RouteBinding
{
    public class RouteBindingApp : CardApp
    {
        public RouteBindingApp(CardAppContext context) : base(context)
        {
        }

        public string? Id { get; set; }
    }
}