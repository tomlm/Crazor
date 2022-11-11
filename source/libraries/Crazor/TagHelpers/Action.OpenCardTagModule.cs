
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Microsoft.AspNetCore.Razor.TagHelpers;
using AdaptiveCards;
using Newtonsoft.Json.Linq;
using System.Reflection.Emit;
using Microsoft.Bot.Connector;
using System.ComponentModel;

namespace Crazor.TagHelpers
{

    /// <summary>
    /// TagHelper for opening a card appropriate to channel (either openUrl or Submit to show task module)
    /// </summary>
    [HtmlTargetElement("Action.OpenCard")]
    public class ActionOpenCardTagHelper : ActionOpenUrlTagHelper
    {
        public ActionOpenCardTagHelper()
        {
        }

        [HtmlAttributeName(nameof(ChannelId))]
        public string ChannelId { get; set; }

        public async override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            await base.ProcessAsync(context, output);

            if (ChannelId == Channels.Msteams)
            {
                output.TagName = "Action.Submit";
                output.TagMode = TagMode.StartTagAndEndTag;
                dynamic data = new JObject();
                data.commandId = new Uri(this.Url).PathAndQuery;
                data.msteams = new JObject();
                data.msteams.type = "task/fetch";
                output.Content.SetHtmlContent((string)data.ToString());
                output.Attributes.RemoveAll(nameof(Url));
            }
            else
            {
                output.TagName = "Action.OpenUrl";
                output.TagMode = TagMode.SelfClosing;
                output.Attributes.RemoveAll(nameof(ChannelId));
            }
        }
    }
}
