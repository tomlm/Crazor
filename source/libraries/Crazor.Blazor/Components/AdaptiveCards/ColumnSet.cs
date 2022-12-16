// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using AdaptiveCards;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;

namespace Crazor.Blazor.Components.AdaptiveCards
{

    /// <summary>
    /// Component for ColumnSet
    /// </summary>

    public class ColumnSet : ElementComponent<AdaptiveColumnSet>
    {
        [Parameter]
        public Boolean? Bleed { get => Item.Bleed; set => Item.Bleed = value ?? false; }

        [Parameter]
        [DefaultValue(typeof(AdaptiveHorizontalAlignment), "Left")]
        public AdaptiveHorizontalAlignment HorizontalAlignment { get => Item.HorizontalAlignment; set => Item.HorizontalAlignment = value; } 

        [Parameter]
        public Boolean? IsVisible { get => Item.IsVisible ; set => Item.IsVisible = value ?? true; }  

        [Parameter]
        public String MinHeight { get => Item.MinHeight; set => Item.MinHeight = value; } 

        [Parameter]
        public Boolean? Separator { get => Item.Separator; set => Item.Separator = value ?? false; } 

        [Parameter]
        [DefaultValue(typeof(AdaptiveSpacing), "Default")]
        public AdaptiveSpacing Spacing { get => Item.Spacing; set => Item.Spacing = value; } 

        [Parameter]
        [DefaultValue(typeof(AdaptiveContainerStyle), "Default")]
        public AdaptiveContainerStyle Style { get => Item.Style; set => Item.Style = value ; } 

        [Parameter]
        [DefaultValue(typeof(AdaptiveVerticalAlignment), "Top")]
        public AdaptiveVerticalAlignment VerticalAlignment { get => Item.VerticalAlignment; set => Item.VerticalAlignment = value; } 

        [Parameter]
        [DefaultValue(typeof(AdaptiveVerticalContentAlignment), "Top")]
        public AdaptiveVerticalContentAlignment VerticalContentAlignment { get => Item.VerticalContentAlignment ; set => Item.VerticalContentAlignment  = value; } 

        [Parameter]
        public String Height { get => Item.Height.ToString(); set => Item.Height = value; } 
    }
}
