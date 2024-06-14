using Crazor.Blazor;
using Crazor.Server;
using Crazor;
using CrazorProject.Components;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure.Blobs;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web.UI;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers();
// builder.Services.AddControllersWithViews();

builder.Services.AddServerSideBlazor()
   .AddMicrosoftIdentityConsentHandler();

var initialScopes = builder.Configuration["DownstreamApi:Scopes"]?.Split(' ') ?? builder.Configuration["MicrosoftGraph:Scopes"]?.Split(' ');
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
        .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
            .AddMicrosoftGraph(builder.Configuration.GetSection("MicrosoftGraph"))
        .AddInMemoryTokenCaches();

builder.Services.AddAuthorization();

builder.Services.AddRazorPages()
    .AddMicrosoftIdentityUI();

// ---- <CRAZOR>
builder.Services.AddCrazor();
builder.Services.AddCrazorServer((options) =>
{
    options.Manifest.Version = "1.0.0";
    options.Manifest.Developer.Name = "Uknown";
    options.Manifest.Description.Full = "This is a demo of using Blazor templates for crazor apps.";
});
builder.Services.AddCrazorBlazor();

var storageKey = builder.Configuration.GetValue<string>("AzureStorage");
if (storageKey != null)
   builder.Services.AddSingleton<IStorage>(sp => new BlobsStorage(storageKey, containerName: "cards"));
// ---- </CRAZOR>

var app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();
// ---- <CRAZOR>
app.UseCrazorServer();
app.UseCrazorBlazor();
// </CRAZOR>
app.MapRazorPages();
app.UseAntiforgery();
app.MapControllers();
app.UseAuthorization();
app.MapRazorComponents<App>()
    .AddCrazorComponents()
    .AddInteractiveServerRenderMode();

app.Run();
