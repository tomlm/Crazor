

![image](https://user-images.githubusercontent.com/17789481/197238565-e3f895d0-6def-4d41-aba2-721d5432b1ef.png)


# Reusable Components
You can create reusable components that use Adaptive Card markup in it to reduce duplicated template definitions just like any Blazor project.

# 1. Create a razor file for your component

Let's say we want to create a new tag &lt;Person&gt; that is data bound to a AD.  To do that we simply create a **Person.razor** definition that is a fragment of markup we want to use.

```C#
<ColumnSet Id="@Id">
    <Column Width="AdaptiveWidth.Auto"HorizontalAlignment="AdaptiveHorizontalAlignment.Center">
        <Image Style="AdaptiveImageStyle.Person" Url="@Url" Size="@Size" />
        <TextBlock Weight="AdaptiveTextWeight.Bolder" Wrap="true">@Name</TextBlock>
    </Column>
    <Column Width="AdaptiveWidth.Stretch" VerticalContentAlignment="AdaptiveVerticalContentAlignment.Center" >
        <TextBlock Wrap="true" HorizontalAlignment="AdaptiveHorizontalAlignment.Center">@Text</TextBlock>
    </Column>
</ColumnSet>

@code
{
    [Parameter]
    public string Id { get; set; }

    [Parameter]
    public string Url { get; set; } = string.Empty;

    [Parameter]
    public AdaptiveImageSize Size { get; set; } = AdaptiveImageSize.Small;

    [Parameter]
    public string? Name { get; set; } = string.Empty;

    [Parameter]
    public string? Text { get; set; } = string.Empty;
}
```

# 2. Use component in another .razor file

Now we can use this new component in another  Crazor Card View razor file:

```c#
    <Person Name="Tom Laird-McConnell" Url="https://avatars.githubusercontent.com/u/17789481?s=96"
            Size="@AdaptiveImageSize.Medium" Text="AdaptiveCard guru extraordinaire." />
```

# More information

* [Card Views](CardView.md) - How to define views with **CardView** with **Blazor**
* [Card Apps](../CardApp.md) - How to create a **CardApp** class to define state and operations against state.
  * [Card App Memory](../Memory.md) - Information on persistence and memory model
* [Card Routing](../RoutingCards.md) - Information on customizing urls to support deep linking into cards
* [Authentication](../Authentication.md) - Authenticating users and Authorizing access to create per-user secure views
* [Writing Unit tests](../UnitTests.md) - Writing unit tests for your cards.
* [Components (Advanced)](Components.md) - How to define reusable components via Blazor Components




![image](https://user-images.githubusercontent.com/17789481/197365048-6a74c3d5-85cd-4c04-a07a-eef2a46e0ddf.png)
