// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.


using AdaptiveCards;
using Microsoft.AspNetCore.Components;

namespace Crazor.Blazor.Components.AdaptiveCards
{

    /// <summary>
    /// Component for Refresh
    /// </summary>

    public class Refresh : ItemComponent<AdaptiveRefresh>
    {
        /// <summary>
        ///    The action to be executed to refresh the card.
        ///    Clients can run this refresh action automatically or can provide an affordance for users to trigger it manually.
        /// </summary>
        public RenderFragment Action { get; set; }

        /// <summary>
        ///     A list of user Ids informing the client for which users should the refresh action should be run automatically.
        ///     Some clients will not run the refresh action automatically unless this property is specified.
        ///     Some clients may ignore this property and always run the refresh action automatically.
        /// </summary>
        public RenderFragment UserIds { get; set; }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);

            if (this.Parent is AdaptiveCard card)
            {
                card.Refresh = Item;
            }
        }
    }
}
