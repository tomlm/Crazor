
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Microsoft.AspNetCore.Razor.TagHelpers;
using AdaptiveCards;
using System;
using System.ComponentModel;

namespace Crazor.TagHelpers
{

    /// <summary>
    /// TagHelper for Table
    /// </summary>

    [HtmlTargetElement("Table")]
    public class TableTagHelper : ReflectionTagHelper
    {

        [HtmlAttributeName(nameof(FirstRowAsHeader))]
        [DefaultValue(true)]
        public Boolean FirstRowAsHeader { get; set; }  = true;

        [HtmlAttributeName(nameof(GridStyle))]
        [DefaultValue(typeof(AdaptiveContainerStyle), "Default")]
        public AdaptiveContainerStyle GridStyle { get; set; } 

        [HtmlAttributeName(nameof(HorizontalCellContentAlignment))]
        [DefaultValue(typeof(AdaptiveHorizontalAlignment), "Left")]
        public AdaptiveHorizontalAlignment HorizontalCellContentAlignment { get; set; } 

        [HtmlAttributeName(nameof(IsVisible))]
        [DefaultValue(true)]
        public Boolean IsVisible { get; set; }  = true;

        [HtmlAttributeName(nameof(Separator))]
        [DefaultValue(false)]
        public Boolean Separator { get; set; } 

        [HtmlAttributeName(nameof(ShowGridLines))]
        [DefaultValue(true)]
        public Boolean ShowGridLines { get; set; }  = true;

        [HtmlAttributeName(nameof(Spacing))]
        [DefaultValue(typeof(AdaptiveSpacing), "Default")]
        public AdaptiveSpacing Spacing { get; set; } 

        [HtmlAttributeName(nameof(Speak))]
        [DefaultValue(null)]
        public String Speak { get; set; } 

        [HtmlAttributeName(nameof(VerticalCellContentAlignment))]
        [DefaultValue(typeof(AdaptiveVerticalAlignment), "Top")]
        public AdaptiveVerticalAlignment VerticalCellContentAlignment { get; set; } 
    }
}
