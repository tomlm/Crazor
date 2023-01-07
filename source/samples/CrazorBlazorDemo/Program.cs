// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Crazor;
using Crazor.Blazor;
using Crazor.Server;
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

            // ---- <CRAZOR>
            builder.Services.AddControllers();

            // register blob storage for state management
            var storageKey = builder.Configuration.GetValue<string>("AzureStorage");
            if (!String.IsNullOrEmpty(storageKey))
            {
                builder.Services.AddSingleton<IStorage, BlobsStorage>(sp => new BlobsStorage(storageKey, nameof(CrazorBlazorDemo).ToLower()));
            }
            builder.Services.AddCrazor();
            builder.Services.AddCrazorServer();
            builder.Services.AddCrazorBlazor();
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
            app.UseCrazor();
            app.UseCrazorBlazor();
            app.UseCrazorServer<CardView>();

            app.UseRouting();
            app.MapControllers();
            // </CRAZOR>

            app.UseStaticFiles();
            app.UseRouting();
            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");


            app.Run();
        }
    }
}