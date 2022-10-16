
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Microsoft.AspNetCore.Razor.TagHelpers;
using AdaptiveCards;
using System;
using System.ComponentModel;
using Crazor.Attributes;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace Crazor.TagHelpers
{

    /// <summary>
    /// TagHelper for Input.ChoiceSet
    /// </summary>

    [HtmlTargetElement("Input.ChoiceSet")]
    [RestrictChildren("Choice", "Data.Query")]
    public class InputChoiceSetTagHelper : InputTagHelper
    {

        [HtmlAttributeName(nameof(IsMultiSelect))]
        public Boolean? IsMultiSelect { get; set; }

        [HtmlAttributeName(nameof(IsVisible))]
        public Boolean? IsVisible { get; set; } 

        [HtmlAttributeName(nameof(Placeholder))]
        public String Placeholder { get; set; }

        [HtmlAttributeName(nameof(Separator))]
        public Boolean? Separator { get; set; }

        [HtmlAttributeName(nameof(Spacing))]
        [DefaultValue(typeof(AdaptiveSpacing), "Default")]
        public AdaptiveSpacing Spacing { get; set; }

        [HtmlAttributeName(nameof(Speak))]
        public String Speak { get; set; }

        [HtmlAttributeName(nameof(Style))]
        [DefaultValue(typeof(AdaptiveChoiceInputStyle), "Compact")]
        public AdaptiveChoiceInputStyle Style { get; set; }

        [HtmlAttributeName(nameof(Value))]
        [Binding(BindingType.Value)]
        public String Value { get; set; }

        [HtmlAttributeName(nameof(Wrap))]
        public Boolean? Wrap { get; set; }

        [HtmlAttributeName(nameof(Height))]
        public String Height { get; set; }

    }
}
