

![image](https://user-images.githubusercontent.com/17789481/197238565-e3f895d0-6def-4d41-aba2-721d5432b1ef.png)

# Setting up Azure

To make your projectwork you need

* A Web Site
* A Azure Bot registration
* An Azure Storage account

## Create an Azure Storage account

Create an **Azure Storage** account for your service to use.

## Create Web App Service

Create an **Azure Web Service** to deploy your service to.

## Create a Bot Registration

To deploy you will need a bot registration.  In azure portal go to [Create an Azure Bot - Microsoft Azure](https://ms.portal.azure.com/#create/Microsoft.AzureBot)
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

> 



![image](https://user-images.githubusercontent.com/17789481/197365048-6a74c3d5-85cd-4c04-a07a-eef2a46e0ddf.png)
