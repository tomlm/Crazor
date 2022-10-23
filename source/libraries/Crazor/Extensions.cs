using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Crazor.Encryption;
using Crazor.Interfaces;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Neleus.DependencyInjection.Extensions;
using OpBot;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Crazor
{
    public static class Extensions
    {

        public static IServiceCollection AddCrazor(this IServiceCollection services)
        {
            services.AddHttpClient();
            services.TryAddSingleton<IStorage>((sp) =>
            {
                Trace.TraceWarning(@"There is no IStorage provider registered for Crazor cards to use.");
                Trace.TraceWarning("The MemoryStorage provider is being used which is only suitable for local development becuase it is not durable.");
                Trace.TraceWarning("Add an IStorage provider via dependency injection in your program.cs.  For example to register Azure BlobStorage as your provider:");
                Trace.TraceWarning("    var storageKey = builder.Configuration.GetValue<string>(\"AzureStorage\");");
                Trace.TraceWarning("    if (!String.IsNullOrEmpty(storageKey))");
                Trace.TraceWarning("    {");
                Trace.TraceWarning("        builder.Services.AddSingleton<IStorage, BlobsStorage>(sp => new BlobsStorage(storageKey, \"opbot\"));");
                Trace.TraceWarning("    }");
                return new MemoryStorage();
            });
            services.TryAddSingleton<IEncryptionProvider, NoEncryptionProvider>();
            services.TryAddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();
            services.TryAddScoped<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();
            services.TryAddScoped<IBot, CardActivityHandler>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.TryAddScoped<IUrlHelper, UrlHelperProxy>();

            // add Apps
            var cardAppServices = services.AddByName<CardApp>();
            foreach (var cardAppType in Assembly.GetCallingAssembly().DefinedTypes.Where(t => t.IsAssignableTo(typeof(CardApp)) && t.IsAbstract == false))
            {
                services.AddScoped(cardAppType);
                cardAppServices.Add(cardAppType.Name, cardAppType);
            }
            cardAppServices.Build();

            // add card Razor pages support
            var mvcBuilder = services.AddRazorPages()
                 .AddRazorOptions(options =>
                 {
                     options.ViewLocationFormats.Add("/Cards/{0}.cshtml");
                 });
           
            return services;
        }

    }
}
