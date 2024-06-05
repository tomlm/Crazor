using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SharedCards.Cards.UnitTest
{

    [JsonConverter(typeof(StringEnumConverter), true)]
    public enum ConversationType
    {
        Unknown,
        Personal,
        Channel,
        Email
    }

    [JsonConverter(typeof(StringEnumConverter), true)]
    public enum Platform
    {
        Web,
        Android,
        iOS,
        Windows,
        MacOS,
    }

    public class BugReport
    {

        public string Title { get; set; } = String.Empty;

        public string Description { get; set; } = String.Empty;

        public Activity LastActivity { get; set; } 
    }

    public class ClientDetails
    {
        public string Activation { get; set; } 

        public ConversationType ConversationType { get; set; } = ConversationType.Unknown;

        public Platform Platform { get; set; } = Platform.Web;
    }
}
