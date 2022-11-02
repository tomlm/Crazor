

![image](https://user-images.githubusercontent.com/17789481/197238565-e3f895d0-6def-4d41-aba2-721d5432b1ef.png)


# Hosting goo

Here's all of the goo for setting up your Crazor project

# Adding Crazor 

To install crazor into a aspnetcore project

## Installing Crazor package

```shell
nuget add package crazor
```

Register crazor

```c#
builder.Services.AddCrazor();
```



## Installing IStorage provider

You will need a IStorage provider for key/value storage of memory. The Azure blob storage library works great

```shell
nuget add Microsoft.Bot.Builder.Azure.Blobs
```

Registering dependency injection

```C#
var storageKey = builder.Configuration.GetValue<string>("AzureStorage");
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
* (Optional) add **MicrosoftAppId** to appsettings.json
* (Optional) store the **MicrosoftAppPassword** and **AzureStorage** in your user-secrets

# Setting up Azure

To make your projectwork you need

* A Web Site
* A bot registration
* A Azure Storage account

## Create an Azure Storage account

Create an **Azure Storage** account for your service to use.

## Create Web App Service

Create an **Azure Web Service** to deploy your service to.

## Create a Bot Registration

To deploy you will need a bot registration.  In azure portal
1. Create a **MultiTenant** ***registration only bot***, this will give you an appid which you should put into appsettings.json as **"MicrosoftAppId"**
2. Go to mananage keys (there is a link on the bot registration page) **create a new client secret**.  
3. **Copy and save it** off someplace safe (don't check it in!) 
4. **Configure the bot** **endpoint** to be **https://{YOURWEBSITE}/api/cardapps** were YOURWEBSITE is obiovusly your web site



## Update Azure Web Service configuration

Update the web service configuration with the following settings.

| key                      | description                                                  | example                              |
| ------------------------ | ------------------------------------------------------------ | ------------------------------------ |
| **MicrosoftAppType**     | The apptype for your bot registration                        | MultiTenant                          |
| **MicrosoftAppId**       | The AppID for your bot                                       | xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx |
| **MicrosoftAppPassword** | The super secret password you kept from the bot registration | ...                                  |
| **HostUri**              | The root url for your web site (Crazor needs this to know what urls to support link unfurling) | https://mywebsite.azurewebsites.net  |
| **BotName**              | A user friendly name for your bot (Crazor needs this to create appropriate title for cards) | My Cool Bot                          |
| **AzureStorage**         | The connection string for your azure storage account         | ... ya know what it looks like...    |





# Deploying

The web app is jsut a normal Azure web app, just deploy it to the cloud.

