using AdaptiveCards;
using Crazor;
using Crazor.Attributes;
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
