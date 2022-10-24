using AdaptiveCards;
using AdaptiveCards.Rendering;
using Crazor;
using Crazor.Attributes;
using Microsoft.Bot.Schema;

namespace CrazorDemoBot.Cards.Addresses
{
    public class AddressesApp : CardApp
    {
        public AddressesApp(IServiceProvider services)
            : base(services)
        {
        }

        [SharedMemory]
        public List<Address> Addresses { get; set; } = new List<Address>();
    }
}
