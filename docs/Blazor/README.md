

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
* **Client-side** The ability for all of your Crazor logic to run on the client as a part of a WASM SPA.

# Installation

[Installing Crazor.Blazor](Install.md) - Installing Crazor.Blazor into your project

* [Settings](../Settings.md) - Settings for Crazor

# Walkthroughs

* [HelloWorld](HelloWorldWalkthrough.md) - Walkthrough creating your first card application.
* [Counters](CountersWalkthrough.md) - Walkthrough showing creating a card with data binding and action handlers.

# Documentation

* [Card Views](CardView.md) - How to define views with **CardView** and **Razor templates**
* [Card Apps](../CardApp.md) - How to create a **CardApp** class to define state and operations against state.
  * [Card App Memory](../Memory.md) - Information on persistence and memory model

* [Card Routing](../RoutingCards.md) - Information on customizing urls to support deep linking into cards
* [Writing Unit tests](../UnitTests.md) - Writing unit tests for your cards.
* [Components (Advanced)](Components.md) - How to define reusuable components via Blazor Components

# Sample Card Apps
| | |Description|
|---|---|---|
|[HelloWorld](https://crazorblazordemo.azurewebsites.net/Cards/HelloWorld) | [Source](https://github.com/microsoft/crazor/tree/main/source/samples/CrazorBlazorDemo/Cards/HelloWorld)| Hello world |
|[MultiScreen](https://crazorblazordemo.azurewebsites.net/Cards/MultiScreen) | [Source](https://github.com/microsoft/crazor/tree/main/source/samples/CrazorBlazorDemo/Cards/MultiScreen)| Shows **ShowView(),** **CloseView(),** **CancelView()** to navigate between multiple screens |
|[Counters](https://crazorblazordemo.azurewebsites.net/Cards/Counters) | [Source](https://github.com/microsoft/crazor/tree/main/source/samples/CrazorBlazorDemo/Cards/Counters)| Shows **memory** attributes and **verb handlers** |
|[Quiz](https://crazorblazordemo.azurewebsites.net/Cards/Quiz) | [Source](https://github.com/microsoft/crazor/tree/main/source/samples/CrazorBlazorDemo/Cards/Quiz)| Shows **Paramater** binding to **verb handlers** |
|[Binding](https://crazorblazordemo.azurewebsites.net/Cards/Binding) | [Source](https://github.com/microsoft/crazor/tree/main/source/samples/CrazorBlazorDemo/Cards/Binding)| Shows 2-way data binding |
|[Inputs](https://crazorblazordemo.azurewebsites.net/Cards/Inputs) | [Source](https://github.com/microsoft/crazor/tree/main/source/samples/CrazorBlazorDemo/Cards/Inputs)| Shows using **inputs** with **validation** and **passing models** between screens |
|[Dice](https://crazorblazordemo.azurewebsites.net/Cards/Dice) | [Source](https://github.com/microsoft/crazor/tree/main/source/samples/CrazorBlazorDemo/Cards/Dice)| Shows **settings** |
|[Addresses](https://crazorblazordemo.azurewebsites.net/Cards/Addresses) | [Source](https://github.com/microsoft/crazor/tree/main/source/samples/CrazorBlazorDemo/Cards/Addresses)| **Full multipage sample** that shows editing a list of addresses |
|[BingSearch](https://crazorblazordemo.azurewebsites.net/Cards/BingSearch) | [Source](https://github.com/microsoft/crazor/tree/main/source/samples/CrazorBlazorDemo/Cards/BingSearch)| Implements bing search, shows calling **3rd party API** and **paging** |
|[CodeOnlyView](https://crazorblazordemo.azurewebsites.net/Cards/CodeOnlyView) | [Source](https://github.com/microsoft/crazor/tree/main/source/samples/CrazorBlazorDemo/Cards/CodeOnlyView)| Sample showing that you can make templates with code only |
|[DataQuery](https://crazorblazordemo.azurewebsites.net/Cards/DataQuery) | [Source](https://github.com/microsoft/crazor/tree/main/source/samples/CrazorBlazorDemo/Cards/DataQuery)| Shows how to implement **OnSearchChoices** for dynamic **ChoiceSet** |
|[ProductCatalog](https://crazorblazordemo.azurewebsites.net/Cards/ProductCatalog) | [Source](https://github.com/microsoft/crazor/tree/main/source/samples/CrazorBlazorDemo/Cards/ProductCatalog)| Shows **calling an external database** for your data |
|[Components](https://crazorblazordemo.azurewebsites.net/Cards/Components) | [Source](https://github.com/microsoft/crazor/tree/main/source/samples/CrazorBlazorDemo/Cards/Components) | Shows how you can use blazor to create **custom card components** |
|[TaskModule](https://crazorblazordemo.azurewebsites.net/Cards/TaskModule) | [Source](https://github.com/microsoft/crazor/tree/main/source/samples/CrazorBlazorDemo/Cards/TaskModule) | ***TEAMS*** A teams **taskmodule** that lets you experiment with the **CloseTaskModule**() function. |
|[Nuget](https://crazorblazordemo.azurewebsites.net/Cards/Nuget) | [Source](https://github.com/microsoft/crazor/tree/main/source/samples/CrazorBlazorDemo/Cards/Nuget)| ***TEAMS*** Shows **search message extension** |
|[Wordle](https://crazorblazordemo.azurewebsites.net/Cards/Wordle) | [Source](https://github.com/microsoft/crazor/tree/main/source/samples/CrazorBlazorDemo/Cards/Wordle)| Wordle game, shows leveraging TaskModule and sharing to make shared ux. |
|[Table](https://crazorblazordemo.azurewebsites.net/Cards/Table) | [Source](https://github.com/microsoft/crazor/tree/main/source/samples/CrazorBlazorDemo/Cards/Table)| Shows Table support |
|[RichText](https://crazorblazordemo.azurewebsites.net/Cards/RichText) | [Source](https://github.com/microsoft/crazor/tree/main/source/samples/CrazorBlazorDemo/Cards/RichText)| Shows RichText support |

# Setting up Azure

[Setting up your azure web deployment](docs/Deployment.md)  

# Teams

[Installing your card applications into teams](docs/Teams.md) 

# Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

# Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft 
trademarks or logos is subject to and must follow 
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.

![image](https://user-images.githubusercontent.com/17789481/197365048-6a74c3d5-85cd-4c04-a07a-eef2a46e0ddf.png)
