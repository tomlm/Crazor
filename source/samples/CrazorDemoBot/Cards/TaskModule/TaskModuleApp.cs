using Crazor;
using Crazor.Attributes;

namespace CrazorDemoBot.Cards.TaskModule
{
    [TaskInfo(Width = "small", Height = "medium", Title = "Test the Task Module")]
    public class TaskModuleApp : CardApp
    {
        public TaskModuleApp(CardAppContext context) : base(context)
        {
        }

        [SessionMemory]
        public int Counter { get; set; }
    }
}
