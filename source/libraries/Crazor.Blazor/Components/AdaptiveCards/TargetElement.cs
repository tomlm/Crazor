// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

using AdaptiveCards;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;

namespace Crazor.Blazor.Components.AdaptiveCards
{

    /// <summary>
    /// Component for TargetElement
    /// </summary>

    public class TargetElement : ItemComponent<IList<AdaptiveTargetElement>, AdaptiveTargetElement>
    {

        /// <summary>
        /// Target element Id.
        /// </summary>
        [Parameter]
        public string ElementId { get; set; }

        /// <summary>
        /// Target element visibility.
        /// </summary>
        [Parameter]
        [DefaultValue(null)]
        public bool? IsVisible { get; set; } = null;

    }
}
