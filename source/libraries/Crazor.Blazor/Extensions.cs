// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Crazor.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Crazor.Blazor
{
    public static class Extensions
    {
        public static IServiceCollection AddCrazor(this IServiceCollection services)
        {
            services.AddCrazorCore();

            // add CardViews 
            var cardViewTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(asm =>
                            asm.DefinedTypes
                                .Where(t => t.IsAbstract == false && t.ImplementedInterfaces.Contains(typeof(ICardView)))).ToList();
            foreach (var cardViewType in cardViewTypes)
            {
                services.AddTransient(cardViewType);
            }

            services.AddScoped<CardViewFactory>((sp) =>
            {
                var factory = new CardViewFactory(sp);
                foreach (var cardViewType in cardViewTypes)
                {
                    factory.Add(cardViewType.FullName, cardViewType);
                }

                return factory;
            });

            // add RouteManager
            services.AddScoped<RouteManager>((sp) =>
            {
                RouteManager routeManager = new RouteManager();
                foreach (var cardViewType in cardViewTypes)
                {
                    routeManager.Add(cardViewType);
                }
                return routeManager;
            });

            return services;
        }
    }
}