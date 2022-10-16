
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Microsoft.AspNetCore.Razor.TagHelpers;
using AdaptiveCards;
using System;
using System.ComponentModel;
using Crazor.Attributes;

namespace Crazor.TagHelpers
{

    /// <summary>
    /// TagHelper for Data.Query
    /// </summary>

    [HtmlTargetElement("Data.Query", ParentTag ="Input.ChoiceSet")]
    public class DataQueryTagHelper : ReflectionTagHelper
    {

        [HtmlAttributeName(nameof(Count))]
        public Int32? Count { get; set; } 

        [HtmlAttributeName(nameof(Dataset))]
        [DefaultValue(null)]
        public String Dataset { get; set; } 

        [HtmlAttributeName(nameof(Skip))]
        public Int32? Skip { get; set; } 

        [HtmlAttributeName(nameof(Value))]
        [DefaultValue(null)]
        public String Value { get; set; } 
    }
}
