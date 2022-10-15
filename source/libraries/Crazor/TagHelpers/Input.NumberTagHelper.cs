
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Microsoft.AspNetCore.Razor.TagHelpers;
using AdaptiveCards;
using System;
using System.ComponentModel;
using Crazor.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Crazor.TagHelpers
{

    /// <summary>
    /// TagHelper for Input.Number
    /// </summary>

    [HtmlTargetElement("Input.Number")]
    public class InputNumberTagHelper : InputTagHelper
    {

        [HtmlAttributeName(nameof(IsVisible))]
        [DefaultValue(true)]
        public Boolean IsVisible { get; set; }  = true;

        [HtmlAttributeName(nameof(Max))]
        [DefaultValue(Double.NaN)]
        public Double Max { get; set; } 

        [HtmlAttributeName(nameof(Min))]
        [DefaultValue(Double.NaN)]
        public Double Min { get; set; } 

        [HtmlAttributeName(nameof(Placeholder))]
        [DefaultValue(null)]
        public String Placeholder { get; set; } 

        [HtmlAttributeName(nameof(Separator))]
        [DefaultValue(false)]
        public Boolean Separator { get; set; } 

        [HtmlAttributeName(nameof(Spacing))]
        [DefaultValue(typeof(AdaptiveSpacing), "Default")]
        public AdaptiveSpacing Spacing { get; set; } 

        [HtmlAttributeName(nameof(Speak))]
        [DefaultValue(null)]
        public String Speak { get; set; } 

        [HtmlAttributeName(nameof(Value))]
        [DefaultValue(Double.NaN)]
        [Binding(BindingType.Value)]
        public Double Value { get; set; } 

        [HtmlAttributeName(nameof(Height))]
        [DefaultValue(null)]
        public String Height { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            await base.ProcessAsync(context, output);

            // if we don't have required, but binding property has [Required] then set it
            var rangeAttribute = BindingProperty.GetCustomAttribute<RangeAttribute>();
            if (output.Attributes[nameof(Min)] == null && rangeAttribute?.Minimum != null)
            {
                output.Attributes.SetAttribute(nameof(Min), Convert.ToDouble(rangeAttribute.Minimum));
            }

            if (output.Attributes[nameof(Max)] == null && rangeAttribute?.Maximum != null)
            {
                output.Attributes.SetAttribute(nameof(Max), Convert.ToDouble(rangeAttribute.Maximum));
            }

            if (output.Attributes[nameof(ErrorMessage)] == null && rangeAttribute?.ErrorMessage != null)
            {
                output.Attributes.SetAttribute(nameof(Min), rangeAttribute?.ErrorMessage);
            }
        }

    }
}
