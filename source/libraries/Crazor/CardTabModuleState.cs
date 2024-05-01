
using AdaptiveCards;

namespace Crazor
{
    internal class CardTabModuleState
    {
        public Dictionary<string, AdaptiveExecuteAction> RefreshMap { get; set; } = new Dictionary<string, AdaptiveExecuteAction>();
    }
}
