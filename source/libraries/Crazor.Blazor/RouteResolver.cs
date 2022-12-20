// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

namespace Crazor.Blazor
{
    public class BlazorRouteResolver : RouteResolver
    {
        private Dictionary<string, List<RouteTemplate>> _routes = new Dictionary<string, List<RouteTemplate>>(StringComparer.OrdinalIgnoreCase);

        public BlazorRouteResolver()
        {
            foreach (var cardViewType in CardView.GetCardViewTypes())
            {
                this.AddCardViewType(cardViewType);
            }
        }
    }
}