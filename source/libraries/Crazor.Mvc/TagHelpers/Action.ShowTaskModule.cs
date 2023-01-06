// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Microsoft.AspNetCore.Razor.TagHelpers;
using Newtonsoft.Json.Linq;

namespace Crazor.Mvc.TagHelpers
{

    /// <summary>
    /// TagHelper for showing a card in a task module
    /// </summary>
    [HtmlTargetElement("Action.ShowTaskModule")]
    public class ActionShowTaskModuleTagHelper : ActionSubmitTagHelper
    {
        public ActionShowTaskModuleTagHelper()
        {
        }

        [HtmlAttributeName]
        public string Route { get; set; }

        public async override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            await base.ProcessAsync(context, output);

            output.TagName = "Action.Submit";
            output.TagMode = TagMode.StartTagAndEndTag;
            var content = output.Content.GetContent();
            dynamic data = String.IsNullOrWhiteSpace(content) ? new JObject() : JObject.Parse(content);
            data.commandId = this.Route;
            data.msteams = new JObject();
            data.msteams.type = "task/fetch";
            output.Content.SetHtmlContent((string)data.ToString());
            output.Attributes.RemoveAll(nameof(Route));
        }
    }
}
