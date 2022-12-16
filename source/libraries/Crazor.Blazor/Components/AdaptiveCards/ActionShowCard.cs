// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using AdaptiveCards;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;

namespace Crazor.Blazor.Components.AdaptiveCards
{

    /// <summary>
    /// Component for Action.ShowCard
    /// </summary>
    public class ActionShowCard : ActionComponent<AdaptiveShowCardAction>
    {

        [Parameter]
        public String IconUrl { get => Item.IconUrl; set => Item.IconUrl = value; }

        [Parameter]
        public Boolean? IsEnabled { get => Item.IsEnabled; set => Item.IsEnabled = value ?? true; }

        [Parameter]
        [DefaultValue(typeof(AdaptiveActionMode), "Primary")]
        public AdaptiveActionMode Mode { get => Item.Mode; set => Item.Mode = value; }

        [Parameter]
        [DefaultValue(typeof(AdaptiveActionStyle), "Default")]
        public AdaptiveActionStyle Style { get => Item.Style; set => Item.Style = value; }

        [Parameter]
        public String Title { get => Item.Title; set => Item.Title = value; }

        [Parameter]
        public String Tooltip { get => Item.Tooltip; set => Item.Tooltip = value; }
    }
}
