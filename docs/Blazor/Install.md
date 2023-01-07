

![image](https://user-images.githubusercontent.com/17789481/197238565-e3f895d0-6def-4d41-aba2-721d5432b1ef.png)

# Adding Crazor.Blazor 

Adding **Crazor.Blazor** is super easy.  Take a stock Blazor Server project and add the **Crazor.Blazor** package.

![image-20230106113527616](assets/image-20230106113527616.png)

## Add Crazor.Blazor package to Server

>  **NOTE: Currently Crazor.Blazor is only published to an internal Microsoft devops nuget feed.  To connect to this feed, add a nuget.config in the root of your project with the following:**

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="Crazor" value="https://fuselabs.pkgs.visualstudio.com/c861868a-1061-43d1-8232-ed9ab373867c/_packaging/Crazor/nuget/v3/index.json" />
  </packageSources>
</configuration>
```

Then you can add the **Crazor.Blazor** package

```shell
nuget add package Crazor
nuget add package Crazor.Blazor
nuget add package Crazor.Server
```

And register crazor in your **program.cs** :

```c#
builder.Services.AddCrazor();
builder.Services.AddCrazorServer();
builder.Services.AddCrazorBlazor();
...
app.UseCrazor();
app.UseCrazorBlazor();
app.UseCrazorServer<CardView>();
```



## Add IStorage provider

The default IStorage provider is the volitile MemoryStorage where all data is simple stored in memory and lost when you restart the process. 

To deploy a service you need a real IStorage provider. Here's how to add the Azure Blob IStorage implementation:

```shell
nuget add Microsoft.Bot.Builder.Azure.Blobs
```

Adding to your **program.cs**:

```C#
var storageKey = builder.Configuration.GetValue<string>("AzureStorage");
if (storageKey != null)
	builder.Services.AddSingleton<IStorage, BlobsStorage>(sp => new BlobsStorage(storageKey, "mybot"));
```



## Add Bot Controller on Blazor Server

Crazor requires that a bot controller for integrating with Teams/Office, etc. To do that in a blazor project you need to add:

```C#
builder.Services.AddControllers();
...
app.UseStaticFiles();
app.UseRouting();
app.MapControllers();
```



# Modifications to your Project

## Create Cards folder

1. add a **Cards** folder (should be a peer to **Pages** folder)

2. Create a **Cards/_Imports.razor** file containing default namespaces:

   ```C#
   @using System.ComponentModel.DataAnnotations;
   @using Crazor.Blazor.Components.AdaptiveCards;
   ```

## Settings

Look at [Settings](../Settings.md) page for information settings for your project 

# (Optional) Create a cards.razor host file

Create a **Pages/cards.razor** file with this content.  This will give you a "landing page" which is bound to the card url that matches it.

```c#
@page "/Cards/{*routeFragment}"
@inject NavigationManager MyNavigationManager
@implements IDisposable
@using Crazor.Blazor.Components

<CardViewer Route="@Route.Route" OnCardRouteChanged="OnCardRouteChanged" @ref="MyCardViewer" />

@code {
    // this is only used to "load the cardview" from the page on initial load.
    [Parameter]
    public string? routeFragment { get; set; }
    CardRoute Route { get; set; }
    public CardViewer MyCardViewer = default!;
    bool routeLoaded = false;

    protected async override Task OnInitializedAsync()
    {
        MyNavigationManager.LocationChanged += OnPageLocationChanged;
        Route = CardRoute.Parse(new Uri(MyNavigationManager.Uri).PathAndQuery);
        routeLoaded = false;
        await base.OnInitializedAsync();
    }

    public void OnPageLocationChanged(object sender, LocationChangedEventArgs locationChanged)
    {
        System.Diagnostics.Debug.WriteLine($"LOCATION: {locationChanged.Location}");
        var route = new Uri(locationChanged.Location).PathAndQuery;
        if (route.StartsWith("/Cards/"))
        {
            CardRoute cardRoute = CardRoute.Parse(route);
            if (cardRoute.App == Route.App)
            {
                if (routeLoaded == false && cardRoute.Path != Route.Route)
                {
                    base.InvokeAsync(async () =>
                    {
                        await MyCardViewer.LoadRouteAsync(route);
                        StateHasChanged();
                    });
                }
                routeLoaded = false;
            }
        }
    }

    public void OnCardRouteChanged(string route)
    {
        if (route.ToLower() != new Uri(MyNavigationManager.Uri).PathAndQuery.ToLower())
        {
            routeLoaded = true;
            MyNavigationManager.NavigateTo(route);
        }
    }

    public void Dispose()
    {
        MyNavigationManager.LocationChanged -= OnPageLocationChanged;
    }
}	
```



# (Optional) Change index.razor to enumerate your cards

Insert this the content of your **Index.razor**

```html
@page "/"
@using Crazor;
@using Microsoft.AspNetCore.Components.Forms
@inject CardAppFactory CardAppFactory
<PageTitle>Cards</PageTitle>
<h2>Cards</h2>
<ul>
    @foreach (var cardAppType in CardAppFactory.GetNames().OrderBy(n => n))
    {
        var appName = cardAppType.Replace("App", String.Empty);
        <li><a href="/Cards/@appName">@appName Card</a></li>
    }
</ul>

```

This will give you an easy way to interacting with your cards:

![image-20221104003206930](../assets/image-20221104003206930.png)
