// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.


using AdaptiveCards;
using Microsoft.AspNetCore.Components;

namespace Crazor.Blazor.Components.AdaptiveCards
{

    /// <summary>
    /// Component for TableColumn
    /// </summary>

    public class TableColumn : ItemComponent<IList<AdaptiveTableColumn>, AdaptiveTableColumn>
    {

        [Parameter]
        public String Width { get => Item.Width.ToString(); set => Item.Width = value; } 
    }
}
