using Crazor;
using Crazor.Attributes;

namespace CrazorDemoBot.Cards.TaskModule
{
    [TaskInfo(Width = "small", Height = "medium", Title = "Test the Task Module")]
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
