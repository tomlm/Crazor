// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using AdaptiveCards;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;

namespace Crazor.Blazor.Components.AdaptiveCards
{

    /// <summary>
    /// Component for TextBlock
    /// </summary>

    public class TextBlock : ElementComponent<AdaptiveTextBlock>
    {

        [Parameter]
        [DefaultValue(typeof(AdaptiveTextColor), "Default")]
        public AdaptiveTextColor Color { get => Item.Color; set => Item.Color = value; }

        [Parameter]
        [DefaultValue(typeof(AdaptiveFontType), "Default")]
        public AdaptiveFontType FontType { get => Item.FontType; set => Item.FontType = value; }

        [Parameter]
        [DefaultValue(typeof(AdaptiveHorizontalAlignment), "Left")]
        public AdaptiveHorizontalAlignment HorizontalAlignment { get => Item.HorizontalAlignment; set => Item.HorizontalAlignment = value; }

        [Parameter]
        public Boolean? IsSubtle { get => Item.IsSubtle; set => Item.IsSubtle = value ?? false; }

        [Parameter]
        public Boolean? IsVisible { get => Item.IsVisible; set => Item.IsVisible = value ?? true; }

        [Parameter]
        public Int32? MaxLines { get => Item.MaxLines; set => Item.MaxLines = value ?? 0; }

        [Parameter]
        public Int32? MaxWidth { get => Item.MaxWidth; set => Item.MaxWidth = value ?? 0; }

        [Parameter]
        [DefaultValue(false)]
        public Boolean? Separator { get => Item.Separator; set => Item.Separator = value ?? false; }

        [Parameter]
        [DefaultValue(typeof(AdaptiveTextSize), "Default")]
        public AdaptiveTextSize Size { get => Item.Size; set => Item.Size = value; }

        [Parameter]
        [DefaultValue(typeof(AdaptiveSpacing), "Default")]
        public AdaptiveSpacing Spacing { get => Item.Spacing; set => Item.Spacing = value; }

        [Parameter]
        [DefaultValue(typeof(AdaptiveTextBlockStyle), "Default")]
        public AdaptiveTextBlockStyle Style { get => Item.Style; set => Item.Style = value; }

        [Parameter]
        public String Text { get => Item.Text; set => Item.Text = value; }

        [Parameter]
        [DefaultValue(typeof(AdaptiveTextWeight), "Normal")]
        public AdaptiveTextWeight Weight { get => Item.Weight; set => Item.Weight = value; }

        [Parameter]
        public Boolean? Wrap { get => Item.Wrap; set => Item.Wrap = value ?? false; }

        [Parameter]
        public String Height { get => Item.Height.ToString(); set => Item.Height = value; }
    }
}
