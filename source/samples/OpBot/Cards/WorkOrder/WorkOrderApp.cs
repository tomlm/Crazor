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


        public override async Task LoadAppAsync(string? resourceId, string? sessionId, Activity activity, CancellationToken cancellationToken)
        {
            await base.LoadAppAsync(resourceId, sessionId, activity, cancellationToken);
            
            //var accounts = await GetResponseAsync<OData<IEnumerable<Account>>>(HttpMethod.Get, "accounts?$top=5");
            //if (accounts != null)
            //{
            //    ServiceAccounts = accounts.Value.ToList();
            //}

            //var workOrderTypes = await GetResponseAsync<OData<IEnumerable<WorkOrderType>>>(HttpMethod.Get, "msdyn_workordertypes?$top=5");
            //if (workOrderTypes != null)
            //{
            //    WorkOrderTypes = workOrderTypes.Value.ToList();
            //}

            // TODO:
            // Fetch a real Work Order using the resourceId
        }

        public override Task SaveAppAsync(CancellationToken cancellationToken)
        {
            return base.SaveAppAsync(cancellationToken);
        }

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
