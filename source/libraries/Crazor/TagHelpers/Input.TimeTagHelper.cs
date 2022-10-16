
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
    /// TagHelper for Input.Time
    /// </summary>

    [HtmlTargetElement("Input.Time")]
    public class InputTimeTagHelper : InputTagHelper
    {


        [HtmlAttributeName(nameof(IsVisible))]
        public Boolean? IsVisible { get; set; }  

        [HtmlAttributeName(nameof(Max))]
        [DefaultValue(null)]
        public String Max { get; set; } 

        [HtmlAttributeName(nameof(Min))]
        [DefaultValue(null)]
        public String Min { get; set; } 

        [HtmlAttributeName(nameof(Placeholder))]
        [DefaultValue(null)]
        public String Placeholder { get; set; } 

        [HtmlAttributeName(nameof(Separator))]
        [DefaultValue(false)]
        public Boolean? Separator { get; set; } 

        [HtmlAttributeName(nameof(Spacing))]
        [DefaultValue(typeof(AdaptiveSpacing), "Default")]
        public AdaptiveSpacing Spacing { get; set; } 

        [HtmlAttributeName(nameof(Speak))]
        [DefaultValue(null)]
        public String Speak { get; set; } 

        [HtmlAttributeName(nameof(Value))]
        [DefaultValue(null)]
        [Binding(BindingType.Value)]
        public String Value { get; set; } 

        [HtmlAttributeName(nameof(Height))]
        [DefaultValue(null)]
        public String Height { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            await base.ProcessAsync(context, output);

            // if we don't have required, but binding property has [Required] then set it
            var rangeAttribute = BindingProperty.GetCustomAttribute<RangeAttribute>();
            if (IfValidation() && output.Attributes[nameof(Min)] == null && rangeAttribute?.Minimum != null)
            {
                output.Attributes.SetAttribute(nameof(Min), Convert.ToDateTime(rangeAttribute.Minimum));
            }

            if (IfValidation() && output.Attributes[nameof(Max)] == null && rangeAttribute?.Maximum != null)
            {
                output.Attributes.SetAttribute(nameof(Max), Convert.ToDateTime(rangeAttribute.Maximum));
            }

            if (IfValidation() && output.Attributes[nameof(ErrorMessage)] == null && rangeAttribute?.ErrorMessage != null)
            {
                output.Attributes.SetAttribute(nameof(ErrorMessage), rangeAttribute?.ErrorMessage);
            }
        }
    }
}
