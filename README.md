

![image](https://user-images.githubusercontent.com/17789481/197238565-e3f895d0-6def-4d41-aba2-721d5432b1ef.png)


# Crazor
**Crazor** is a library which marries **Razor Templating** with **Adaptive Cards** to create a super productive 
coding environment focused 100% on building the micro card based applications.

All of the bot logic is implemented for you, all you do is make templates bound to your data and write
the code behind.  Really. 

If you know ASP.NET then Crazor will feel crazy familiar and powerful to you.

# Features

![image](https://user-images.githubusercontent.com/17789481/199912880-bc35becb-9469-4470-9253-612cdf1a9d53.png)

**Crazor** provides:

* **Razor based card templates** - Define your Adaptive Card views using Razor templating markup with logic, leveraging all of the years of tooling built into Visual Studio to make it super productive to author your experience:
  * **Strong-Typing** - you can refactor and get build errors when working with your models
  * **Intellisense** - Visual studio shows errors and auto-completion
  * **Debugger** - Visual studio debugging allows you to set breakpoints in your templates, etc.
* **No need to understand bots** - Crazor comes with the all of the bot protocol implemented, you just write cards.
* **Automatic state management** - no need to worry about the complexity of coming up with a persistence model
* **Rich Data Binding and Validation** - Rich data binding and attribute based validation support.
* **Built-in Navigation Model** - Crazor implements a navigation model allowing you to do nested calls between screens 
* **Built-in Teams integrations** - Your card application can be **unfurled via a link**, pop up as a **Task Module**, a **Tab** etc. Just register your app with teams and it just works.
* **Built-in Card hosting** - Your **card application** is also hosted automatically in your web site, giving people a normal HTTP link they can view and interact with the card.

# Pick a templating engine
 
There are currently 2 Crazor templating implementations:

* [Crazor.MVC](docs/MVC/README.md) - Implements templating using **.cshtml** files using **MVC** semantics.
* [Crazor.Blazor](docs/Blazor/README.md) - Implements templating using **.razor** files using **Blazor** semantics.

[Read more about MVC vs Blazor](docs/MVCVsBlazor.md) 

# Documentation

* [Architecture](docs/Architecture.md) - Describes overall structure of a Crazor hosting environment.
* [Card Apps](docs/CardApp.md) - How to create a **CardApp** class to define state and operations against state.
  * [Card App Memory](docs/Memory.md) - Information on persistence and memory model
  * [ICard View](docs/ICardView.md) - Information on **ICardView** interface
* [Card Routing](docs/RoutingCards.md) - Information on customizing urls to support deep linking into cards
* [Writing Unit tests](docs/UnitTests.md) - Writing unit tests for your cards.

# Setting up your service

* [Setting up your azure web deployment](docs/Deployment.md)  
* [Installing your card applications into teams](docs/Teams.md) 

# Visual Studio Extension 

[Installing an extension for Visual Studio](docs/VSIX.md) 

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
