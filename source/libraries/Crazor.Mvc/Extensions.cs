// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Crazor.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Crazor.Mvc
{
    public static class Extensions
    {
        public static IServiceCollection AddCrazorMvc(this IServiceCollection services)
        {
            // add CardViews 
            foreach (var cardViewType in CardView.GetCardViewTypes())
            {
                services.AddTransient(cardViewType);
            }

            services.AddScoped<ICardViewFactory, CardViewFactory>();
            services.AddScoped<IRouteResolver, MvcRouteResolver>();

            // add card home pages support
            var mvcBuilder = services.AddRazorPages()
                 .AddRazorOptions(options =>
                 {
                     options.ViewLocationFormats.Add("/Cards/{0}.cshtml");
                 });

            return services;
        }

        public static IApplicationBuilder UseCrazorMvc(this IApplicationBuilder builder)
        {
            return builder;
        }
    }
}
