// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Crazor;
using Crazor.Mvc;
using Crazor.Server;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure.Blobs;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var initialScopes = builder.Configuration["DownstreamApi:Scopes"]?.Split(' ') ?? builder.Configuration["MicrosoftGraph:Scopes"]?.Split(' ');

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
        .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
            .AddMicrosoftGraph(builder.Configuration.GetSection("MicrosoftGraph"))
            .AddInMemoryTokenCaches();

builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy.
    options.FallbackPolicy = options.DefaultPolicy;
});

builder.Services.AddRazorPages()
    .AddMicrosoftIdentityUI();

// ---- <CRAZOR>
// register blob storage for state management
var storageKey = builder.Configuration.GetValue<string>("AzureStorage");
if (!String.IsNullOrEmpty(storageKey))
{
    builder.Services.AddSingleton<IStorage, BlobsStorage>(sp => new BlobsStorage(storageKey, nameof(CrazorDemoBot).ToLower()));
}
builder.Services.AddCrazor();
builder.Services.AddCrazorMvc();
builder.Services.AddCrazorServer((serverOptions) =>
{
    serverOptions.Manifest.Version = "1.2";
    serverOptions.Manifest.Description.Full = "This is a demo of using MVC templates for crazor apps.";
});


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

app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.MapRazorPages();
app.MapControllers();
//app.UseMvc();
app.Run();
