

![image](https://user-images.githubusercontent.com/17789481/197238565-e3f895d0-6def-4d41-aba2-721d5432b1ef.png)

# Crazor Visual Studio Extensions
There is a Visual Studio Extension  which extends visual studio for Crazor.
It adds
* a template for creating an App
* templates for creating adaptive card crazor views
* A tool which will allow you to paste json from AdaptiveCards.io designer into your cshtml as a XML.
![image](https://user-images.githubusercontent.com/17789481/197404452-1b7da0b9-72e8-4b84-bcf4-5e03caec0d38.png)

![image](https://user-images.githubusercontent.com/17789481/196826860-263d6bfa-093c-4ae3-9c88-8205d316a205.png)

To install it download and install [CrazorExtensions.vsix](https://github.com/microsoft/crazor/raw/main/CrazorExtensions.vsix)


# Hosting goo

## Dependency injection stuff.
* **services.AddCardApps()** - Adds all of the dependency injection for everything you need for the bot, defaulting to memory storage implementation.
> NOTE:For deployment in the cloud you will want a storage provider like Azure Storage provider.  
> The sample has it hooked up, you just need to define **AzureStorage** in configuration to enable it.

## Bots stuff
To deploy you will need a bot registration.  In azure portal
1. Create a **MultiTenant** *registration only bot*, this will give you an appid which you should put into appsettings.json as **"MicrosoftAppId"**
2. Go to mananage keys (there is a link on the bot registration page) create a new client secret and put it into user-secret configuration as **"MicrosoftAppPassword"**
3. Set the endpoint for the bot registration to be **https://{YOURSERVICENAME}.azurewebsites.net/api/cardapps** 
> IMPORTANT NOTE: it is **/api/cardapps** NOT /api/**messages**.  This is because the bot controller that you don't have to write
	is injected automaticly into your webapp, and we don't want
	to conflict with any existing /api/messages endpoint.  It turns out that the end point name of /api/messages is completely a convention that
	was just in our samples and has propagated throughout the world even though there is nothing that depends on that name ending in /messages.  

## Configuration stuff
Configuration needs following keys
* **MicrosoftAppType** - Should be **"MultiTenant"**
* **MicrosoftAppId** - The appid for your bot registration id
* **MicrosoftAppPassword** - The client secret for your bot registration (from Active Directory)
* **AzureStorage** - The connection string for an Azure Storage account to use.
* **HostUri** - The full uri web server  **https://{YOURSERVICENAME}.azurewebsites.net**

> NOTE: For local development I set BotUri appsettings.json to localhost:xxxx/api/cardapps, and in portal I have it configured with full deployed url

## Teams stuff
In the Teams folder there is a manifest which is already set up for
* Universal action handling
* Link unfurling
* messaging (if you send a message with your card app name it will return with the card app)

> Edit the manifest.json to insert your botId!

Zip **all 3 files** up into a .zip file and import into teams and you can chat with your bot/link unfurl, etc.

## Deploying
The web app is jsut a normal Azure web app, just deploy it to the cloud and make sure the configuration is correct for BotUri and AzureStorage

# Project

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft 
trademarks or logos is subject to and must follow 
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.

![image](https://user-images.githubusercontent.com/17789481/197365048-6a74c3d5-85cd-4c04-a07a-eef2a46e0ddf.png)
