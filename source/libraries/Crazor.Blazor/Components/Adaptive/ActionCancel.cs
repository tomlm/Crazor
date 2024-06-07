﻿using AdaptiveCards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crazor.Blazor.Components.Adaptive
{
    /// <summary>
    /// Cancel the current view (this is just semantic sugar over closing current view and go show caller view)
    /// </summary>
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
