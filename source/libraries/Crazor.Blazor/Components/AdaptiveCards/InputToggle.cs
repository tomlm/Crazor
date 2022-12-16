// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using AdaptiveCards;
using Crazor.Attributes;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;

namespace Crazor.Blazor.Components.AdaptiveCards
{

    /// <summary>
    /// Component for Input.Toggle
    /// </summary>

    public class InputToggle : Input<AdaptiveToggleInput>
    {
        [Parameter]
        public Boolean? IsVisible { get => Item.IsVisible; set => Item.IsVisible = value ?? true; }

        [Parameter]
        public Boolean? Separator { get => Item.Separator; set => Item.Separator = value ?? false; }

        [Parameter]
        [DefaultValue(typeof(AdaptiveSpacing), "Default")]
        public AdaptiveSpacing Spacing { get => Item.Spacing; set => Item.Spacing = value; }

        [Parameter]
        public String Title { get => Item.Title; set => Item.Title = value; }

        [Parameter]
        [Binding(BindingType.Value)]
        public String Value { get => Item.Value; set => Item.Value = value; }

        [Parameter]
        public String ValueOff { get => Item.ValueOff; set => Item.ValueOff = value; }

        [Parameter]
        public String ValueOn { get => Item.ValueOn; set => Item.ValueOn = value; }

        [Parameter]
        public Boolean? Wrap { get => Item.Wrap; set => Item.Wrap = value ?? false; }

        [Parameter]
        public String Height { get => Item.Height.ToString(); set => Item.Height = value; }
    }
}
