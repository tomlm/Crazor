using Crazor;
using Crazor.Attributes;

namespace CrazorTests.Cards.Memory
{
    public class MemoryApp : CardApp
    {
        public MemoryApp(IServiceProvider services) : base(services)
        {
        }

        [AppMemory]
        public string? App { get; set; }

        [SessionMemory]
        public string? Session { get; set; }

        [UserMemory]
        public string? User { get; set; }

        [ConversationMemory]
        public string? Conversation { get; set; }

        [PathMemory("Value")]
        public string? Path { get; set; }

        [TempMemory]
        public string? Temp { get; set; }

        public string Value { get; set; } = "Value";
    }
}