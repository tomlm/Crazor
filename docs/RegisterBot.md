

![image](https://user-images.githubusercontent.com/17789481/197238565-e3f895d0-6def-4d41-aba2-721d5432b1ef.png)

# RegisterBot

A **Bot** is nothing more than **contact record** which maps a **contact name** to deployed **web service endpoint**.

Each deployed service needs a bot registration so that users can interact with it.

Let's say you have a bot named **Gronk**. You will want to create 2 bots:

* [Create a local bot](#Register a development Bot) named **Gronk-Dev** which is running locally on your box for debugging.
* [Create a production bot](#Register a production bot) named **Gronk** which is running in the cloud for production.

The **RegisterBot** tool is a jack of all trades that makes managing bots a walk in the park. 

# Install RegisterBot

1. Install AZ CLI [How to install the Azure CLI | Microsoft Learn](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli)

2. run ```az login```

3. Install dotnet msidentity 
   ``` dotnet tool install -g microsoft.dotnet-msidentity```

4. Install the **RegisterBot** CLI tool by running
   ```dotnet tool install -g registerbot```

All steps together:

```cmd
winget install Microsoft.AzureCLI
az login
dotnet tool install -g microsoft.dotnet-msidentity
dotnet tool install -g registerbot
```

## Usage

```shell
RegisterBot --name [botName] -endpoint [endpoint]

Creates a bot registration for [botName] pointing to [endpoint] with teams channel enabled.
```

There are 2 ways to run use **RegisterBot**:

* **interactive** If you pass in no parameters it will prompt you for the bot name, resource-group and endpoint.
* **arguments** you can pass in arguments to script the creation process

It will create all necessary resources as needed and update the settings for the web site all from one command.


# Register a development Bot

For your local bot to be accessible from teams you will need to use tunnelling service to create a public endpoint for the local bot.

## 1. Create a public tunnel using VS devtunnels

Create a visual studio devtunnel named your bot name with 

* **TunnelType=Persistant** 
* **Access=Public** 
* **Use Tunnel Domain=true** (```--host-header unchanged --origin-header unchanged```)

Start the project, the URL it creates will look something like this: ```https://ls13q8g5-7232.usw2.devtunnels.ms```

> **(ALTERNATIVE) Create a public tunnel using ngrok.io** 
>
> ```ngrok http --host-header=preserve https://localhost:7232```
>
> The URL it creates will look something like this: ```https://1a52-50-35-77-214.ngrok-free.app```
>

## 2. Run RegisterBot to create a bot for the local endpoint

To register your local service using the public tunnel endpoint you run **registerbot** in the project folder and associate the **Bot Name** with the **endpoint** by running **RegisterBot** like this:

```
registerbot --name Gronk-Dev --endpoint https://xxxxxxxx-xxxxxx.xxx.devtunnels.ms
```
**registerbot** will update the local settings.development.json and user-secrets for all settings!

That's it you have a local dev bot!

# Register a production bot 

Your production bot simply needs to be published to the web and then a bot registered which points to it.

## 1. Publish your service to Azure
Publish your service to azure cloud, for example: https://gronk.azurewebsites.net 

## 2. Run RegisterBot to create a bot for published endpoint

Run **RegisterBot** to register the endpoing.

```registerbot --name Gronk --endpoint https://Gronk.azurewebsites.net```

**registerbot** will update the **service configuration and secrets** automatically, so your bot is good to go! It's like magic!



# Appendix

What does RegisterBot do?

It does a ton of error-prone administrivia for you:

* Ensures a resource group 
* Ensures there is an Entra Application ID for the bot
* Configures an SSO redirect URI for application ID that redirects to your endpoint.
* Configures you as an owner for the AppId
* Enables basic read user scopes for oauth2PermissionScopes
* Configures preauthorized application guids for bot channels
* Creates or updates a Bot registration for your endpoint 
* Enables Teams channel
* Enables m365 channel (outlook actionable messages)
* Roll secrets 
* Update secrets/settings for all of the above 
  * for local endpoints it updates appsettings.development.json and user-secrets)
  * for product endpoints it updates azure web settings/key vault secrets
* Changes launchsettings.json to launch local endpoints.

Literally making it a one line command to make your bot "just work" (tm). 

![image](https://user-images.githubusercontent.com/17789481/197365048-6a74c3d5-85cd-4c04-a07a-eef2a46e0ddf.png)
