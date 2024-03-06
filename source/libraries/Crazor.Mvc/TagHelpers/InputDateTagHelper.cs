// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Crazor.AdaptiveCards;
using Crazor.Attributes;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Crazor.Mvc.TagHelpers
{

    /// <summary>
    /// TagHelper for Input.Date
    /// </summary>

    [HtmlTargetElement("InputDate")]
    public class InputDateTagHelper : InputTagHelper
    {
        internal const string Format = "yyyy-MM-dd";

        [HtmlAttributeName(nameof(IsVisible))]
        public Boolean? IsVisible { get; set; }

        [HtmlAttributeName(nameof(Max))]
        public DateTime? Max { get; set; }

        [HtmlAttributeName(nameof(Min))]
        public DateTime? Min { get; set; }

        [HtmlAttributeName(nameof(Placeholder))]
        public String Placeholder { get; set; }

        [HtmlAttributeName(nameof(Separator))]
        public Boolean? Separator { get; set; }

        [HtmlAttributeName(nameof(Spacing))]
        [DefaultValue(typeof(AdaptiveSpacing), "Default")]
        public AdaptiveSpacing Spacing { get; set; }

        [HtmlAttributeName(nameof(Speak))]
        public String Speak { get; set; }

        [HtmlAttributeName(nameof(Value))]
        [Binding(BindingType.Value)]
        public DateTime? Value { get; set; }

        [HtmlAttributeName(nameof(Height))]
        public String Height { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            await base.ProcessAsync(context, output);
            
            output.TagName = "Input.Date";


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

            if (ClientValidation == null || ClientValidation == true)
            {
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
}
