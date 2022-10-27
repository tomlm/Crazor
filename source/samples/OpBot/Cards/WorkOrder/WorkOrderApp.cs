using Crazor;
using Crazor.Attributes;
using Microsoft.Bot.Schema;

namespace OpBot.Cards.WorkOrder
{
    public class WorkOrderApp : DataverseCardApp
    {
        public WorkOrderApp(IServiceProvider services)
            : base(services)
        {
            
        }

        [SharedMemory]
        public Dictionary<string, WorkOrder> WorkOrders { get; set; } = new Dictionary<string, WorkOrder>();


        [SharedMemory]
        public List<Account> ServiceAccounts { get; set; }

        public List<WorkOrderType> WorkOrderTypes { get; set; }

        public List<PriceLevel> PriceLists { get; set; }

        public List<SystemStatus> SystemStatuses { get; set; }

        public override string GetSharedId() => Utils.GetNewId();

        public async Task<WorkOrder?> LookupWorkOrder(string workOrderName)
        {
            await Task.CompletedTask;

            if (WorkOrders.TryGetValue(workOrderName, out var model))
                return model;
            return null;
        }

        public async Task UpdateWorkOrder(WorkOrder workOrder)
        {
            await Task.CompletedTask;
            WorkOrders[workOrder.Name] = workOrder;
        }

        public async Task DeleteWorkOrder(string name)
        {
            await Task.CompletedTask;
            WorkOrders.Remove(name);
        }

    }
}
