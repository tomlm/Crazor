
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Microsoft.AspNetCore.Razor.TagHelpers;
using AdaptiveCards;

namespace Crazor.TagHelpers
{

    /// <summary>
    /// TagHelper for Action.Execute with Cancel verb (associatedInputs = None and
    /// </summary>

    [HtmlTargetElement("Action.Close")]
    public class ActionCloseTagHelper : ActionExecuteTagHelper
    {
        public ActionCloseTagHelper()
        {
            this.Verb = Constants.CLOSE_VERB;
        }

        public async override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            await base.ProcessAsync(context, output);

            output.TagName = "Action.Execute";
        }
    }
}
