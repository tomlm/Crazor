// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Crazor.Interfaces
{
    public interface IRouteResolver
    {
        void AddCardViewType(Type type);

        bool IsRouteValid(CardRoute route);

        bool ResolveRoute(CardRoute route, out Type? cardViewType);
    }
}
