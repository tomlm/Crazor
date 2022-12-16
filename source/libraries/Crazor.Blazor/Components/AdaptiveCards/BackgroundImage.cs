// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using AdaptiveCards;
using Crazor.Attributes;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;
using System.Xml.Linq;

namespace Crazor.Blazor.Components.AdaptiveCards
{

    /// <summary>
    /// Component for BackgroundImage
    /// </summary>

    public class BackgroundImage : ItemComponent<AdaptiveTypedElement, AdaptiveBackgroundImage>
    {
        [Parameter]
        [DefaultValue(typeof(AdaptiveImageFillMode), "Cover")]
        public AdaptiveImageFillMode FillMode { get => Item.FillMode; set => Item.FillMode = value; }

        [Parameter]
        [DefaultValue(typeof(AdaptiveHorizontalAlignment), "Left")]
        public AdaptiveHorizontalAlignment HorizontalAlignment { get => Item.HorizontalAlignment; set => Item.HorizontalAlignment = value; }

        [Parameter]
        public String Url { get => Item.Url; set => Item.Url = value; }

        [Parameter]
        [DefaultValue(typeof(AdaptiveVerticalAlignment), "Top")]
        public AdaptiveVerticalAlignment VerticalAlignment { get => Item.VerticalAlignment; set => Item.VerticalAlignment = value; }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            ArgumentNullException.ThrowIfNull(Parent);
            if (Parent is AdaptiveCard card)
                card.BackgroundImage = this.Item;
            else if (Parent is AdaptiveContainer container)
                container.BackgroundImage = this.Item;
            else
                throw new Exception($"{Parent.GetType().Name} is not a valid parent for BackgroundImage");
        }
    }
}
