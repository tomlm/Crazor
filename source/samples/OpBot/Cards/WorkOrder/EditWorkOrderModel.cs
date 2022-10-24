using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;

namespace OpBot.Cards.WorkOrder
{
    public class EditWorkOrderModel
    {
        public bool IsEdit { get; set; }

        [BindProperty]
        public WorkOrder WorkOrder { get; set; } = new WorkOrder();
    }
}
