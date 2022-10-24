using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using AdaptiveCards;

namespace OpBot.Cards.WorkOrder
{
    public class WorkOrder : HasExtensionData
    {

        [JsonProperty("msdyn_name")]
        public string Name { get; set; }

        [JsonProperty("msdyn_serviceaccount@odata.bind")]
        public Uri ServiceAccount { get; set; }

        [JsonProperty("msdyn_workordersummary")]
        public string Summary { get; set; }

        [JsonProperty("msdyn_workordertype")]
        public WorkOrderType WorkOrderType { get; set; }


        [JsonProperty("msdyn_systemstatus")]
        public SystemStatus SystemStatus { get; set; }

        [JsonProperty("msdyn_pricelist")]
        [Required]
        public PriceLevel PriceList { get; set; }

        [JsonProperty("msdyn_customerasset")]
        public Asset CustomerAsset { get; set; }

        public DateTimeOffset? PromisedBy { get; set; }

        public DateTimeOffset? StartTime { get; set; }

        public DateTimeOffset? CompleteTime { get; set; }

        public Person? AssignedTo { get; set; }

        public void AssignTo(string userName)
        {
            this.AssignedTo = Person.GetTechnicians().FirstOrDefault(x => x.Email.Equals(userName, StringComparison.OrdinalIgnoreCase));
        }

        public AdaptiveTextColor GetStatusColor
        {
            get
            {
                switch (this.SystemStatus?.Name)
                {
                    case "Unscheduled":
                    case "Canceled":
                        return AdaptiveTextColor .Attention;

                    case "Scheduled":
                    case "In Progress":
                    case "Completed":
                        return AdaptiveTextColor.Good;
                }
                return AdaptiveTextColor.Default;
            }
        }

        public static WorkOrder Dummy =>
            new WorkOrder
            {
                Name = "WO3872",
                WorkOrderType = new WorkOrderType("Diagnose and Repair"),
                SystemStatus = new SystemStatus("Unscheduled"),
                CustomerAsset = new Asset("Hydraulic Pwr Unit #3"),
                Summary = "Repair hydraulic leak",
                PromisedBy = DateTime.UtcNow.AddHours(24),
            };
    }


    public class WorkOrderType
    {
        public WorkOrderType()
        {

        }

        public WorkOrderType(string name)
        {
            Name = name;
        }

        public string Name { get; set;  }
    }
}
