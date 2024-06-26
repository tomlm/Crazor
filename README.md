

![image](https://user-images.githubusercontent.com/17789481/197238565-e3f895d0-6def-4d41-aba2-721d5432b1ef.png)


# Crazor
**Crazor** is a library which marries **Razor Templating** with **Adaptive Cards** to create a super productive 
coding environment focused 100% on building Adaptive Card based experiences.

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

# Documentation

## Quick Start

* [Quick Start](docs/Blazor/QuickStart.md) - Quick start to creating a new Crazor.Blazor App Server 
* [Crazor.Blazor](docs/Blazor/README.md) - templating using **.razor** files using **Blazor** semantics.

## Archicture

* [Architecture](docs/Architecture.md) - Describes overall structure of  **Crazor** **application**

## Concepts

* [Card Apps](docs/CardApp.md) - How to create a **CardApp** class to define state and operations against state.
* [Card Views](docs/CardView.md) - General information about Card Views
* [Memory](docs/Memory.md) - Information on persistence and memory 
* [Routing](docs/RoutingCards.md) - Information on customizing urls to support deep linking into cards
* [Unit tests](docs/UnitTests.md) - Writing unit tests for your cards.

## Advanced topics

* [ICard View](docs/ICardView.md) - Information on **ICardView** interface
* [Installing your card applications into teams](docs/Teams.md) 



![image](https://user-images.githubusercontent.com/17789481/197365048-6a74c3d5-85cd-4c04-a07a-eef2a46e0ddf.png)
