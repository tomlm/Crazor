// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using AdaptiveCards;

namespace Crazor.Blazor.Components.AdaptiveCards
{

    /// <summary>
    /// Component for Action.Execute with Cancel verb (associatedInputs = None and
    /// </summary>

    public class ActionCancel : ActionExecute
    {
        public ActionCancel()
        {
            AssociatedInputs = AdaptiveAssociatedInputs.None;
            Title = "Cancel";
            Verb = Constants.CANCEL_VERB;
        }
    }
}
