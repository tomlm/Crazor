using Microsoft.Bot.Builder.Azure.Blobs;
using Microsoft.Bot.Builder;
using Crazor;
using Crazor.Server;
using Crazor.Mvc;
using Crazor.Blazor;

var builder = WebApplication.CreateBuilder(args);

// ---- <CRAZOR>
builder.Services.AddCrazor();
builder.Services.AddCrazorMvc();
builder.Services.AddCrazorBlazor();
builder.Services.AddCrazorServer((options) =>
{
    options.Manifest.Version = "1.0";
    options.Manifest.Name.Short = "CrazorDemoBot";
    options.Manifest.Name.Full = "This is a demo of using MVC templates for crazor apps.";
    options.Manifest.Developer.Name = "Tom Laird-McConnell";
    options.Manifest.Description.Short = "CrazorDemoBot";
    options.Manifest.Description.Full = "This is a demo of using MVC templates for crazor apps.";
});

var storageKey = builder.Configuration.GetValue<string>("AzureStorage");
if (!String.IsNullOrEmpty(storageKey))
{
    builder.Services.AddSingleton<IStorage, BlobsStorage>(sp => new BlobsStorage(storageKey, nameof(SampleWebApp)));
}
// ---- </CRAZOR>

var mvcBuilder = builder.Services.AddMvc();
if (builder.Environment.IsDevelopment())
{
    mvcBuilder.AddRazorRuntimeCompilation();
}

builder.Services.AddControllers();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


// ---- <CRAZOR>
app.UseCrazorServer();
// </CRAZOR>
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.MapControllers();
app.Run();
