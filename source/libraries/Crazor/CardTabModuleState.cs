using AdaptiveCards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crazor
{
    internal class CardTabModuleState
    {
        public Dictionary<string, AdaptiveExecuteAction> RefreshMap { get; set; } = new Dictionary<string, AdaptiveExecuteAction>();
    }
}
