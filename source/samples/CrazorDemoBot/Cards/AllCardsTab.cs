using Crazor;

namespace CrazorDemoBot.Cards
{
    public class AllCardsTab : CardTabModule
    {
        public AllCardsTab(IServiceProvider services) : base(services) { }

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

            return Task.FromResult(uris.ToArray());
        }
    }
}
