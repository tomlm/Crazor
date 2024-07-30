

![image](https://user-images.githubusercontent.com/17789481/197238565-e3f895d0-6def-4d41-aba2-721d5432b1ef.png)

# QuickStart

This will walk you through creating a Crazor App Project using Blazor.

# Prerequisites 

[Install Prerequisite tools](../InstallTools.md)

# Creating a new project

## Option 1: Creating from Visual Studio

Create a **Crazor App Server** project

![image-20240621213410771](assets/image-20240621213410771.png)

## Option 2: Creating from CLI

NOTE: you can alternatively create the project from the cli by using
```cmd
dotnet new crazorserver
```

# Add a dev tunnel for local development

For bots to work with Teams and other apps they need a public addressable endpoint. Dev tunnels does exactly that, so we will create a dev tunnel with the following options:

* **TunnelType=Persistant** 
* **Access=Public** 

![image-20240621213541534](assets/image-20240621213541534.png)

Run the project. You will be launched a on page like https://jx4wclpb-7232.usw2.devtunnels.ms/. You will have an error, but that's OK. We just want to get the url for your project. Copy that and save it off someplace.

## Configure tunnel

Click on your devtunnel settings  to manage the tunnel:

![image-20240621213914448](assets/image-20240621213914448.png)

Make sure that **Use Tunnnel Domain** is turned on:

![image-20240621214005832](assets/image-20240621214005832.png)

> **(ALTERNATIVE) Create a public tunnel using ngrok.io** 
>
> ```ngrok http --host-header=preserve https://localhost:7232```
>
> The URL it creates will look something like this: ```https://1a52-50-35-77-214.ngrok-free.app```

# Register your bot 

In the same folder as your csproj, run **RegisterBot** tool to create a development bot called **MyBot-Dev**. 

```cmd
registerbot --name MyBot-Dev --endpoint https://jx4wclpb-7232.usw2.devtunnels.ms/
```

> NOTE: By convention we use **MyBot-Dev** because when we publish to production we want to register **MyBot** as the production bot

# Run project

You should see this:

![image-20240621214750538](assets/image-20240621214750538-1719090917549-6.png)

And if you click on the card you should see this:

![image-20240621214806458](assets/image-20240621214806458.png)

# Next Steps

* [Create your own Hello World app](HelloWorldWalkthrough.md)
* [Create an app with actions](CountersWalkthrough.md)

# More information

* [Card Views](CardView.md) - How to define views with **CardView** with **Blazor**
* [Card Apps](../CardApp.md) - How to create a **CardApp** class to define state and operations against state.
  * [Card App Memory](../Memory.md) - Information on persistence and memory model
* [Card Routing](../RoutingCards.md) - Information on customizing urls to support deep linking into cards
* [Authentication](../Authentication.md) - Authenticating users and Authorizing access to create per-user secure views
* [Writing Unit tests](../UnitTests.md) - Writing unit tests for your cards.
* [Components (Advanced)](Components.md) - How to define reusable components via Blazor Components

![image](https://user-images.githubusercontent.com/17789481/197365048-6a74c3d5-85cd-4c04-a07a-eef2a46e0ddf.png)

  

