

![image](https://user-images.githubusercontent.com/17789481/197238565-e3f895d0-6def-4d41-aba2-721d5432b1ef.png)


# Creating a new app 

We are going to create a card application called **Sample1**

## 1. Create a folder for your app Sample1

The **Cards** folder is a special folder that creates an area for your applications to live (just like Pages organizes your web pages). 

The convention is that each app is a sub-folder in the Cards folder, so we create a folder in **/Cards/Sample1**

## 2. Create a Default.razor file

By default Crazor will load the **Default.razor** file as the initial view for the application represented by the folder for https://host/Cards/Sample1

**/Cards/Sample1/Default.razor**

```xml
@inherits CardView
<Card Version="1.5">
    <TextBlock>Hello world!</TextBlock>
</Card>
```

That's it.  Now run the application and you should see this:

![image-20240622133012226](assets/image-20240622133012226.png)

And if you click on the **Sample1** it takes you to **/Cards/Sample1** and you should see your card view:

![image-20221103115603471](../assets/image-20221103115603471.png)

# Next Steps

* [Create an app with actions](CountersWalkthrough.md)

# More information

* [Card Views](CardView.md) - How to define views with **CardView** with **Blazor**
* [Card Apps](../CardApp.md) - How to create a **CardApp** class to define state and operations against state.
  * [Card App Memory](../Memory.md) - Information on persistence and memory model
* [Card Routing](../RoutingCards.md) - Information on customizing urls to support deep linking into cards
* [Authentication](../Authentication.md) - Authenticating users and Authorizing access to create per-user secure views
* [Writing Unit tests](../UnitTests.md) - Writing unit tests for your cards.
* [Components (Advanced)](Components.md) - How to define reusable components via Blazor Components

![image](https://user-images.githubusercontent.com/17789481/197365048-6a74c3d5-85cd-4c04-a07a-eef2a46e0ddf.png)
