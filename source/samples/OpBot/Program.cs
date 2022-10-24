using Microsoft.Bot.Builder;
using Crazor;
using System.Diagnostics;
using Microsoft.Bot.Builder.Azure.Blobs;

var builder = WebApplication.CreateBuilder(args);

// ---- <CRAZOR>
// register blob storage for state management
var storageKey = builder.Configuration.GetValue<string>("AzureStorage");
if (!String.IsNullOrEmpty(storageKey))
{
    builder.Services.AddSingleton<IStorage, BlobsStorage>(sp => new BlobsStorage(storageKey, "opbot"));
}
builder.Services.AddCrazor();
// ---- </CRAZOR>

var mvcBuilder = builder.Services.AddMvc()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull | System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault;
    }); 

// uncomment this to enable live edit of markup pages
//if (builder.Environment.IsDevelopment())
//{
//    mvcBuilder.AddRazorRuntimeCompilation();
//}

builder.Services.AddControllers();

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
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.MapControllers();
//app.UseMvc();
app.Run();
