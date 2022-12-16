// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.


using AdaptiveCards;
using Microsoft.AspNetCore.Components;

namespace Crazor.Blazor.Components.AdaptiveCards
{

    /// <summary>
    /// Component for Fact
    /// </summary>

    public class Fact : ItemComponent<AdaptiveFact>
    {
        [Parameter]
        public String Title { get => Item.Title; set => Item.Title = value; }

        [Parameter]
        public String Value { get => Item.Value; set => Item.Value = value; }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);

            if (this.Parent is AdaptiveFactSet factSet)
            {
                factSet.Facts.Add(Item);
            }
        }
    }
}
