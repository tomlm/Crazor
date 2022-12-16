// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using AdaptiveCards;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;

namespace Crazor.Blazor.Components.AdaptiveCards
{

    /// <summary>
    /// Component for Card
    /// </summary>

    public class Card : TypedElementComponent<AdaptiveCard>
    {
        [Parameter]
        public String FallbackText { get => Item.FallbackText; set => Item.FallbackText = value; }

        [Parameter]
        public String Lang { get => Item.Lang; set => Item.Lang = value; }

        [Parameter]
        public String MinHeight { get => Item.MinHeight; set => Item.MinHeight = value; }

        [Parameter]
        public Boolean? Rtl { get => Item.Rtl; set => Item.Rtl = value ?? false; }

        [Parameter]
        public String Speak { get => Item.Speak; set => Item.Speak = value; }

        [Parameter]
        [DefaultValue(typeof(AdaptiveContainerStyle), "Default")]
        public AdaptiveContainerStyle Style { get => Item.Style; set => Item.Style = value; }

        [Parameter]
        public String Title { get => Item.Title; set => Item.Title = value; }

        [Parameter]
        [DefaultValue(typeof(AdaptiveVerticalContentAlignment), "Top")]
        public AdaptiveVerticalContentAlignment VerticalContentAlignment { get => Item.VerticalContentAlignment; set => Item.VerticalContentAlignment = value; }

        [Parameter]
        public String Height { get => Item.Height.ToString(); set => Item.Height = value; }

        [Parameter]
        public String Version { get => Item.Version.ToString(); set => Item.Version = new AdaptiveSchemaVersion(value); }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);

            if (Parent is AdaptiveShowCardAction action)
            {
                action.Card = Item;
            }
        }
    }
}
