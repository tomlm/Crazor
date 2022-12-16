// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using AdaptiveCards;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;

namespace Crazor.Blazor.Components.AdaptiveCards
{

    /// <summary>
    /// Component for TextRun
    /// </summary>

    public class TextRun : ItemComponent<IList<AdaptiveTextRun>, AdaptiveTextRun>
    {

        [Parameter]
        [DefaultValue(typeof(AdaptiveTextColor), "Default")]
        public AdaptiveTextColor Color { get => Item.Color; set => Item.Color = value; }

        [Parameter]
        [DefaultValue(typeof(AdaptiveFontType), "Default")]
        public AdaptiveFontType FontType { get => Item.FontType; set => Item.FontType = value; }

        [Parameter]
        public Boolean? Highlight { get => Item.Highlight; set => Item.Highlight = value ?? false; }

        [Parameter]
        public Boolean? IsSubtle { get => Item.IsSubtle; set => Item.IsSubtle = value ?? false; }

        [Parameter]
        public Boolean? Italic { get => Item.Italic; set => Item.Italic = value ?? false; }

        [Parameter]
        [DefaultValue(typeof(AdaptiveTextSize), "Default")]
        public AdaptiveTextSize Size { get => Item.Size; set => Item.Size = value; }

        [Parameter]
        public Boolean? Strikethrough { get => Item.Strikethrough; set => Item.Strikethrough = value ?? false; }

        [Parameter]
        public Boolean? Underline { get => Item.Underline; set => Item.Underline = value ?? false; }

        [Parameter]
        [DefaultValue(typeof(AdaptiveTextWeight), "Normal")]
        public AdaptiveTextWeight Weight { get => Item.Weight; set => Item.Weight = value; }
    }
}
