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

        [SharedMemory]
        public int Counter { get; set; }
    }
}
