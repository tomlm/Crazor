// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Crazor.Blazor;
using Crazor;

using CrazorBlazorClientDemo.Wasm;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Bot.Builder;
using CrazorBlazorClientDemo.Wasm.Helpers;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// ---- <CRAZOR>

// register AppSettings.json file from wwwroot folder
var http = new HttpClient()
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
};

builder.Services.AddScoped(sp => http);

using var response = await http.GetAsync("appsettings.json");
using var stream = await response.Content.ReadAsStreamAsync();

builder.Configuration.AddJsonStream(stream);

// register blob storage for state management
builder.Services.AddCrazor("SharedCards");
builder.Services.AddCrazorBlazor();

// If you're using the StateController.cs file, to save and load state.
// builder.Services.AddScoped<IStorage, RemoteStorage>();

// ---- </CRAZOR>

await builder.Build().RunAsync();
