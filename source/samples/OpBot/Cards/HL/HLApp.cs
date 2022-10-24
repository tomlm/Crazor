using AdaptiveCards;
using Crazor;
using Crazor.Attributes;
using OpBot.Cards.WorkOrder;

namespace OpBot.Cards.HL
{
    public class HLApp : CardApp
    {
        public HLApp(IServiceProvider services)
            : base(services)
        {
            if (WorkOrder == null)
            {
                WorkOrder = OpBot.Cards.WorkOrder.WorkOrder.Dummy;
                WorkOrder.AssignTo(Person.DEMO_USER);
            }
        }

        [SharedMemory]
        public WorkOrder.WorkOrder WorkOrder { get; set; }

        //[SessionMemory]
        //public string Status { get; set; } = "Scheduled";

        //[SessionMemory]
        //public DateTimeOffset? PromisedBy { get; set; }

        //[SessionMemory]
        //public DateTimeOffset? StartTime { get; set; }

        //[SessionMemory]
        //public DateTimeOffset? CompleteTime { get; set; }
    }
}
