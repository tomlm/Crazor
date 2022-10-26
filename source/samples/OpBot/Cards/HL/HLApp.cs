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
    }
}
