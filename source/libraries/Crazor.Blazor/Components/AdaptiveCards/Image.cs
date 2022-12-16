// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using AdaptiveCards;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;

namespace Crazor.Blazor.Components.AdaptiveCards
{

    /// <summary>
    /// Component for Image
    /// </summary>

    public class Image : ElementComponent<AdaptiveImage>
    {

        [Parameter]
        public String AltText { get => Item.AltText ; set=>Item.AltText =value; } 

        [Parameter]
        public String BackgroundColor { get; set; } 

        [Parameter]
        [DefaultValue(typeof(AdaptiveHorizontalAlignment), "Left")]
        public AdaptiveHorizontalAlignment HorizontalAlignment { get => Item.HorizontalAlignment; set => Item.HorizontalAlignment = value; } 

        [Parameter]
        public Boolean? IsVisible { get => Item.IsVisible ; set => Item.IsVisible = value ?? true; }  

        [Parameter]
        public Boolean? Separator { get => Item.Separator; set => Item.Separator = value ?? false; } 

        [Parameter]
        [DefaultValue(typeof(AdaptiveImageSize), "Auto")]
        public AdaptiveImageSize Size { get; set; } 

        [Parameter]
        [DefaultValue(typeof(AdaptiveSpacing), "Default")]
        public AdaptiveSpacing Spacing { get => Item.Spacing; set => Item.Spacing = value; } 

        [Parameter]
        public String Speak { get; set; } 

        [Parameter]
        [DefaultValue(typeof(AdaptiveImageStyle), "Normal")]
        public AdaptiveImageStyle Style { get; set; } 

        [Parameter]
        public String Url { get; set; } 

        [Parameter]
        public String Height { get => Item.Height.ToString(); set => Item.Height = value; } 

        [Parameter]
        public String Width { get => Item.Width.ToString(); set => Item.Width = value; } 
    }
}
