
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
        internal const string Format = "HH:mm";

        [HtmlAttributeName(nameof(IsVisible))]
        public Boolean? IsVisible { get; set; }  

        [HtmlAttributeName(nameof(Max))]
        public String Max { get; set; } 

        [HtmlAttributeName(nameof(Min))]
        public String Min { get; set; } 

        [HtmlAttributeName(nameof(Placeholder))]
        public String Placeholder { get; set; } 

        [HtmlAttributeName(nameof(Separator))]
        [DefaultValue(false)]
        public Boolean? Separator { get; set; } 

        [HtmlAttributeName(nameof(Spacing))]
        [DefaultValue(typeof(AdaptiveSpacing), "Default")]
        public AdaptiveSpacing Spacing { get; set; } 

        [HtmlAttributeName(nameof(Speak))]
        public String Speak { get; set; } 

        [HtmlAttributeName(nameof(Value))]
        [Binding(BindingType.Value)]
        public String Value { get; set; } 

        [HtmlAttributeName(nameof(Height))]
        public String Height { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            await base.ProcessAsync(context, output);

            // make sure value is output in correct format 
            if (output.Attributes[nameof(Value)] != null)
            {
                var value = output.Attributes[nameof(Value)].Value;
                if (value is string str)
                {
                    output.Attributes.SetAttribute(nameof(Value), DateTime.Parse(str).ToString(Format));
                }
                else if (value is DateTime dt)
                {
                    output.Attributes.SetAttribute(nameof(Value), dt.ToString(Format));
                }
            }

            // if we don't have required, but binding property has [Required] then set it
            var rangeAttribute = BindingProperty?.GetCustomAttribute<RangeAttribute>();
            if (output.Attributes[nameof(Min)] == null && rangeAttribute?.Minimum != null)
            {
                output.Attributes.SetAttribute(nameof(Min), DateTime.Parse((string)rangeAttribute.Minimum).ToString(Format));
            }

            if (output.Attributes[nameof(Max)] == null && rangeAttribute?.Maximum != null)
            {
                output.Attributes.SetAttribute(nameof(Max), DateTime.Parse((string)rangeAttribute.Maximum).ToString(Format));
            }

            if (output.Attributes[nameof(ErrorMessage)] == null && rangeAttribute?.ErrorMessage != null)
            {
                output.Attributes.SetAttribute(nameof(ErrorMessage), rangeAttribute?.ErrorMessage);
            }
        }
    }
}
