using AdaptiveCards;
using Crazor;
using Crazor.Attributes;
using Microsoft.AspNetCore.Components;
using Microsoft.Bot.Schema;

namespace CrazorDemoBot.Cards.Counters
{
    public class CountersApp : CardApp
    {
        public CountersApp(IServiceProvider services)
            : base(services)
        {
        }

        [SharedMemory]
        public int SharedCounter { get; set; } = 0;

        public override string GetSharedId() => Utils.GetNewId();
    }
}
