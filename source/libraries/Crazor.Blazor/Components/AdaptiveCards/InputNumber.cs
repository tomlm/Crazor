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
    /// Component for Input.Number
    /// </summary>

    public class InputNumber : Input<AdaptiveNumberInput>
    {

        [Parameter]
        public Boolean? IsVisible { get => Item.IsVisible; set => Item.IsVisible = value ?? true; }

        [Parameter]
        public Double? Max { get => Item.Max == Double.NaN ? null : Item.Max; set => Item.Max = value ?? Double.NaN; }

        [Parameter]
        public Double? Min { get => Item.Min == Double.NaN ? null : Item.Min; set => Item.Min = value ?? Double.NaN; }

        [Parameter]
        public String Placeholder { get => Item.Placeholder; set => Item.Placeholder = value; }

        [Parameter]
        public Boolean? Separator { get => Item.Separator; set => Item.Separator = value ?? false; }

        [Parameter]
        [DefaultValue(typeof(AdaptiveSpacing), "Default")]
        public AdaptiveSpacing Spacing { get => Item.Spacing; set => Item.Spacing = value; }

        [Parameter]
        [Binding(BindingType.Value)]
        public Double? Value { get => Item.Value == Double.NaN ? null : Item.Value; set => Item.Value = value ?? Double.NaN; }

        [Parameter]
        public String Height { get => Item.Height.ToString(); set => Item.Height = value; }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            // if we don't have required, but binding property has [Required] then set it
            var rangeAttribute = BindingProperty?.GetCustomAttribute<RangeAttribute>();
            if (this.Min == null && rangeAttribute?.Minimum != null)
            {
                this.Min = Convert.ToDouble(rangeAttribute.Minimum);
            }

            if (this.Max == null && rangeAttribute?.Maximum != null)
            {
                this.Max = Convert.ToDouble(rangeAttribute.Maximum);
            }

            if (this.ErrorMessage == null && rangeAttribute?.ErrorMessage != null)
            {
                this.ErrorMessage = rangeAttribute?.ErrorMessage;
            }
        }

    }
}
