// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using AdaptiveCards;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;

namespace Crazor.Blazor.Components.AdaptiveCards
{

    /// <summary>
    /// Component for FactSet
    /// </summary>

    public class FactSet : ElementComponent<AdaptiveFactSet>
    {

        [Parameter]
        public Boolean? IsVisible { get => Item.IsVisible ; set => Item.IsVisible = value ?? true; }  

        [Parameter]
        public Boolean? Separator { get => Item.Separator; set => Item.Separator = value ?? false; } 

        [Parameter]
        [DefaultValue(typeof(AdaptiveSpacing), "Default")]
        public AdaptiveSpacing Spacing { get => Item.Spacing; set => Item.Spacing = value; } 

        [Parameter]
        public String Height { get => Item.Height.ToString(); set => Item.Height = value; } 
    }
}
