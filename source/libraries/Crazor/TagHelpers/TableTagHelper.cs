
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Microsoft.AspNetCore.Razor.TagHelpers;
using AdaptiveCards;
using System;
using System.ComponentModel;
using Crazor.Attributes;

namespace Crazor.TagHelpers
{

    /// <summary>
    /// TagHelper for Table
    /// </summary>

    [HtmlTargetElement("Table")]
    [RestrictChildren("TableColumn", "TableRow")]
    public class TableTagHelper : ReflectionTagHelper
    {

        [HtmlAttributeName(nameof(FirstRowAsHeader))]
        public Boolean? FirstRowAsHeader { get; set; }  

        [HtmlAttributeName(nameof(GridStyle))]
        [DefaultValue(typeof(AdaptiveContainerStyle), "Default")]
        public AdaptiveContainerStyle GridStyle { get; set; } 

        [HtmlAttributeName(nameof(HorizontalCellContentAlignment))]
        [DefaultValue(typeof(AdaptiveHorizontalAlignment), "Left")]
        public AdaptiveHorizontalAlignment HorizontalCellContentAlignment { get; set; } 

        [HtmlAttributeName(nameof(IsVisible))]
        public Boolean? IsVisible { get; set; }  

        [HtmlAttributeName(nameof(Separator))]
        public Boolean? Separator { get; set; } 

        [HtmlAttributeName(nameof(ShowGridLines))]
        public Boolean? ShowGridLines { get; set; }  

        [HtmlAttributeName(nameof(Spacing))]
        [DefaultValue(typeof(AdaptiveSpacing), "Default")]
        public AdaptiveSpacing Spacing { get; set; } 

        [HtmlAttributeName(nameof(Speak))]
        public String Speak { get; set; } 

        [HtmlAttributeName(nameof(VerticalCellContentAlignment))]
        [DefaultValue(typeof(AdaptiveVerticalAlignment), "Top")]
        public AdaptiveVerticalAlignment VerticalCellContentAlignment { get; set; } 

        [HtmlAttributeName(nameof(Height))]
        public String Height { get; set; } 
    }
}
