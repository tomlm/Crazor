// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Crazor.Blazor;
using Crazor.Server;
using Crazor;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure.Blobs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// ---- <CRAZOR>
builder.Services.AddControllers();

// register blob storage for state management
var storageKey = builder.Configuration.GetValue<string>("AzureStorage");
if (!String.IsNullOrEmpty(storageKey))
{
    builder.Services.AddSingleton<IStorage, BlobsStorage>(sp => new BlobsStorage(storageKey, nameof(CrazorBlazorClientDemo).ToLower()));
}
builder.Services.AddCrazor("CrazorBlazorClientDemo.Shared");
builder.Services.AddCrazorServer();
builder.Services.AddCrazorBlazor();
// ---- </CRAZOR>

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

// ---- <CRAZOR>
app.UseCrazorServer<CardView>();
// </CRAZOR>

app.UseRouting();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
