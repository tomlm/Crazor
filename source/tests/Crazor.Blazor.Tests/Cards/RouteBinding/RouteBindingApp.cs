// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Crazor.Blazor.Tests.Cards.RouteBinding
{
    public class RouteBindingApp : CardApp
    {
        public RouteBindingApp(CardAppContext context) : base(context)
        {
        }

        public string? Id { get; set; }
    }
}