

![image](https://user-images.githubusercontent.com/17789481/197238565-e3f895d0-6def-4d41-aba2-721d5432b1ef.png)

# Authentication and Authorization 

To create per-user authenticated views you need to 

* **Authenticate** - get credentials for the user to identify the user
* **Authorize** - validate the the user has is authorized to perform the action.

# Update your card views

You need to modify you card views to have authenticate the user and authorize their access.

## Authentication

Crazor has built in authentication protocols to do Single-Sign-In (SSO) and OAuth authentication flows via Cards in the client (such as Office). 

### Add [OAuthConnection(connectionName)] attribute

When you add an **[OAuthConnection(...)]** attribute to your card you pass in the *OAuth Connection Name* you configured in the Azure Bot Portal.  Adding this attribute will trigger the authentication flow for your view using the connection as your authentication flow.

When **user is not authenticated**, then the card will be rendered **anonymously:**

* **User** property will have an unauthenticated **ClaimsPrincipal**
* **UserToken** property will be null
* Crazor will include automatically in the Card payload SSO/OAuth flows information to trigger authentication flows.

When **user is authenticated** then the card can be rendered based on the **User**

* **User** property will have an authenticated **ClaimsPrincipal**
* **UserToken** property will have access token for the connection that can be used to call APIs.
* You can use **User.IsInRole(...)** to conditionally transform your template to show per-user view.

Here is an example with Graph API connection called "Graph".

```c#
@inherits CardView
@attribute [OAuthConnection("Graph")]
<Card Version="1.5">
    @if (User.Identity.IsAuthenticated)
    {
        <TextBlock>@User.GetDisplayName()</TextBlock>
    }
    else
    {
        <TextBlock>Anonymous</TextBlock>
    }
</Card>
```

## Authorization 

To authorize users in your CrazorApp you use the **[OAuthConnection]** attribute or the **User** Claims principal for the user against a role or policy.  

### Using [OAuth] attribute

If you add a **[OAuthConnection(connectionName)]** attribute the view will throw an exception if the user doesn't meet the required role or policy.  

This example shows a card that will not display anything to the user if they are not authenticated and in the Admin role.

```c#
@inherits CardView
@attribute [OAuthConnection("Graph")]
@attribute [Authorize(Role="Admin")]
<Card Version="1.5">
    ... Admin only view for card
</Card> 
```



### Using User.IsInRole() method to check claims

You can explicitly add authorization validations by using the **User.IsInRole()** method like this:

```c#
@inherits CardView
@attribute [OAuthConnection("Graph")]
<Card Version="1.5">
    @if (User.Identity.IsAuthenticated)
    {
        @if (User.IsInRole("VIP"))
        {
             <TextBlock>Saluatations VIP @User.GetDisplayName()</TextBlock>
        }
        else
        {
            <TextBlock>@User.GetDisplayName()</TextBlock>
        }
	}
    else
    {
        <TextBlock>Anonymous</TextBlock>
    }
</Card>    
```



### Using IAuthorizationService 
You can also explicit perform authorization validations by using **IAuthorizationService** interface.

```c#
@attribute [OAuthConnection("Graph")]
@inherits CardView
@inject IAuthorizationService AuthService;
...
@code {
    protected async override Task OnInitializedAsync()
    {
        var result = await AuthService.AuthorizeAsync(User, null, policyName="MustBeAdmin");
        ...
    }
}
```


### (Blazor) Using <AuthorizeView> Component

With Blazor you can use **<CascadingAuthenticationState>** and **<AuthorizeView>** to define anonymous and authorized views for your template.

```c#
@attribute [OAuthConnection("Graph")]
@inherits CardView
@using Microsoft.Graph;
@inject GraphServiceClient GraphClient;

<CascadingAuthenticationState>
    <Card Version="1.5">
        <AuthorizeView>
            <Authorized>
                <TextBlock>Graph User is: @UserName</TextBlock>
            </Authorized>
            <NotAuthorized>
                <TextBlock>Anonymous</TextBlock>
            </NotAuthorized>
        </AuthorizeView>
    </Card>
</CascadingAuthenticationState>

@code 
{
    public string UserName { get; set; }

    protected async override Task OnInitializedAsync()
    {
        if (User.Identity.IsAuthenticated)
        {
            var result = await GraphClient.Me.Request().GetAsync();
            UserName = result.DisplayName;
        }

        await base.OnInitializedAsync();
        return;
    }

}
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





![image](https://user-images.githubusercontent.com/17789481/197365048-6a74c3d5-85cd-4c04-a07a-eef2a46e0ddf.png)
