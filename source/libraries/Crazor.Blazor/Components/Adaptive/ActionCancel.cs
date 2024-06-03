using AdaptiveCards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crazor.Blazor.Components.Adaptive
{
    public class ActionCancel : ActionExecute
    {
        public ActionCancel()
        {
            this.Title = "Cancel";
            this.Verb = Constants.CANCEL_VERB;
            this.AssociatedInputs = AdaptiveAssociatedInputs.None;
        }
    }
}
