// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Crazor;
using Crazor.Blazor;
using Crazor.Server;
using CrazorBlazorDemo.Cards.CodeOnlyView;
using CrazorBlazorDemo.Data;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure.Blobs;

namespace CrazorBlazorDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();
            builder.Services.AddSingleton<WeatherForecastService>();
            builder.Services.AddControllers();

            // ---- <CRAZOR>
            builder.Services.AddCrazor();
            builder.Services.AddCrazorServer((options) =>
            {
                options.Manifest.Version = "1.0";
                options.Manifest.Name.Short = "CrazorBlazorDemo";
                options.Manifest.Name.Full = "This is a demo of using Blazor templates for crazor apps.";
                options.Manifest.Developer.Name = "Tom Laird-McConnell";
                options.Manifest.Description.Short = "CrazorBlazorDemo";
                options.Manifest.Description.Full = "This is a demo of using Blazor templates for crazor apps.";
            });
            builder.Services.AddCrazorBlazor();
            builder.Services.AddCardView<MyCodeView>();

            // register blob storage for state management
            var storageKey = builder.Configuration.GetValue<string>("AzureStorage");
            if (!String.IsNullOrEmpty(storageKey))
            {
                builder.Services.AddSingleton<IStorage, BlobsStorage>(sp => new BlobsStorage(storageKey, nameof(CrazorBlazorDemo).ToLower()));
            }
            // ---- </CRAZOR>

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            if (!System.Diagnostics.Debugger.IsAttached)
            {
                app.UseHttpsRedirection();
            }

            // ---- <CRAZOR>
            app.UseCrazorServer();

            app.UseStaticFiles();
            app.UseRouting();
            app.MapControllers();
            // </CRAZOR>

            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");


            app.Run();
        }
    }
}