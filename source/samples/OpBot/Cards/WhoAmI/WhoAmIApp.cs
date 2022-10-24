using Crazor.Attributes;
using Microsoft.Bot.Schema;

namespace OpBot.Cards.WorkOrder
{
    public class WhoAmIApp : DataverseCardApp
    {
        public WhoAmIApp(IServiceProvider services)
            : base(services)
        {
        }


        [SessionMemory]
        public WhoAmI WhoAmI { get; set; }

        public override async Task LoadAppAsync(string? resourceId, string? sessionId, Activity activity, CancellationToken cancellationToken)
        {
            await base.LoadAppAsync(resourceId, sessionId, activity, cancellationToken);
            WhoAmI = await GetResponseAsync<WhoAmI>(HttpMethod.Get, "WhoAmI");

        }

    }
}
