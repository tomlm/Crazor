// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Crazor.Blazor;
using Crazor;
using CrazorBlazorClientDemo.Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Bot.Builder.Azure.Blobs;
using Microsoft.Bot.Builder;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// ---- <CRAZOR>
// register blob storage for state management
var storageKey = builder.Configuration.GetValue<string>("AzureStorage");
if (!String.IsNullOrEmpty(storageKey))
{
    builder.Services.AddSingleton<IStorage, BlobsStorage>(sp => new BlobsStorage(storageKey, nameof(CrazorBlazorClientDemo).ToLower()));
}
builder.Services.AddCrazor("CrazorBlazorClientDemo.Shared");
builder.Services.AddCrazorBlazor();
// ---- </CRAZOR>


var app = builder.Build();
app.RunAsync();
