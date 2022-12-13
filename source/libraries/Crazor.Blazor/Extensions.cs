// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Crazor.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
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
                                .Where(t => t.IsAbstract == false && t.ImplementedInterfaces.Contains(typeof(ICardView)))
                                .Where(t => (t.Name != "CardView" && t.Name != "CardView`1" && t.Name != "CardView`2" && t.Name != "CardViewBase`1" && t.Name != "EmptyCardView"))).ToList();
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
                RouteResolver routeResolver = new RouteResolver();
                foreach (var cardViewType in cardViewTypes)
                {
                    routeResolver.AddCardViewType(cardViewType);
                }
                return routeResolver;
            });

            return services;
        }

        public static IApplicationBuilder UseCrazor(this IApplicationBuilder builder)
        {
            var fileProvider = new EmbeddedFileProvider2(typeof(CardView).Assembly);
            builder.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = fileProvider,
                RequestPath = new PathString("")
            });
            return builder;
        }

    }
}