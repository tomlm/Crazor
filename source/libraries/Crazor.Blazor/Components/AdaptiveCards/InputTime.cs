// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using AdaptiveCards;
using Crazor.Attributes;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Crazor.Blazor.Components.AdaptiveCards
{

    /// <summary>
    /// Component for Input.Time
    /// </summary>

    public class InputTime : Input<AdaptiveTimeInput>
    {
        internal const string Format = "HH:mm";

        [Parameter]
        public Boolean? IsVisible { get => Item.IsVisible ; set => Item.IsVisible = value ?? true; }  

        [Parameter]
        public String Max { get => Item.Max; set => Item.Max = value; }

        [Parameter]
        public String Min { get => Item.Min; set => Item.Min = value; }

        [Parameter]
        public String Placeholder { get => Item.Placeholder ; set=> Item.Placeholder  = value; } 

        [Parameter]
        [DefaultValue(false)]
        public Boolean? Separator { get => Item.Separator; set => Item.Separator = value ?? false; } 

        [Parameter]
        [DefaultValue(typeof(AdaptiveSpacing), "Default")]
        public AdaptiveSpacing Spacing { get => Item.Spacing; set => Item.Spacing = value; } 

        [Parameter]
        [Binding(BindingType.Value)]
        public String Value { get => Item.Value; set => Item.Value = value; } 

        [Parameter]
        public String Height { get => Item.Height.ToString(); set => Item.Height = value; }

        public override async Task ProcessAsync(ComponentContext context, ComponentOutput output)
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
