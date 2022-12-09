// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Crazor.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;

namespace Crazor.Mvc
{
    public static class Extensions
    {
        public static IServiceCollection AddCrazor(this IServiceCollection services)
        {
            services.AddCrazorCore();

            // add CardViews 
            var cardViewTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(asm =>
                            asm.DefinedTypes
                                .Where(t => t.IsAbstract == false && t.ImplementedInterfaces.Contains(typeof(ICardView)))
                                .Where(t => (t.Name != "CardView" && t.Name != "CardView`1" && t.Name != "CardView`2" && t.Name != "CardViewBase`1" && t.Name != "EmptyCardView"))).ToList();
            foreach (var cardViewType in cardViewTypes)
            {
                services.AddTransient(cardViewType);
            }

            services.AddScoped<ICardViewFactory>((sp) =>
            {
                var factory = new CardViewFactory(sp, 
                    sp.GetRequiredService<IRazorViewEngine>(), 
                    sp.GetRequiredService<IHttpContextAccessor>(), 
                    sp.GetRequiredService<ITempDataProvider>());
                foreach (var cardViewType in cardViewTypes)
                {
                    factory.Add(cardViewType.FullName, cardViewType);
                }

                return factory;
            });

            // add RouteResolver
            services.AddScoped<IRouteResolver>((sp) =>
            {
                RouteResolver routeManager = new RouteResolver();
                foreach (var cardViewType in cardViewTypes)
                {
                    routeManager.AddCardViewType(cardViewType);
                }
                return routeManager;
            });

            return services;
        }

    }

}
