# Configuring your bot

## 1. Install AZ CLI

There are a number ways of installing (see https://learn.microsoft.com/en-us/cli/azure/install-azure-cli-windows)

To install using winget
```winget install -e --id Microsoft.AzureCLI```

Make sure to login 

```az login```

## 2. Install registerbot tool
The **registerbot** tool is a jack of all trades that makes managing bots a walk in the park.

```dotnet new install -g registerbot```

## 3. Register your service as a bot
Let's say you have a bot **Jed**. You will want 2 bots, 

* (Local) a bot named **Jed-Dev** which is running locally on your box.
* (Production) a bot named **Jed** which is running in the cloud

The **registerbot** tool takes care of all of details of managing the registrations.

## Creating local bot pointing to your local service

Bots need to be accessible from the cloud, so for local development we will use ngrok to create a public endpoint for the local bot.

Run this command and copy the ngrok url from the screen.  This creates a publicly addressable URL that is resolved to local aspnet service on port 7232
```ngrok http --host-header=preserve https://localhost:7232```

The URL it creates will look something like this: ```https://1a52-50-35-77-214.ngrok-free.app```

To register your local endpoint as the **Jed-Dev** bot you run **registerbot** in the project folder and associate the **Bot Name** with the **endpoint** like this:

```
registerbot --name Jed-Dev --endpoint https://xxxxxxxxxxxxxxx.ngrok-free.app
```
**registerbot** will update the local configuration/secrets or the remote service configuration/secrets as appropriate!

# 4. Run the project

The project is just an ASP.NET Core web project that implements both bot protocols and hosts the cards:

```dotnet run```

Navigate your browser to ngrok url and you should see a working card!

# 5. Creating a card application

* Create a folder called */cards/Counter*

* Create a file **Default.razor** in that folder

  ```razor
  @inherits CardView
  
  <Card Version="1.4">
      <TextBlock Size="AdaptiveTextSize.Large">Hello world!</TextBlock>
      <TextBlock>Counter:@Counter</TextBlock>
  
      <ActionSet>
          <ActionExecute Title="Increment" Verb="@nameof(OnIncrement)" />
      </ActionSet>
  </Card>
  
  @code {
      public int Counter { get; set; }
  
      public void OnIncrement()
      => this.Counter++;
  }
  ```

  Now go to your website /cards/counter and you should see your card!



# 6. Installing your bot in teams

To install in teams you need a manifest, but no problem, Crazor creates one for you!

To register your bot with teams

* Go to teams
* add Apps
* Upload App and use **https://...ngrok.io/teams.zip** as the url. That's the path to the teams manifest file.

# 7. Publishing

Publish your service to the azure cloud

## Creating production bot pointing to your cloud service

You need to create and register a bot for that endpoint, which again you can just use **registerbot** to do.

```registerbot --name Jed --endpoint https://jed.azurewebsites.net```

**registerbot** will update the deployed services configuration/secrets automatically!

# 
