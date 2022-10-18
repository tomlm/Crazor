
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Microsoft.AspNetCore.Razor.TagHelpers;
using AdaptiveCards;

namespace Crazor.TagHelpers
{

    /// <summary>
    /// TagHelper for Action.Execute to close current view and return the results to caller.
    /// </summary>
    /// <remarks>Default behavior is to call CloseView(GetModel()) </remarks>
    [HtmlTargetElement("Action.OK")]
    public class ActionOKTagHelper : ActionExecuteTagHelper
    {
        public ActionOKTagHelper()
        {
            this.Title = "OK";
            this.Verb = Constants.CLOSE_VERB;
        }

        public async override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            await base.ProcessAsync(context, output);

            output.TagName = "Action.Execute";
        }
    }
}
