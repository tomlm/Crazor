using AdaptiveCards;
using Microsoft.Bot.Cards;
using Microsoft.Bot.Cards.Attributes;
using Microsoft.Bot.Schema;

namespace OpBot.Cards.Counters
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
    }
}
