using AdaptiveCards;
using Microsoft.Bot.Cards;
using Microsoft.Bot.Cards.Attributes;
using Microsoft.Bot.Schema;

namespace OpBot.Cards.Quiz
{
    public class QuizApp : CardApp
    {
        public QuizApp(IServiceProvider services)
            : base(services)
        {
        }
    }
}
