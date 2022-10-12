using Crazor;
using Crazor.Attributes;

namespace SampleWebApp.Cards.HelloWorld
{
    public class HelloWorldApp : CardApp
    {
        public HelloWorldApp(IServiceProvider services)
            : base(services)
        {
        }

        // [SharedMemory] // property value is shared with all viewers.
        // [SessionMemory] // property value is for each viewer
        // public int Counter { get; set; } = 0;
    }
}
