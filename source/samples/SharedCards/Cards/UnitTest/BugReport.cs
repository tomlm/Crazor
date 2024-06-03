using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedCards.Cards.UnitTest
{
    public enum InsertionType
    {
        Unknown,
        Message,
        LinkUnfurling,
        TaskModuleInsert,
        ActionableMessage
    }

    public enum AppHost
    {
        Teams,
        Outlook, 
        Unknown
    }

    public enum ConversationType
    {
        Unknown,
        Personal,
        Channel,
        Email
    }

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

        public Activity Activity { get; set; }

        public string Title { get; set; }
        
        public string Description { get; set; }

        public ConversationType ConversationType { get; set; }

        public InsertionType InsertionType { get; set; } = InsertionType.Unknown;

        public AppHost AppHost { get; set; }

        public Platform Platform { get; set; }
    }
}
