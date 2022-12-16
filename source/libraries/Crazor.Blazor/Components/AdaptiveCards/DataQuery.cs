// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.


using AdaptiveCards;
using Microsoft.AspNetCore.Components;

namespace Crazor.Blazor.Components.AdaptiveCards
{

    /// <summary>
    /// Component for Data.Query
    /// </summary>

    public class DataQuery : ItemComponent<AdaptiveChoiceSetInput, AdaptiveDataQuery>
    {
        [Parameter]
        public Int32? Count { get => Item.Count; set => Item.Count = value ?? 10; }

        [Parameter]
        public String Dataset { get => Item.Dataset; set => Item.Dataset = value; } 

        [Parameter]
        public Int32? Skip { get => Item.Skip; set => Item.Skip = value ?? 0; } 

        [Parameter]
        public String Value { get => Item.Value; set => Item.Value = value; }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            
            this.Parent.DataQuery = Item;
        }
    }
}
