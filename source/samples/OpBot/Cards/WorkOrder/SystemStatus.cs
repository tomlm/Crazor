
namespace OpBot.Cards.WorkOrder
{
    public class SystemStatus
    {
        public static List<string> GetDefault() => new List<string> {
            "Unscheduled",
            "Scheduled",
            "In Progress",
            "Completed",
            "Posted",
            "Canceled"
        };

        public SystemStatus()
        {

        }
        public SystemStatus(string name)
        {
            Name = name;
        }

        public string? Name { get; set; }
    }
}