// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Crazor.Blazor;
using CrazorBlazorDemo.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
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
            // register blob storage for state management
            var storageKey = builder.Configuration.GetValue<string>("AzureStorage");
            if (!String.IsNullOrEmpty(storageKey))
            {
                builder.Services.AddSingleton<IStorage, BlobsStorage>(sp => new BlobsStorage(storageKey, "opbot"));
            }
            builder.Services.AddCrazor();
            // ---- </CRAZOR>

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");

            app.UseCrazor();
            app.MapControllers();

            app.Run();
        }
    }
}