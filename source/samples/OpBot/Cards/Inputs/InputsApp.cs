using Crazor;
using Crazor.Attributes;

namespace OpBot.Cards.Inputs
{
    public class InputsApp : CardApp
    {
        public InputsApp(IServiceProvider services)
            : base(services)
        {
        }

        [SharedMemory]
        public InputsModel Data { get; set; } = new InputsModel();
    }
}
