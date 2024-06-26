

![image](https://user-images.githubusercontent.com/17789481/197238565-e3f895d0-6def-4d41-aba2-721d5432b1ef.png)

# Crazor Architecture

Crazor is made using layers so you can mix and match use just the parts you need.

## Libraries

Crazor is made up of 5 libraries

* **Crazor** - Defines the CardApp application programming model independent of hosting.  It needs ICardView implementations to create Cards.
* **Crazor.Server** - Defines a Controller and IBot which implements all of the hosting infrastructure for serving up a Crazor App in Teams/Outlook.
* **Crazor.Blazor** - Defines a implementation of ICardView which uses **.razor** template files to define cards.
* **Crazor.Test** - Defines infrastructure for you to write unit tests for your cards.



## Crazor Server hosting CardApps

![image-20230107140457543](assets/image-20230107140457543.png)

The Crazor is running on a server and servicing any of the CardApps defined there.  The 2 mechanisms for interaction are:

* **CardAppController** - When the card is running a s teams/outlook/office integration, the card UI is delivered via the CardAppController to the client. All rendering is client side and all logic is running server side.
* **Card Host Page** - When someone navigates to the card app host page the card is is hosted via HTML, with all rendering client side and all logic running server side.

This is true regardless of the ICardView implementation that is being used to turn templates into cards.

## Concepts

* [Architecture](docs/Architecture.md) - Describes overall structure of  **Crazor** **application**
* [Card Apps](docs/CardApp.md) - How to create a **CardApp** class to define state and operations against state.
* [Card Views](docs/CardView.md) - General information about Card Views
* [Memory](docs/Memory.md) - Information on persistence and memory 
* [Validation](docs/Validation.md) - Model validation
* [Routing](docs/RoutingCards.md) - Information on customizing urls to support deep linking into cards
* [Authentication](docs/authentication.md) - Authentication
* [Unit tests](docs/UnitTests.md) - Writing unit tests for your cards.
