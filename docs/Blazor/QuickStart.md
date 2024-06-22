

![image](https://user-images.githubusercontent.com/17789481/197238565-e3f895d0-6def-4d41-aba2-721d5432b1ef.png)

# QuickStart

>  Make sure to [Install Prerequisite tools](InstallTools.md).

# Creating a bot project from Visual Studio

Create a **Crazor App Server** project

![image-20240621213410771](C:/Users/therm/AppData/Roaming/Typora/typora-user-images/image-20240621213410771.png)

> NOTE: you can create the project from the cli by using
>
> ```cmd
> dotnet new crazorserver
> ```
>

# Add a dev tunnel for local development

Use 

* **TunnelType=Persistant** 
* **Access=Public** 

![image-20240621213541534](C:/Users/therm/AppData/Roaming/Typora/typora-user-images/image-20240621213541534.png)

Run the project. You will be launched a on page like https://jx4wclpb-7232.usw2.devtunnels.ms/. You will have an error, but that's OK. We just want to get the url for your project. Copy that and save it off someplace.

## Configure tunnel

Click on your devtunnel settings  to manage the tunnel:

![image-20240621213914448](C:/Users/therm/AppData/Roaming/Typora/typora-user-images/image-20240621213914448.png)

Make sure that **Use Tunnnel Domain** is turned on:

![image-20240621214005832](C:/Users/therm/AppData/Roaming/Typora/typora-user-images/image-20240621214005832.png)

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

![image-20240621214750538](C:/Users/therm/AppData/Roaming/Typora/typora-user-images/image-20240621214750538.png)

And if you click on the card you should see this:

![image-20240621214806458](C:/Users/therm/AppData/Roaming/Typora/typora-user-images/image-20240621214806458.png)



