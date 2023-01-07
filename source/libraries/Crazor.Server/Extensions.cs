// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Crazor.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using System.Reflection;

namespace Crazor.Server
{
    public static class Extensions
    {
        public static IServiceCollection AddCrazorServer(this IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddHttpContextAccessor();
            services.TryAddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();
            services.TryAddScoped<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();
            services.TryAddScoped<IBot, CardActivityHandler>();
            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

            HttpHelper.BotMessageSerializerSettings.Formatting = Formatting.None;
            HttpHelper.BotMessageSerializer.Formatting = Formatting.None;
            return services;
        }

        public static IApplicationBuilder UseCrazorServer<CardViewT>(this IApplicationBuilder builder)
            where CardViewT : ICardView
        {
            var fileProvider = new EmbeddedFileProvider2(typeof(CardViewT).Assembly);
            builder.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = fileProvider,
                RequestPath = new PathString("")
            });

            return builder;
        }
    }
}
