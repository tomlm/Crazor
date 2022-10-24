using AdaptiveCards;
using Crazor;
using Crazor.Attributes;
using Microsoft.Bot.Schema;
using CrazorDemoBot.Cards.HelloWorld;

namespace CrazorDemoBot.Cards.HelloWorld
{
    public class HelloWorldApp : CardApp
    {
        public HelloWorldApp(IServiceProvider services)
            : base(services)
        {
        }

    }
}
