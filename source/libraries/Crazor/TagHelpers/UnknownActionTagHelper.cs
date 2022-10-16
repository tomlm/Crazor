
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Microsoft.AspNetCore.Razor.TagHelpers;
using AdaptiveCards;
using System;
using System.ComponentModel;
using Crazor.Attributes;

namespace Crazor.TagHelpers
{

    /// <summary>
    /// TagHelper for UnknownAction
    /// </summary>

    [HtmlTargetElement("Action.Unknown")]
    public class UnknownActionTagHelper : ReflectionTagHelper
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
    }
}
