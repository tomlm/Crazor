using Crazor;

namespace CrazorDemoBot.Cards.NoShared
{
    public class NoSharedApp : CardApp
    {
        public NoSharedApp(IServiceProvider services) : base(services)
        {
            AutoSharedId = false;
        }
    }
}
