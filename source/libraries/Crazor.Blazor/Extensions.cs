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
        /// <summary>
        /// Add Crazor Dependencies.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddCrazor(this IServiceCollection services)
        {
            services.AddCrazorCore();

            // add CardViews 
            foreach (var cardViewType in CardView.GetCardViewTypes())
            {
                services.AddTransient(cardViewType);
            }

            services.AddScoped<ICardViewFactory, CardViewFactory>();
            services.AddScoped<IRouteResolver, RouteResolver>();
            return services;
        }

        /// <summary>
        /// Add support for Crazor embedded content.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
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