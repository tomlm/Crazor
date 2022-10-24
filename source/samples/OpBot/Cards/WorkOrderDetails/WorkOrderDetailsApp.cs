using AdaptiveCards;
using Crazor;
using Crazor.Attributes;

namespace OpBot.Cards.WorkOrderDetails
{
    public class WorkOrderDetailsApp : CardApp
    {
        public WorkOrderDetailsApp(IServiceProvider services)
            : base(services)
        {
        }

        [SessionMemory]
        public string Status { get; set; } = "Scheduled";

        [SessionMemory]
        public DateTimeOffset? PromisedBy { get; set; }

        [SessionMemory]
        public DateTimeOffset? StartTime { get; set; }

        [SessionMemory]
        public DateTimeOffset? CompleteTime { get; set; }
    }
}
