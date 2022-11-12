

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

## Custom Routing

If you want to support deep linking you sometimes need to specify more information beyond the normal route pattern. 

To do that with Crazor you do the following 3 things:

1. You add a **[Route]** attribute to tell us how to match a link to your view
2. You override the **GetRoute()** method to gives us the deep link path when sharing
3. You override the **OnLoadRoute()** method to process the deep link and load the state for the card.

Here is an example for the Edit page for Addresses, which is editing the model with address with an id **this.Model.Id**.  We add the following to our **Edit.cshtml** to support the deeper link structure

```c#
@attribute [Route("{addressId}")]

@functions {
    public override string GetRoute() => $"{this.Model.Id}";

    public void OnLoadRoute(string addressId)
        => this.Model = this.App.LoadAddress(AddressId);
}
```

* The **GetRoute()** method says that the deep link for Edit.cshtml should be **/cards/Addresses/Edit/{this.Model.Id}**
* The **[Route("{addressId}")]** attribute tells us how to get the **{addressId}** out of the url 
* The **OnLoadRoute(string addressId)** is called with the **addressId** and loads the appropriate model into the card.





![image](https://user-images.githubusercontent.com/17789481/197365048-6a74c3d5-85cd-4c04-a07a-eef2a46e0ddf.png)
