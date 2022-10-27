using Crazor;
using Crazor.Attributes;
using Microsoft.AspNetCore.Components;
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

        public override string GetSharedId() => DateTime.Now.ToString("yyyyMMdd");
    }
}
