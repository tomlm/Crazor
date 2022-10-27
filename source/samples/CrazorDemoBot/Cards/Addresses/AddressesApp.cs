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

        public override Task LoadAppAsync(string? sharedId, string? sessionId, Activity activity, CancellationToken cancellationToken)
        {
            if (sharedId == null)
                sharedId = DateTime.Now.ToString("yyyyMMdd");
            return base.LoadAppAsync(sharedId, sessionId, activity, cancellationToken);
        }
    }
}
