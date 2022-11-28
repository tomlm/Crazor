using Crazor;
using Crazor.Attributes;

namespace CrazorDemoBot.Cards.Counters
{
    public class CountersApp : CardApp
    {
        public CountersApp(IServiceProvider services)
            : base(services)
        {
        }

        [AppMemory]
        public int AppCounter { get; set; } 

    }
}
