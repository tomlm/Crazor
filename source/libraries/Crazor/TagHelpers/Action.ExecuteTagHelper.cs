
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Microsoft.AspNetCore.Razor.TagHelpers;
using AdaptiveCards;
using System;
using System.ComponentModel;
using Crazor.Attributes;

namespace Crazor.TagHelpers
{

    /// <summary>
    /// TagHelper for Action.Execute
    /// </summary>

    [HtmlTargetElement("Action.Execute")]
    public class ActionExecuteTagHelper : ReflectionTagHelper
    {

        [HtmlAttributeName(nameof(AssociatedInputs))]
        [DefaultValue(typeof(AdaptiveAssociatedInputs), "Auto")]
        public AdaptiveAssociatedInputs AssociatedInputs { get; set; } 

        [HtmlAttributeName(nameof(IconUrl))]
        [DefaultValue(null)]
        public String IconUrl { get; set; } 

        [HtmlAttributeName(nameof(IsEnabled))]
        [DefaultValue(true)]
        public Boolean IsEnabled { get; set; }  = true;

        [HtmlAttributeName(nameof(Mode))]
        [DefaultValue(typeof(AdaptiveActionMode), "Primary")]
        public AdaptiveActionMode Mode { get; set; } 

        [HtmlAttributeName(nameof(Speak))]
        [DefaultValue(null)]
        public String Speak { get; set; } 

        [HtmlAttributeName(nameof(Style))]
        [DefaultValue(typeof(AdaptiveActionStyle), "Default")]
        public AdaptiveActionStyle Style { get; set; } 

        [HtmlAttributeName(nameof(Title))]
        [DefaultValue(null)]
        public String Title { get; set; } 

        [HtmlAttributeName(nameof(Tooltip))]
        [DefaultValue(null)]
        public String Tooltip { get; set; } 

        [HtmlAttributeName(nameof(Verb))]
        [DefaultValue(null)]
        public String Verb { get; set; } 
    }
}