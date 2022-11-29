// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Diag = System.Diagnostics;

namespace Crazor.Test
{
    public class CardTest
    {
        public static IServiceProvider Services { get; set; }

        public static CardAppFactory Factory => Services.GetRequiredService<CardAppFactory>();

        public static void InitCardServices(Action<IServiceCollection> callback = null)
        {
            Environment.CurrentDirectory = Path.Combine(Environment.CurrentDirectory, "..", "..", "..");
            var builder = WebApplication.CreateBuilder();
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string>()
            {
                { "HostUri", "http://localhost" },
                { "BotId", "00000000-0000-0000-0000-000000000000" }
            });
            builder.Services.AddSingleton<IRazorViewEngine, RazorViewEngine>();
            builder.Services.AddSingleton<IStorage, MemoryStorage>();
            builder.Services.AddCrazor();
            builder.Services.AddMvc()
                // .AddRazorOptions((options) => { var x = options; })
                // .AddRazorPagesOptions((options) => { var y = options; })
                .AddRazorRuntimeCompilation();
            var listener = new Diag.DiagnosticListener("Microsoft.AspNetCore");
            builder.Services.AddSingleton<Diag.DiagnosticListener>(listener);
            builder.Services.AddSingleton<Diag.DiagnosticSource>(listener);
            if (callback != null)
            {
                callback(builder.Services);
            }
            Services = builder.Services.BuildServiceProvider();
        }

        public static Activity CreateActivity(string channelId = "test")
        {
            return new Activity()
            {
                Type = ActivityTypes.Invoke,
                ChannelId = channelId,
                Id = "test",
                From = new ChannelAccount(id: "test", name: "Test"),
                Recipient = new ChannelAccount(id: "test", name: "Test"),
                Conversation = new ConversationAccount(id: "test"),
                ServiceUrl = "http://localhost/api/messsages",
            };
        }

        /// <summary>
        /// Load card by route
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        public async Task<CardTestContext> LoadCard(string route, string channelId = "test", bool isPreview = false)
        {
            var cardApp = Services.GetRequiredService<CardAppFactory>().Create(CardRoute.Parse(route));

            var card = await cardApp.ProcessInvokeActivity(CreateActivity().CreateLoadRouteActivity(route), isPreview, default(CancellationToken));
            return new CardTestContext() { Card = card, Services = Services };
        }
    }
}