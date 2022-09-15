using AdaptiveCards;
using AdaptiveCards.Rendering;
using Microsoft.Bot.Cards;
using Microsoft.Bot.Cards.Attributes;
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
