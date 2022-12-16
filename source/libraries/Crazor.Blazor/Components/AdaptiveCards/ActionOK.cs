// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.


namespace Crazor.Blazor.Components.AdaptiveCards
{

    /// <summary>
    /// Component for Action.Execute to close current view and return the results to caller.
    /// </summary>
    /// <remarks>Default behavior is to call CloseView(GetModel()) </remarks>
    public class ActionOK : ActionExecute
    {
        public ActionOK()
        {
            this.Title = "OK";
            this.Verb = Constants.OK_VERB;
        }
    }
}
