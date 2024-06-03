using AdaptiveCards;
using Azure.Core;
using Crazor.Interfaces;
using Crazor.Teams;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Threading;
using Microsoft.AspNetCore.Components.Authorization;
using Crazor.Server.Controllers;

namespace Crazor.Server
{
    public static class Extensions
    {
        /// <summary>
        /// AddCrazorServer - Add dependencies for CrazorServer, with optional manifest definition
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configureOptions">manifest factory</param>
        /// <returns></returns>
        public static IServiceCollection AddCrazorServer(this IServiceCollection services, Action<CrazorServerOptions> configureOptions = null)
        {
            services.AddHttpClient();
            services.AddHttpContextAccessor();
            services.AddScoped<CardAppController>();
            services.AddSingleton<ManifestController>();

            //services.AddScoped<IAuthorizationHeaderProvider, CrazorAuthorizationHeaderProvider>();
            //services.AddScoped<TokenCredential, CrazorAuthorizationHeaderProvider>();
            services.TryAddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();
            //services.AddScoped<UserTokenClient>((sp) =>
            //{
            //    var config = sp.GetRequiredService<IConfiguration>();
            //    var botAppId = config.GetValue<string>("MicrosoftAppId");
            //    var claimsIdentity = new ClaimsIdentity(new List<Claim>
            //    {
            //        // Adding claimse for both Emulator and Channel.
            //        new Claim(AuthenticationConstants.AudienceClaim, botAppId),
            //        new Claim(AuthenticationConstants.AppIdClaim, botAppId),
            //        // new Claim(AuthenticationConstants.ServiceUrlClaim, serviceUrl)
            //    }); 

            //    var botAuth = sp.GetRequiredService<BotFrameworkAuthentication>();
            //    var userTokenClient = botAuth.CreateUserTokenClientAsync(claimsIdentity, CancellationToken.None).GetAwaiter().GetResult();
            //    return userTokenClient;
            //});
            services.TryAddScoped<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();
            services.TryAddScoped<IBot, CardActivityHandler>();
            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.TryAddSingleton<Manifest>();
            services.TryAddSingleton((sp) =>
            {
                var options = new CrazorServerOptions()
                {
                    Manifest = sp.GetRequiredService<Manifest>()
                };

                if (configureOptions != null)
                {
                    configureOptions(options);
                }
                return options;
            });

            services.AddScoped<AuthenticationStateProvider, DynamicAuthenticationStateProvider>();
            services.AddAuthorizationCore();

            HttpHelper.BotMessageSerializerSettings.Formatting = Formatting.None;
            HttpHelper.BotMessageSerializer.Formatting = Formatting.None;
            return services;
        }

        public static IApplicationBuilder UseCrazorServer(this IApplicationBuilder builder)
        {
            // mount any assembly with wwwroot.
            foreach (var assembly in Utils.GetAssemblies()
                    .Where(asm => asm.GetManifestResourceNames().Any(name => name.Split('.').Contains("wwwroot") || name.Split('.').Contains("Cards"))))
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

        //public static async Task<AdaptiveCard> ProcessInvokeActivitySilent(this CardApp cardApp, AdaptiveAuthentication? authentication, IInvokeActivity invokeActivity, bool isPreview, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        return await cardApp.ProcessInvokeActivity(invokeActivity, isPreview, cancellationToken);
        //    }
        //    catch (MicrosoftIdentityWebChallengeUserException)
        //    {
        //        return await cardApp.CreateAuthCard(authentication, cancellationToken);
        //    }
        //    catch (UnauthorizedAccessException)
        //    {
        //        return await cardApp.CreateAuthCard(authentication, cancellationToken);
        //    }
        //}
    }
}
