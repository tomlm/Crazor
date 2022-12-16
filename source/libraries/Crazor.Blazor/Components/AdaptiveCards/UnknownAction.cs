// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using AdaptiveCards;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;

namespace Crazor.Blazor.Components.AdaptiveCards
{

    /// <summary>
    /// Component for UnknownAction
    /// </summary>

    public class UnknownAction : ElementComponent
    {

        [Parameter]
        public String IconUrl { get; set; } 

        [Parameter]
        public Boolean? IsEnabled { get; set; }  

        [Parameter]
        [DefaultValue(typeof(AdaptiveActionMode), "Primary")]
        public AdaptiveActionMode Mode { get; set; } 

        [Parameter]
        public String Speak { get; set; } 

        [Parameter]
        [DefaultValue(typeof(AdaptiveActionStyle), "Default")]
        public AdaptiveActionStyle Style { get; set; } 

        [Parameter]
        public String Title { get => Item.Title; set => Item.Titlte = value; } 

        [Parameter]
        public String Tooltip { get; set; } 
    }
}
