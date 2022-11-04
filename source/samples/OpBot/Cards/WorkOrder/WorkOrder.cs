using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using AdaptiveCards;

namespace OpBot.Cards.WorkOrder
{
    public class WorkOrder : HasExtensionData
    {

        [JsonProperty("msdyn_name")]
        [Required]
        public string? Name { get; set; }

        [JsonProperty("msdyn_serviceaccount@odata.bind")]
        public string? ServiceAccount { get; set; }

        [JsonProperty("msdyn_workordersummary")]
        [Required]
        public string? Summary { get; set; }

        [JsonProperty("msdyn_workordertype")]
        [Required]
        public WorkOrderType WorkOrderType { get; set; } = new WorkOrderType();

        [JsonProperty("msdyn_systemstatus")]
        [Required]
        public SystemStatus SystemStatus { get; set; } = new SystemStatus();

        [JsonProperty("msdyn_pricelist")]
        public PriceLevel PriceList { get; set; } = new PriceLevel();

        [JsonProperty("msdyn_customerasset")]
        public Asset CustomerAsset { get; set; } = new Asset();

        public string? IoTAlert { get; set; }

        public DateTimeOffset? PromisedBy { get; set; }

        public DateTimeOffset? StartTime { get; set; }

        public DateTimeOffset? CompleteTime { get; set; }

        public Person? AssignedTo { get; set; }

        public void AssignTo(string userName)
        {
            this.AssignedTo = Person.GetTechnicians().FirstOrDefault(x => x.Email!.Equals(userName, StringComparison.OrdinalIgnoreCase));
        }

        [JsonIgnore]
        public AdaptiveTextColor GetStatusColor
        {
            get
            {
                switch (this.SystemStatus?.Name)
                {
                    case "Unscheduled":
                    case "Canceled":
                        return AdaptiveTextColor.Attention;

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

        [Required]
        public string? Name { get; set; }
    }
}
