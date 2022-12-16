// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.


using AdaptiveCards;

namespace Crazor.Blazor.Components.AdaptiveCards
{

    /// <summary>
    /// Component for Fallback
    /// </summary>
    public class Fallback : ItemComponent<AdaptiveFallbackElement>
    {
        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);

            if (this.Parent is AdaptiveElement element)
            {
                element.Fallback = Item;
            }
        }
    }
}
