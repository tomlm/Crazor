// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;

namespace Crazor.Mvc.TagHelpers
{

    /// <summary>
    /// TagHelper for ActionSet
    /// </summary>

    [HtmlTargetElement("ValidationMessage")]
    public class ValidationMessageTagHelper : ReflectionTagHelper
    {
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (!String.IsNullOrEmpty(this.Id) && View.ValidationErrors.TryGetValue(this.Id, out var errors))
            {
                StringBuilder sb = new StringBuilder();
                foreach (var error in errors)
                {
                    sb.AppendLine($"<TextBlock Spacing=\"None\" Color=\"Attention\">{error}</TextBlock>");
                }
                output.TagName = null;
                output.Content.SetHtmlContent(sb.ToString());
            }
            else
            {
                output.TagName = null;
            }

            return Task.CompletedTask;
        }
    }
}
