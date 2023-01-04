// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Crazor.Blazor.Tests.Cards.RouteBinding2
{
    public class RouteBinding2App: CardApp
    {
        public RouteBinding2App(CardAppContext context) : base(context)
        {
        }

        public string? Id { get; set; }
    }
}