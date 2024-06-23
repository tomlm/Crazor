

![image](https://user-images.githubusercontent.com/17789481/197238565-e3f895d0-6def-4d41-aba2-721d5432b1ef.png)


# Crazor.Blazor
**Crazor.Blazor** is a library which marries **Blazor** templates (*.razor) with **Crazor** to create a super productive 
coding environment focused 100% on building the **Adaptive Card** based applications.

# Features

![image](https://user-images.githubusercontent.com/17789481/199912880-bc35becb-9469-4470-9253-612cdf1a9d53.png)

**Crazor.Blazor** provides:

* **.RAZOR based card templates** - Define your Adaptive Card views using **.razor** templating markup with logic, leveraging all of the years of tooling built into Visual Studio to make it super productive to author your experience:
  * **Strong-Typing** - you can refactor and get build errors when working with your models
  * **Intellisense** - Visual studio shows errors and auto-completion
  * **Debugger** - Visual studio debugging allows you to set breakpoints in your templates, etc.
* **AdaptiveCard  Blazor components** - giving you intellisense for defining the card layouts
* **CardViewer component** - CardViewer component for embedding Crazor cards into your blazor layout.
* **Authentication** and **Authorization** - Giving you ability to show per user views and make delegated calls on behalf of the user.

# Quick Start

* [Quick Start](QuickStart.md) - create a fully functional Crazor Card bot in 5 minutes.

# Walkthroughs

* [HelloWorld](HelloWorldWalkthrough.md) - Walkthrough creating your first card application.
* [Counters](CountersWalkthrough.md) - Walkthrough showing creating a card with data binding and action handlers.

# Documentation

* [Card Views](CardView.md) - How to define views with **CardView** and **Razor templates**
* [Card Apps](../CardApp.md) - How to create a **CardApp** class to define state and operations against state.
  * [Card App Memory](../Memory.md) - Information on persistence and memory model

* [Card Routing](../RoutingCards.md) - Information on customizing urls to support deep linking into cards
* [Authentication](../Authentication.md) - Authenticating users and Authorizing access to create per-user secure views
* [Writing Unit tests](../UnitTests.md) - Writing unit tests for your cards.
* [Components (Advanced)](Components.md) - How to define reusable components via Blazor Components

# Sample Card Apps
| | |Description|
|---|---|---|
|[HelloWorld](https://crazordemo.azurewebsites.net/Cards/HelloWorld) | [Source](https://github.com/tomlm/crazor/tree/main/source/samples/SharedCards/Cards/HelloWorld) | Hello world |
|[MultiScreen](https://crazordemo.azurewebsites.net/Cards/MultiScreen) | [Source](https://github.com/microsoft/crazor/tree/main/source/samples/SharedCards/Cards/MultiScreen)| Shows **ShowView(),** **CloseView(),** **CancelView()** to navigate between multiple screens |
|[Counters](https://crazordemo.azurewebsites.net/Cards/Counters) | [Source](https://github.com/microsoft/crazor/tree/main/source/samples/SharedCards/Cards/Counters)| Shows **memory** attributes and **verb handlers** |
|[Quiz](https://crazordemo.azurewebsites.net/Cards/Quiz) | [Source](https://github.com/microsoft/crazor/tree/main/source/samples/SharedCards/Cards/Quiz)| Shows **Paramater** binding to **verb handlers** |
|[Binding](https://crazordemo.azurewebsites.net/Cards/Binding) | [Source](https://github.com/microsoft/crazor/tree/main/source/samples/SharedCards/Cards/Binding)| Shows 2-way data binding |
|[Inputs](https://crazordemo.azurewebsites.net/Cards/Inputs) | [Source](https://github.com/microsoft/crazor/tree/main/source/samples/SharedCards/Cards/Inputs)| Shows using **inputs** with **validation** and **passing models** between screens |
|[Dice](https://crazordemo.azurewebsites.net/Cards/Dice) | [Source](https://github.com/microsoft/crazor/tree/main/source/samples/SharedCards/Cards/Dice)| Shows **settings** |
|[Addresses](https://crazordemo.azurewebsites.net/Cards/Addresses) | [Source](https://github.com/microsoft/crazor/tree/main/source/samples/SharedCards/Cards/Addresses)| **Full multipage sample** that shows editing a list of addresses |
|[BingSearch](https://crazordemo.azurewebsites.net/Cards/BingSearch) | [Source](https://github.com/microsoft/crazor/tree/main/source/samples/SharedCards/Cards/BingSearch)| Implements bing search, shows calling **3rd party API** and **paging** |
|[CodeOnlyView](https://crazordemo.azurewebsites.net/Cards/CodeOnlyView) | [Source](https://github.com/microsoft/crazor/tree/main/source/samples/SharedCards/Cards/CodeOnlyView)| Sample showing that you can make templates with code only |
|[DataQuery](https://crazordemo.azurewebsites.net/Cards/DataQuery) | [Source](https://github.com/microsoft/crazor/tree/main/source/samples/SharedCards/Cards/DataQuery)| Shows how to implement **OnSearchChoices** for dynamic **ChoiceSet** |
|[ProductCatalog](https://crazordemo.azurewebsites.net/Cards/ProductCatalog) | [Source](https://github.com/microsoft/crazor/tree/main/source/samples/SharedCards/Cards/ProductCatalog)| Shows **calling an external database** for your data |
|[Components](https://crazordemo.azurewebsites.net/Cards/Components) | [Source](https://github.com/microsoft/crazor/tree/main/source/samples/SharedCards/Cards/Components) | Shows how you can use blazor to create **custom card components** |
|[TaskModule](https://crazordemo.azurewebsites.net/Cards/TaskModule) | [Source](https://github.com/microsoft/crazor/tree/main/source/samples/SharedCards/Cards/TaskModule) | ***TEAMS*** A teams **taskmodule** that lets you experiment with the **CloseTaskModule**() function. |
|[Nuget](https://crazordemo.azurewebsites.net/Cards/Nuget) | [Source](https://github.com/microsoft/crazor/tree/main/source/samples/SharedCards/Cards/Nuget)| ***TEAMS*** Shows **search message extension** |
|[Wordle](https://crazordemo.azurewebsites.net/Cards/Wordle) | [Source](https://github.com/microsoft/crazor/tree/main/source/samples/SharedCards/Cards/Wordle)| Wordle game, shows leveraging TaskModule and sharing to make shared ux. |
|[Table](https://crazordemo.azurewebsites.net/Cards/Table) | [Source](https://github.com/microsoft/crazor/tree/main/source/samples/SharedCards/Cards/Table)| Shows Table support |
|[RichText](https://crazordemo.azurewebsites.net/Cards/RichText) | [Source](https://github.com/microsoft/crazor/tree/main/source/samples/SharedCards/Cards/RichText)| Shows RichText support |
|[Auth](https://crazordemo.azurewebsites.net/Cards/Auth) | [Source](https://github.com/microsoft/crazor/tree/main/source/samples/SharedCards/Cards/Auth) | Authentication sample |

# Setting up Azure

[Setting up your azure web deployment](../Deployment.md)  

# Teams

[Installing your card applications into teams](../Teams.md) 


contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

![image](https://user-images.githubusercontent.com/17789481/197365048-6a74c3d5-85cd-4c04-a07a-eef2a46e0ddf.png)
