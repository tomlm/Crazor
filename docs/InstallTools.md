

![image](https://user-images.githubusercontent.com/17789481/197238565-e3f895d0-6def-4d41-aba2-721d5432b1ef.png)

# Installing prerequisite tools

The following need to be installed

* **Azure CLI** - Azure CLI tooling
* **RegisterBot** - magic tool to automatically set up *all* of your bot settings.
* **Crazor.Templates** - dotnet new templates for creating projects/apps/views 

```cmd
winget install Microsoft.AzureCLI
az login
dotnet tool install -g registerbot
dotnet new install Crazor.Templates
```

## Concepts

* [Architecture](Architecture.md) - Describes overall structure of  **Crazor** **application**
* [Card Apps](CardApp.md) - How to create a **CardApp** class to define state and operations against state.
* [Card Views](CardView.md) - General information about Card Views
* [Memory](Memory.md) - Information on persistence and memory 
* [Validation](Validation.md) - Model validation
* [Routing](RoutingCards.md) - Information on customizing urls to support deep linking into cards
* [Authentication](authentication.md) - Authentication
* [Unit tests](UnitTests.md) - Writing unit tests for your cards.

