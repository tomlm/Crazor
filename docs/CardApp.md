

![image](https://user-images.githubusercontent.com/17789481/197238565-e3f895d0-6def-4d41-aba2-721d5432b1ef.png)

# Conceptual Model

The web services hosts **multiple** **Card applications**.  Each card application represents a micro-app experience that can be used independently and is made up of a views that are defined using Adaptive Cards.

## Card Applications

 A card application is a mini application which is based on AdaptiveCards, using Razor as the templating engine. 

## Card Views

Each card application is made up of one or more multiple **Card Views**. A card view is a class that defines the UX by binding the data and logic into an Adaptive Card to create a "screen" in the application. 

# The CardApp Class

The **CardApp** serves a couple of roles in the system.

* It manages the shared state for all templates in the folder
* It can receive dependency injected resources (the razor templates don't have direct access to dependency injection)

Any properties and methods you put onto the app class are accessible from all **CardView** objects in the folder via the **App** property.

You do not have to define a card application, but to do so you simply create a class which derives from **CardApp**.

 **CardApp** names have a naming convention with the name of the **folder**. The name of the **folder name** is what shows up in the urls, and the name of the class is the name of the folder + App like this: **"{*name of the folder*}App"**

```Cards/Foobar/FoobarApp.cs```

## Properties

Properties on the **CardApp** are available to all **CardView** in the application via the **App** property.

To load and persist the values of any property you have 2 mechanisms:

* Use **[Memory]** attributes on properties. *See [Memory](Memory) for more details*
* You can override the **LoadAppAsync()** and **SaveAppAsync()** methods to lookup/save your data.

## Methods

You can define any methods you like on the **CardApp**.

A useful pattern is to put methods which manipulate your shared state on the app so that you can consolidate data manipulation methods for all **CardView** classes. For example, you can add CRUD methods which manipulate your data to the **CardApp**, and call **App.Create(...)** from a view to manipulate your data directly.

## Dependency Injection

The **CardApp** is created via dependency injection and so it can get access to any resources that it needs and expose them as properties to the **CardView** templates.

## Example CardApp

```C#
public class ExampleApp : CardApp
{
    public ExampleApp(IServiceProvider services, IConfiguration configuration) : base(services)
    {
        this.Configuration = configuration;
    }
    
    public IConfiguration Configuration {get;set;}

    [SessionMemory]
    public Entity Entity { get; set;}
    
    public async Task LookUpEntity(String id)
    {
        ... 
    }
}
```



# Navigation 

![image](https://user-images.githubusercontent.com/17789481/190312126-9db0ffa6-27ae-4c7a-a311-52df7f4aaaa5.png)

**Crazor** maintains a session based **CallStack** which is a call stack of cards that have been called. The **CardView** class has methods for controlling the call stack.

* **ShowView(cardname, model)** This will push the current card on to the stack and load the next card, passing the model as the model for the card.
* **CancelView(message)** This will pop the current card off the stack, and the calling card OnXXXXCanceled() will be called with "message" telling you why it was canceled.
* **CloseView(result)** This will pop the current card off the stack and the calling cards OnXXXCompleted() will be called with the result of the card.
* **ReplaceView(cardname, model)** This will replace the current card on to the stack with the named card, passing model as the model for the card.

# Helper functions

* **AddBannerMessage(message, style)** - gives you ability to add a banner message with background style.  

![image](https://user-images.githubusercontent.com/17789481/190333148-9cebaef6-978c-4b13-b964-d1092df8bd95.png)



## Search extensions

To implement a teams search extension like this:

![image-20221116110720258](assets/image-20221116110720258.png)

you simply need to:

* override CardApp.**OnSearchQueryAsync**() to return search results

* Update manifest to have a **query** command pointing to your app route.

```c#
public async override Task<SearchResult[]> OnSearchQueryAsync(MessagingExtensionQuery query, CancellationToken cancellationToken)
{
    var results = await ...lookup your data...
    return results.Select(result => new SearchResult()
                           {
                               Title = ...,
                               Subtitle = ...,
                               Text = ...,
                               ImageUrl = ...
                               Route = $"/Cards/{...route for your search result...}"
                           }).ToArray();
}
```

And in your manifest commands section:

```json
        {
          "id": "/Cards/{...your app name...}",
          "type": "query",
          "title": "...",
          "description": "...",
          "initialRun": false,
          "parameters": [
            {
              "name": "search",
              "description": "Enter in search terms",
              "title": "Search"
            }
          ]
        }
```

The **route** returned in the search result will be used when someone clicks on an entry, unfurling the url into the card and inserting it.

# Life cycle handlers

In addition to the verb based handlers there are a couple of additional of life cycle handlers which are useful.

## OnInitialized()

When a card state is new the **OnInitialized()** method will be called giving you the opportunity to inspect incoming model and/or route to a different cardview. It will only be called once for the lifetime of that view.

```C#
    public void OnInitialized() 
    {
        if (App.Dice == null)
            ShowView("Settings");
    }
```

> NOTE: **OnInitialized()** is only **synchronous**, there is no async version of this method, because it is called in a synchronous part of the code.

## OnShowView()

**OnShowView()** is invoked when someone calls **ShowView()** with your card view, or on as the **Refresh** verb or when an unknown verb is processed, effectively treating those situations as a request to bind the data fresh (Aka show a refreshed screen).

A common way to use this is to load fresh data into a view for a given card.

```C#
        /// <summary>
        /// OnShowView() - Called when a your view is started because someon called ShowView()
        /// </summary>
        /// <remarks>
        /// Override this to handle the being shown
        /// </remarks>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>task</returns>
        public async Task OnShowView(CancellationToken cancellationToken)
        {
            ...
        }
```

> NOTE: OnShowView()  is like any verb handler and supports **parameter binding**, aka you can add parameters to your OnShowView

## OnResumeView()

When a **child** card calls **CloseView()** the **parent** card 's **OnResumeView()** will be called with the **CardResult** object:

| Property    | Type   | Description                                                  |
| ----------- | ------ | ------------------------------------------------------------ |
| **Name**    | String | Name of the card that completed                              |
| **Success** | Bool   | True if CloseView() was called, False if CancelView() was called |
| **Result**  | Object | the result passed to CloseView()                             |
| **Message** | String | The message passed to CancelView()                           |

Example:

```C#
        /// <summary>
        /// OnResumeView() - Called when a CardResult has returned back to this view
        /// </summary>
        /// <remarks>
        /// Override this to handle the result that is returned to the card from a child view.
        /// When a view is resumed because a child view has completed this method will
        /// be called giving you an opportunity to do something with the result of the child view.
        /// </remarks>
        /// <param name="cardResult">the card result</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>task</returns>
        public virtual async Task OnResumeView(CardResult cardResult, CancellationToken cancellationToken)

```

## OnSearchChoices()

**OnSearchChoices()** will be called when a **Input.ChoiceSet** defines a dynamic filted query

Example Markup:

```xml
    <InputChoiceSet Binding="Number" Style="Filtered">
        <Choice Title="1" Value="1" />
        <Choice Title="2" Value="2" />
        <Choice Title="3" Value="3" />
        <Choice Title="4" Value="4" />
        <DataQuery Dataset="Numbers" />
    </Input.ChoiceSet>
```

And method

```c#
public override async Task<AdaptiveChoice[]> OnSearchChoicesAsync(SearchInvoke search, CancellationToken cancellationToken)
{
    if (search.Dataset == "Numbers")
    {
        return await myDb.GetNumbersAsnc(...);
    }
    return Array.Empty<AdaptiveChoice>();
}
```

## RenderViewAsync()

The **CardView** class does not have to use Razor templates to create the cards for the view. By overriding **RenderViewAsync()** method you can create an adaptive card any way you would like.

Simply create a class which derives from **CardView<>** (like @inherits above) and override the **RenderViewAsync()** method to return an Adaptive Card bound to your data.

By deriving from CardView<> you still get Crazor data binding, verb action handling, navigation methods etc. 

## Example:

```C#
namespace Example
{
    public class MyCodeView : CardView<CodeOnlyViewApp>
    {
        public int Counter { get; set; }

        public override async Task<AdaptiveCard?> RenderViewAsync(bool isPreview, CancellationToken cancellationToken)
        {
            return new AdaptiveCard("1.5")
            {
                Body = new List<AdaptiveElement>() 
                { 
                    new AdaptiveTextBlock($"Counter is {this.Counter}") 
                },
                Actions = new List<AdaptiveAction>() 
                {
                    new AdaptiveExecuteAction(){ Verb = nameof(OnIncrement), Title = "Increment"}
                }
            };
        }

        public void OnIncrement()
            => this.Counter++;
    }
}
```

To use the view you simply use the generic form of ShowView<>() or ReplaceView<>()

```c#
ShowView<MyCodeView>();
ReplaceView<MyCodeView>();
```

>  NOTE: All memory attributes, state management action handlers work, but you do not get the benefit of any of the Razor TagHelpers functionality, specifically: 
>
>  * **Input** **control** **Binding** property 
>  * **Action.OK**/**Action.Cancel** are implemented as tag helpers
>  * **Custom TagHelpers** 
>
>  Tag Helpers are purely a feature of the Razor template engine.



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
