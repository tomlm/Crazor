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
        public static IServiceCollection AddCrazorBlazor(this IServiceCollection services)
        {
            // add CardViews 
            foreach (var cardViewType in CardView.GetCardViewTypes())
            {
                services.AddTransient(cardViewType);
            }

            services.AddScoped<ICardViewFactory, CardViewFactory>();
            services.AddScoped<IRouteResolver, BlazorRouteResolver>();
            return services;
        }

        public static IApplicationBuilder UseCrazorBlazor(this IApplicationBuilder builder)
        {
            return builder;
        }
    }
}