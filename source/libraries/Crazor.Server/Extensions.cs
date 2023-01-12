// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Crazor.Interfaces;
using Crazor.Teams;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using System.Reflection;

namespace Crazor.Server
{
    public static class Extensions
    {
        /// <summary>
        /// AddCrazorServer - Add dependencies for CrazorServer, with optional manifest definition
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options">manifest factory</param>
        /// <returns></returns>
        public static IServiceCollection AddCrazorServer(this IServiceCollection services, Action<IConfiguration, Manifest> options = null)
        {
            services.AddHttpClient();
            services.AddHttpContextAccessor();
            services.TryAddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();
            services.TryAddScoped<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();
            services.TryAddScoped<IBot, CardActivityHandler>();
            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.TryAddSingleton<Manifest>((sp) =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var manifest = new Manifest(configuration, sp.GetRequiredService<IRouteResolver>());
                if (options != null)
                {
                    options(configuration, manifest);
                }
                return manifest;
            });

            HttpHelper.BotMessageSerializerSettings.Formatting = Formatting.None;
            HttpHelper.BotMessageSerializer.Formatting = Formatting.None;
            return services;
        }

        public static IApplicationBuilder UseCrazorServer(this IApplicationBuilder builder)
        {
            // mount any assembly with wwwroot.
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()
                    .Where(asm => asm.GetManifestResourceNames().Any(name => name.Split('.').Contains("wwwroot"))))
            {
                var fileProvider = new EmbeddedFileProvider2(assembly);
                builder.UseStaticFiles(new StaticFileOptions()
                {
                    FileProvider = fileProvider,
                    RequestPath = new PathString("")
                });
            }

            return builder;
        }
    }
}
