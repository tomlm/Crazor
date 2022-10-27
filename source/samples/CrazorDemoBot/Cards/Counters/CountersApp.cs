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

        [SessionMemory]
        public int SessionCounter { get; set; } = 0;

        [SharedMemory]
        public int SharedCounter { get; set; } = 0;

        public override string GetSharedId() => Utils.GetNewId();
    }
}
