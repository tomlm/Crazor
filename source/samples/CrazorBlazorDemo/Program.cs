using Crazor;
using Crazor.Blazor;
using Crazor.Server;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure.Blobs;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

namespace CrazorBlazorDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddServerSideBlazor()
                .AddMicrosoftIdentityConsentHandler();

            builder.Services.AddControllers();
            builder.Services.AddControllersWithViews();

            // Add services to the container.
            var initialScopes = builder.Configuration["DownstreamApi:Scopes"]?.Split(' ') ?? builder.Configuration["MicrosoftGraph:Scopes"]?.Split(' ');
            builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
                    .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
                        .AddMicrosoftGraph(builder.Configuration.GetSection("MicrosoftGraph"))
                    .AddInMemoryTokenCaches();
            builder.Services.AddAuthorization(options =>
            {
                options.FallbackPolicy = options.DefaultPolicy;
            });

            builder.Services.AddRazorPages()
                .AddMicrosoftIdentityUI();

            // ---- <CRAZOR>
            builder.Services.AddCrazor("SharedCards");
            builder.Services.AddCrazorServer((options) =>
            {
                options.Manifest.Version = "1.5.3";
                options.Manifest.Developer.Name = "Tom Laird-McConnell";
                options.Manifest.Description.Full = "This is a demo of using Blazor templates for crazor apps.";
            });
            builder.Services.AddCrazorBlazor();
            // register blob storage for state management
            var storageKey = builder.Configuration.GetValue<string>("AzureStorage");
            if (!String.IsNullOrEmpty(storageKey))
            {
                builder.Services.AddSingleton<IStorage, BlobsStorage>(sp => new BlobsStorage(storageKey, nameof(CrazorBlazorDemo).ToLower()));
            }
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
            app.UseCrazorServer();
            app.UseCrazorBlazor();
            // </CRAZOR>

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");


            app.Run();
        }
    }
}