using Crazor;
using Crazor.Attributes;

namespace CrazorTests.Cards.TaskModule
{
    [TaskInfo(Width = "small", Height = "medium", Title = "Test Task Module")]
    public class TaskModuleApp : CardApp
    {
        public TaskModuleApp(CardAppContext context) : base(context)
        {
        }

        [SessionMemory]
        public int Counter { get; set; }
    }
}
