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

    [HtmlTargetElement("ValidationSummary")]
    public class ValidationSummaryTagHelper : ReflectionTagHelper
    {
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var validationError in View.ValidationErrors)
            {
                foreach (var error in validationError.Value)
                {
                    sb.AppendLine($"<TextBlock Spacing=\"None\" Color=\"Attention\">{error}</TextBlock>");
                }
            }

            output.TagName = null;
            output.Content.SetHtmlContent(sb.ToString());

            return Task.CompletedTask;
        }
    }
}
