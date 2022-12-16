// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using AdaptiveCards;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;

namespace Crazor.Blazor.Components.AdaptiveCards
{

    /// <summary>
    /// Component for Table
    /// </summary>

    public class Table : ElementComponent<AdaptiveTable>
    {

        [Parameter]
        public Boolean? FirstRowAsHeaders { get; set; }  

        [Parameter]
        [DefaultValue(typeof(AdaptiveContainerStyle), "Default")]
        public AdaptiveContainerStyle GridStyle { get; set; } 

        [Parameter]
        [DefaultValue(typeof(AdaptiveHorizontalAlignment), "Left")]
        public AdaptiveHorizontalAlignment HorizontalCellContentAlignment { get; set; } 

        [Parameter]
        public Boolean? IsVisible { get => Item.IsVisible ; set => Item.IsVisible = value ?? true; }  

        [Parameter]
        public Boolean? Separator { get => Item.Separator; set => Item.Separator = value ?? false; } 

        [Parameter]
        public Boolean? ShowGridLines { get; set; }  

        [Parameter]
        [DefaultValue(typeof(AdaptiveSpacing), "Default")]
        public AdaptiveSpacing Spacing { get => Item.Spacing; set => Item.Spacing = value; } 

        [Parameter]
        public String Speak { get; set; } 

        [Parameter]
        [DefaultValue(typeof(AdaptiveVerticalAlignment), "Top")]
        public AdaptiveVerticalAlignment VerticalCellContentAlignment { get; set; } 

        [Parameter]
        public String Height { get => Item.Height.ToString(); set => Item.Height = value; } 
    }
}
