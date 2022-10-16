using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Crazor.Encryption;
using Crazor.Interfaces;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Neleus.DependencyInjection.Extensions;
using OpBot;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Crazor
{
    public static class Extensions
    {
        private static readonly JsonSerializerSettings _cloneSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };

        /// <summary>
        /// DeepClone an object using JSON Serialization
        /// </summary>
        /// <typeparam name="T">Type to clone.</typeparam>
        /// <param name="obj">The object.</param>
        /// <returns>The object as Json.</returns>

        public static object DeepClone(this object obj)
        {
            // use serialization to deep clone
            return JsonConvert.DeserializeObject(JsonConvert.SerializeObject(obj, _cloneSettings), _cloneSettings);
        }

        public static IServiceCollection AddCardApps(this IServiceCollection services)
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
