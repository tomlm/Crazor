
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Microsoft.AspNetCore.Razor.TagHelpers;
using AdaptiveCards;
using System;
using System.ComponentModel;

namespace Crazor.TagHelpers
{

    /// <summary>
    /// TagHelper for Card
    /// </summary>

    [HtmlTargetElement("Card")]
    public class CardTagHelper : ReflectionTagHelper
    {

        [HtmlAttributeName(nameof(FallbackText))]
        [DefaultValue(null)]
        public String FallbackText { get; set; } 

        [HtmlAttributeName(nameof(Lang))]
        [DefaultValue(null)]
        public String Lang { get; set; } 

        [HtmlAttributeName(nameof(MinHeight))]
        [DefaultValue(null)]
        public String MinHeight { get; set; } 

        [HtmlAttributeName(nameof(Rtl))]
        [DefaultValue(false)]
        public Boolean Rtl { get; set; } 

        [HtmlAttributeName(nameof(Speak))]
        [DefaultValue(null)]
        public String Speak { get; set; } 

        [HtmlAttributeName(nameof(Style))]
        [DefaultValue(typeof(AdaptiveContainerStyle), "Default")]
        public AdaptiveContainerStyle Style { get; set; } 

        [HtmlAttributeName(nameof(Title))]
        [DefaultValue(null)]
        public String Title { get; set; } 

        [HtmlAttributeName(nameof(VerticalContentAlignment))]
        [DefaultValue(typeof(AdaptiveVerticalContentAlignment), "Top")]
        public AdaptiveVerticalContentAlignment VerticalContentAlignment { get; set; } 

        [HtmlAttributeName(nameof(Height))]
        [DefaultValue(null)]
        public String Height { get; set; } 

        [HtmlAttributeName(nameof(Version))]
        [DefaultValue(null)]
        public String Version { get; set; } 
    }
}
