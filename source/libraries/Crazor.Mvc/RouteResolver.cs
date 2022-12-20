// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

namespace Crazor.Mvc
{
    public class MvcRouteResolver : RouteResolver
    {
        private Dictionary<string, List<RouteTemplate>> _routes = new Dictionary<string, List<RouteTemplate>>(StringComparer.OrdinalIgnoreCase);

        public MvcRouteResolver()
        {
            foreach (var cardViewType in CardView.GetCardViewTypes())
            {
                this.AddCardViewType(cardViewType);
            }
        }
    }
}
