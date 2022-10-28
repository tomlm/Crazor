using Crazor;
using Crazor.Attributes;

namespace CrazorDemoBot.Cards.TaskModule
{
    public class TaskModuleApp : CardApp
    {
        public TaskModuleApp(IServiceProvider services)
            : base(services)
        {
        }

        [SessionMemory]
        public int Counter { get; set; }
    }
}
