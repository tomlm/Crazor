﻿using AdaptiveCards;
using Crazor;
using Crazor.Attributes;
using Microsoft.Bot.Schema;
using OpBot.Cards.HelloWorld;

namespace OpBot.Cards.HelloWorld
{
    public class HelloWorldApp : CardApp
    {
        public HelloWorldApp(IServiceProvider services)
            : base(services)
        {
        }

        [SessionMemory]
        public int SessionCounter { get; set; } = 0;

        [SharedMemory]
        public int SharedCounter { get; set; } = 0;
    }
}
