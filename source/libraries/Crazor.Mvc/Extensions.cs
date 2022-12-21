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
        public static IServiceCollection AddCrazor(this IServiceCollection services)
        {
            services.AddCrazorCore();

            // add CardViews 
            foreach (var cardViewType in CardView.GetCardViewTypes())
            {
                services.AddTransient(cardViewType);
            }

            services.AddScoped<ICardViewFactory, CardViewFactory>();
            services.AddScoped<IRouteResolver, MvcRouteResolver>();
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
