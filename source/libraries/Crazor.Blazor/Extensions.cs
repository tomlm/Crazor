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

            services.AddScoped<ICardViewFactory>((sp) =>
            {
                var factory = new CardViewFactory(sp);
                foreach (var cardViewType in cardViewTypes)
                {
                    factory.Add(cardViewType.FullName, cardViewType);
                }

                return factory;
            });

            // add RouteManager
            services.AddScoped<IRouteResolver>((sp) =>
            {
                ReouteResolver routeResolver = new ReouteResolver();
                foreach (var cardViewType in cardViewTypes)
                {
                    routeResolver.AddCardViewType(cardViewType);
                }
                return routeResolver;
            });

            return services;
        }
    }
}