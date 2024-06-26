

![image](https://user-images.githubusercontent.com/17789481/197238565-e3f895d0-6def-4d41-aba2-721d5432b1ef.png)

# Routing and Cards

Cards Apps and views have routes which mirror the folder structure. 

If you navigate to **/Cards/HelloWorld** you see the **HelloWorldApp** **Default.cshtml** card. 

If you navigate to **/Cards/HelloWorld/About** you will see the **HelloWorldApp** **About.cshtml** card.

Effectively every card defaults to the following route pattern

```
/Cards/{AppFolder}/{ViewName}
```

Routes serve several purposes

1. They provide a HTML place that a user can go to interact with the card, even if the link is shared with them via a place that doesn't support cards (like SMS)
2. The link is used for card **unfurling**. Crazor will automatically turn the HTML URL for a card into the card if it is shared in a card aware app like Teams.
3. The link supports **deep-linking**, meaning any view that you get to you can save a url and share it.

# Custom Routing

If you want to support deep linking you sometimes need to specify more information beyond the normal route pattern. 

To do that with Crazor you do the following 3 things:

1. You add a **[CardRoute]** attribute to tell us how to match a link to your view
2. You override the **OnLoadRoute()** method to process the deep link and load the state for the card.

Here is an example for the Edit page for Addresses, which is editing the model with address with an id **this.Model.Id**.  We add the following to our **Edit.cshtml** to support the deeper link structure

```c#
@attribute [CardRoute("{Model.Id}")]

@functions {
    public void OnLoadRoute()
        => this.Model = this.App.LoadAddress(Model.Id);
}
```

* The **GetRoute()** method says that the deep link for Edit.cshtml should be **/cards/Addresses/Edit/{this.Model.Id}**
* The **[Route("{addressId}")]** attribute tells us how to get the **{addressId}** out of the url 
* The **OnLoadRoute(string addressId)** is called with the **addressId** and loads the appropriate model into the card.

# Query Parameters

You can initialize your card by building in support for mapping query parameters to properties via the **[FromQuery]** or the **[SupplyParameterFromQuery]** attribute.

For example, in this template we have 2 properties that have **[FromQuery]**

```C#
[FromQuery]
public string Name {get;set;}

[SupplyParameterFromQuery]
public string City {get;set;}

[FromRoute("City")]
public string City2 {get;set;}
```

If you construct a url with Name or City query parameters they will be bound to the these properties.

```https://.../cards/example?name=John&city=boston```

Only properties with [FromQuery] attribute will be bound.

Query Parameters are also available by naming convention on verb handlers

```c#
public void OnLoadRoute(string name, string zip, string city)
{

}
```

> NOTE: [FromQuery] is from MVC 
>
> [SupplyParameterFromQuery] is from Blazor



## Concepts

* [Architecture](docs/Architecture.md) - Describes overall structure of  **Crazor** **application**
* [Card Apps](docs/CardApp.md) - How to create a **CardApp** class to define state and operations against state.
* [Card Views](docs/CardView.md) - General information about Card Views
* [Memory](docs/Memory.md) - Information on persistence and memory 
* [Validation](docs/Validation.md) - Model validation
* [Routing](docs/RoutingCards.md) - Information on customizing urls to support deep linking into cards
* [Authentication](docs/authentication.md) - Authentication
* [Unit tests](docs/UnitTests.md) - Writing unit tests for your cards.



![image](https://user-images.githubusercontent.com/17789481/197365048-6a74c3d5-85cd-4c04-a07a-eef2a46e0ddf.png)
