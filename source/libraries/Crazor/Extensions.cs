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

namespace Crazor
{
    public static class Extensions
    {

        public static IServiceCollection AddCrazor(this IServiceCollection services)
        {
            services.AddHttpClient();
            services.TryAddSingleton<IStorage, MemoryStorage>();
            services.TryAddSingleton<IEncryptionProvider, NoEncryptionProvider>();
            services.TryAddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();
            services.TryAddScoped<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();
            services.TryAddScoped<IBot, CardActivityHandler>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddSingleton<IUrlHelper, UrlHelperProxy>();

            // add Apps
            var cardAppServices = services.AddByName<CardApp>();
            foreach (var cardAppType in Assembly.GetCallingAssembly().DefinedTypes.Where(t => t.IsAssignableTo(typeof(CardApp))))
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
