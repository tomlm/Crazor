# Setting up your service as a bot

Let's say you have a bot **Jed**. You will want 2 bots:

* *(Local)* a bot named **Jed-Dev** which is running locally on your box.
* *(Production)* a bot named **Jed** which is running in the cloud

# Install registerbot

The **registerbot** tool is a jack of all trades that makes managing bots a walk in the park. To install it simply run:

```dotnet new install -g registerbot```

# Creating development bot 

Bots need to be accessible from the cloud, so for local development we will use a tunnel service to create a public endpoint for the local bot.

## 1. Create a public tunnel 

Create a visual studio devtunnel named your bot name with 

* **TunnelType=Persistant** 
* **Access=Public** 
* **Use Tunnel Domain=true** (```--host-header unchanged --origin-header unchanged```)

Start the project, the URL it creates will look something like this: ```https://ls13q8g5-7232.usw2.devtunnels.ms```

> (ALTERNATIVE) You can use **ngrok.io** for your tunnel
>
> ```ngrok http --host-header=preserve https://localhost:7232```
> 
>The URL it creates will look something like this: ```https://1a52-50-35-77-214.ngrok-free.app```
> 

## 2. Run registerbot to create the bot for local tunnel endpoint

To register your local endpoint as the **Jed-Dev** bot you run **registerbot** in the project folder and associate the **Bot Name** with the **endpoint** like this:

```
registerbot --name Jed-Dev --endpoint {http://PUBLICURLFROMABOVE}
```
**registerbot** will update the local configuration/secrets or the remote service configuration/secrets as appropriate!

# Creating production bot 

## 1. Publish your service to Azure
Simply publish your service to azure cloud.

## 2. Run registerbot to create bot for the cloud endpoint

You need to create and register a bot for that endpoint, which again you can just use **registerbot** to do.

```registerbot --name Jed --endpoint https://jed.azurewebsites.net```

**registerbot** will update the deployed services configuration/secrets automatically!

