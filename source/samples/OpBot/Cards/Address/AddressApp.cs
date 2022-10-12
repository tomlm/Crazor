using AdaptiveCards;
using AdaptiveCards.Rendering;
using Crazor;
using Crazor.Attributes;
using Microsoft.Bot.Schema;

namespace OpBot.Cards.Address
{
    public class AddressApp : CardApp
    {
        public AddressApp(IServiceProvider services)
            : base(services)
        {
        }

        [SharedMemory]
        public List<Address> Addresses { get; set; } = new List<Address>();
    }
}
