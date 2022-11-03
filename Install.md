

![image](https://user-images.githubusercontent.com/17789481/197238565-e3f895d0-6def-4d41-aba2-721d5432b1ef.png)

# Adding Crazor 

Adding crazor is super easy.  You basically add the crazor package, register some dependencies and start authoring cards!

## Add Crazor package

```shell
nuget add package crazor
```

Register crazor

```c#
builder.Services.AddCrazor();
```



## Adding IStorage provider

The default IStorage provider is the volitile MemoryStorage where all data is simple stored in memory and lost when you restart the process. 

To deploy a service you need a real IStorage provider. Here's how to add the Azure Blob IStorage implementation:

```shell
nuget add Microsoft.Bot.Builder.Azure.Blobs
```

Registering dependency injection

```C#
var storageKey = builder.Configuration.GetValue<string>("AzureStorage");
if (storageKey != null)
	builder.Services.AddSingleton<IStorage, BlobsStorage>(sp => new BlobsStorage(storageKey, "mybot"));
```

# Modifications to your Web Project

1. add a **Cards** folder

2. Create a **_ViewImports.cshtml** file containing this:

   ```C#
   @using AdaptiveCards
   @using Crazor
   @using Crazor.Exceptions
   @using Crazor.Attributes
   @using System.ComponentModel.DataAnnotations
   @removeTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
   @removeTagHelper *, Microsoft.AspNetCore.Mvc.Razor
   @addTagHelper *, Crazor
   ```

   

# Local development 

* update **HostUri** in your project appsettings.json to the appropriate https://localhost:{PORT}
* *(Optional)* if you have a bot registration add **MicrosoftAppId** to appsettings.json
* *(Optional)* if you have a bot registration store the **MicrosoftAppPassword** in you user-secrets
* *(Optional)* If you have azure storage store **AzureStorage** setting in your user-secrets

