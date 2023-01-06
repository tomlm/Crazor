// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using AdaptiveCards;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Crazor.Mvc.TagHelpers
{

    /// <summary>
    /// TagHelper for Action.Execute with Cancel verb (associatedInputs = None and
    /// </summary>

    [HtmlTargetElement("Action.Cancel")]
    public class ActionCancelTagHelper : ActionExecuteTagHelper
    {
        public ActionCancelTagHelper()
        {
            this.AssociatedInputs = AdaptiveAssociatedInputs.None;
            this.Title = "Cancel";
            this.Verb = Constants.CANCEL_VERB;
        }

        public async override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            await base.ProcessAsync(context, output);

            output.TagName = "Action.Execute";
        }
    }
}
