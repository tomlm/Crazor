using AdaptiveCards;
using Microsoft.Bot.Cards;
using Microsoft.Bot.Cards.Attributes;
using Microsoft.Bot.Schema;

namespace OpBot.Cards.MultiScreen
{
    public class MultiScreenApp : CardApp
    {
        public MultiScreenApp(IServiceProvider services)
            : base(services)
        {
        }
    }
}
