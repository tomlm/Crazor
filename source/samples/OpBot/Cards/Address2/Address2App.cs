using AdaptiveCards;
using AdaptiveCards.Rendering;
using Crazor;
using Crazor.Attributes;
using Microsoft.Bot.Schema;

namespace OpBot.Cards.Address2
{
    public class Address2App : CardApp
    {
        public Address2App(IServiceProvider services)
            : base(services)
        {
        }

        [SharedMemory]
        public List<Address> Addresses { get; set; } = new List<Address>();
    }
}
