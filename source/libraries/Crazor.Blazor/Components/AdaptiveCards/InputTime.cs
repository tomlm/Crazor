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
        public DateTime? Min { get => Item.Min != null ? DateTime.Parse(Item.Min) : null; set => Item.Min = value.Value.ToString(Format); }

        [Parameter]
        public DateTime? Max { get => Item.Max != null ? DateTime.Parse(Item.Max) : null; set => Item.Max = value.Value.ToString(Format); }

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
        public DateTime? Value { get => Item.Value != null ? DateTime.Parse(Item.Value) : null; set => Item.Value = value.Value.ToString(Format); }

        [Parameter]
        public String Height { get => Item.Height.ToString(); set => Item.Height = value; }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            // if we don't have required, but binding property has [Required] then set it
            var rangeAttribute = BindingProperty?.GetCustomAttribute<RangeAttribute>();
            if (this.Min == null && rangeAttribute?.Minimum != null)
            {
                this.Min = DateTime.Parse((string)rangeAttribute.Minimum);
            }

            if (this.Max == null && rangeAttribute?.Maximum != null)
            {
                this.Max = DateTime.Parse((string)rangeAttribute.Maximum);
            }

            if (this.ErrorMessage == null && rangeAttribute?.ErrorMessage != null)
            {
                this.ErrorMessage = rangeAttribute?.ErrorMessage;
            }
        }
    }
}
