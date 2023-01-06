// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Crazor;

namespace CrazorDemoBot.Cards
{
    public class AllCardsTab : CardTabModule
    {
        public AllCardsTab(CardAppContext context) : base(context) { }

        /// <summary>
        /// return /Card/{AppName} for each app 
        /// </summary>
        /// <returns></returns>
        public override Task<string[]> GetCardUrisAsync()
        {
            var uris = new List<string>();
            uris.Add("/Cards/HelloWorld");
            uris.Add("/Cards/Counters");
            uris.Add("/Cards/Quiz");
            uris.Add("/Cards/TagHelper");

            return Task.FromResult(uris.ToArray());
        }
    }
}
