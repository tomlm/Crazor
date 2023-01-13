// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Crazor;
using Crazor.Mvc;
using Crazor.Server;
using CrazorDemoBot.Cards.CodeOnlyView;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure.Blobs;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// ---- <CRAZOR>
// register blob storage for state management
var storageKey = builder.Configuration.GetValue<string>("AzureStorage");
if (!String.IsNullOrEmpty(storageKey))
{
    builder.Services.AddSingleton<IStorage, BlobsStorage>(sp => new BlobsStorage(storageKey, nameof(CrazorDemoBot).ToLower()));
}
builder.Services.AddCrazor();
builder.Services.AddCrazorMvc();
 builder.Services.AddCrazorServer((options) =>
{
    options.Manifest.Version = "1.0";
    options.Manifest.Name.Short = "CrazorDemoBot";
    options.Manifest.Name.Full = "This is a demo of using MVC templates for crazor apps.";
    options.Manifest.Developer.Name = "Tom Laird-McConnell";
    options.Manifest.Description.Short = "CrazorDemoBot";
    options.Manifest.Description.Full = "This is a demo of using MVC templates for crazor apps.";
});


builder.Services.AddCardView<MyCodeView>();
// ---- </CRAZOR>

var mvcBuilder = builder.Services.AddMvc();

if (Debugger.IsAttached)
{
    mvcBuilder.AddRazorRuntimeCompilation();
}

builder.Services.AddLogging();
builder.Services.AddControllers();
//builder.Services.AddControllersWithViews(); 
//builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (!Debugger.IsAttached)
{
    app.UseHttpsRedirection();
}
app.UseCrazorServer();

app.UseRouting();
app.UseAuthorization();
app.UseStaticFiles();
app.MapRazorPages();
app.MapControllers();
//app.UseMvc();
app.Run();
