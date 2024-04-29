// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Crazor;
using Crazor.Attributes;

namespace CrazorDemoBot.Cards.Counters2
{
    public class Counters2App : CardApp
    {
        public Counters2App(CardAppContext context) : base(context)
        {
        }

        [AppMemory]
        public int AppCounter { get; set; }

    }
}
