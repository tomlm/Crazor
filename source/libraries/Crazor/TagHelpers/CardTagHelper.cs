
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Microsoft.AspNetCore.Razor.TagHelpers;
using AdaptiveCards;
using System;
using System.ComponentModel;
using Crazor.Attributes;

namespace Crazor.TagHelpers
{

    /// <summary>
    /// TagHelper for Card
    /// </summary>

    [HtmlTargetElement("Card")]
    public class CardTagHelper : ReflectionTagHelper
    {

        [HtmlAttributeName(nameof(FallbackText))]
        public String FallbackText { get; set; } 

        [HtmlAttributeName(nameof(Lang))]
        public String Lang { get; set; } 

        [HtmlAttributeName(nameof(MinHeight))]
        public String MinHeight { get; set; } 

        [HtmlAttributeName(nameof(Rtl))]
        public Boolean? Rtl { get; set; } 

        [HtmlAttributeName(nameof(Speak))]
        public String Speak { get; set; } 

        [HtmlAttributeName(nameof(Style))]
        [DefaultValue(typeof(AdaptiveContainerStyle), "Default")]
        public AdaptiveContainerStyle Style { get; set; } 

        [HtmlAttributeName(nameof(Title))]
        public String Title { get; set; } 

        [HtmlAttributeName(nameof(VerticalContentAlignment))]
        [DefaultValue(typeof(AdaptiveVerticalContentAlignment), "Top")]
        public AdaptiveVerticalContentAlignment VerticalContentAlignment { get; set; } 

        [HtmlAttributeName(nameof(Height))]
        public String Height { get; set; } 

        [HtmlAttributeName(nameof(Version))]
        public String Version { get; set; } 
    }
}
