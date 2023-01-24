// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Crazor.Blazor;
using Crazor.Server;
using Crazor;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure.Blobs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// ---- <CRAZOR>
builder.Services.AddControllers();

// register blob storage for state management
var storageKey = builder.Configuration.GetValue<string>("AzureStorage");
if (!String.IsNullOrEmpty(storageKey))
{
    builder.Services.AddSingleton<IStorage, BlobsStorage>(sp => new BlobsStorage(storageKey, nameof(CrazorBlazorClientDemo).ToLower()));
}
builder.Services.AddCrazor(options =>
{
    options.LoadCardAssembly("SharedCards");
    options.Logger.Request = (activiy) => Console.WriteLine($"REQUEST: {DateTime.Now} - {activiy.Id}\n\nActivity = {activiy.ToJson()}");
    options.Logger.Response = (activiy, card) => Console.WriteLine($"RESPONSE: {DateTime.Now} - {activiy.Id}\n\nCard = {card.ToJson()}");
});
builder.Services.AddCrazorServer();
builder.Services.AddCrazorBlazor();
// ---- </CRAZOR>

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// ---- <CRAZOR>
app.UseCrazorServer();
app.MapControllers();
// </CRAZOR>

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
