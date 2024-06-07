using AdaptiveCards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crazor.Blazor.Components.Adaptive
{
    /// <summary>
    /// Close the current view and go show caller view
    /// </summary>
    public class ActionCloseView : ActionExecute
    {
        public ActionCloseView()
        {
            this.Title = "Close";
            this.Verb = Constants.CANCEL_VERB;
            this.AssociatedInputs = AdaptiveAssociatedInputs.None;
        }
    }
}
