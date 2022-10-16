using Microsoft.Bot.Builder.Azure.Blobs;
using Microsoft.Bot.Builder;
using Crazor;

var builder = WebApplication.CreateBuilder(args);

// ---- <CRAZOR>
// add storage
var storageKey = builder.Configuration.GetValue<string>("AzureStorage");
if (!String.IsNullOrEmpty(storageKey))
{
    builder.Services.AddSingleton<IStorage, BlobsStorage>(sp => new BlobsStorage(storageKey, "opbot"));
}
builder.Services.AddCrazor();
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


app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.MapControllers();
app.Run();
