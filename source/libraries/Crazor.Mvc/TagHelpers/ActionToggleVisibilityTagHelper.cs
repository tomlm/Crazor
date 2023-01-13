// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using AdaptiveCards;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.ComponentModel;

namespace Crazor.Mvc.TagHelpers
{

    /// <summary>
    /// TagHelper for Action.ToggleVisibility
    /// </summary>

    [HtmlTargetElement("ActionToggleVisibility")]
    [RestrictChildren("TargetElement")]
    public class ActionToggleVisibilityTagHelper : ReflectionTagHelper
    {

        [HtmlAttributeName(nameof(IconUrl))]
        public String IconUrl { get; set; }

        [HtmlAttributeName(nameof(IsEnabled))]
        public Boolean? IsEnabled { get; set; }

        [HtmlAttributeName(nameof(Mode))]
        [DefaultValue(typeof(AdaptiveActionMode), "Primary")]
        public AdaptiveActionMode Mode { get; set; }

        [HtmlAttributeName(nameof(Speak))]
        public String Speak { get; set; }

        [HtmlAttributeName(nameof(Style))]
        [DefaultValue(typeof(AdaptiveActionStyle), "Default")]
        public AdaptiveActionStyle Style { get; set; }

        [HtmlAttributeName(nameof(Title))]
        public String Title { get; set; }

        [HtmlAttributeName(nameof(Tooltip))]
        public String Tooltip { get; set; }

        public async override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            await base.ProcessAsync(context, output);

            output.TagName = "Action.ToggleVisibility";
        }

    }
}
