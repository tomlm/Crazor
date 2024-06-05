using AdaptiveCards;
using Microsoft.Bot.Builder.Adapters;

namespace Crazor.Test
{
    public class CardTestContext
    {
        public IServiceProvider Services { get; set; }

        public AdaptiveCard Card { get; set; }

        public TestAdapter Adapter { get; set; }

        public CardApp App { get; set; }
    }
}