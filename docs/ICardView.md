

![image](https://user-images.githubusercontent.com/17789481/197238565-e3f895d0-6def-4d41-aba2-721d5432b1ef.png)

# Conceptual Model

The web services hosts multiple **Card applications**.  Each card application represents a micro-app experience that can be used independently and is made up of a views that are defined using Adaptive Cards.

## Card Applications

Your service can host 1:N **Card Applications**.  A card application is a mini application which is based on AdaptiveCards, using Razor as the templating engine. 

## Card Views

Each card application is made up of 1:N **Card Views**. A card view is a razor template, binding the data and logic to create a "screen" in the application. 

# The ICardView Interface

**ICardView** defines the interface for the **CardApp** application to ask for an AdaptiveCard to be rendered.  

Currently there are 2 implementations of **ICardView**

* **Crazor.MVC** - Provides an implementation of ICardView using **MVC** style **.cshtml** templates.
* **Crazor.Blazor** - Provides an implementation of ICardView using **Blazor** style **.razor** templates.

The **ICardView** fulfils 2 pieces of the contract:

* It provides the the interface for the **CardApp** to interact with the CardView template to produce a card
  * BindProperties(), RenderCardAsync(), OnActionAsync(), etc.

* It provides standardized properties and methods for the **author** to interact with
  * App, IsModelValid, ValidationErrors, etc.

```c#
public interface ICardView
{
	string Name { get; }
    
    CardApp App { get; set; }
    
    Dictionary<string, HashSet<string>> ValidationErrors { get; set; }
    
    bool IsModelValid { get; set; }
    
    string GetRoute();
    
    object? GetModel();
    
    void SetModel(object? model);
    
    IEnumerable<PropertyInfo> GetPersistentProperties();
    
    IEnumerable<PropertyInfo> GetBindableProperties();
    
    Task<AdaptiveCard?> RenderCardAsync(bool isPreview, CancellationToken cancellationToken);
    
    Task OnActionAsync(AdaptiveCardInvokeAction action, CancellationToken cancellationToken);
    
    Task OnResumeView(CardResult screenResult, CancellationToken cancellationToken);

    Task<AdaptiveChoice[]> OnSearchChoices(SearchInvoke search, CancellationToken cancellationToken);
}
```



## ICardView.Name

This is simply the name of the view. 

## ICardView.App

This simple gives the CardView access to the CardApp as context. When it's instantiated the CardApp will set this data on the CardView. 

## ICardView.ValidationErrors

The CardApp performs data validation, and any errors will be made accessible to the CardView via the shared ValidationErrors object. This allows the author to adjust the template output based on the validation errors. (injecting errors in the produced card for example.)

## ICardView.IsModelValid

When the CardApp performs data validation, the state of the validation is set via the property, again allowing the author to adjust the template output to reflect that the model is valid or not.

## ICardView.GetRoute()

The route for the CardApp is defined by the CardApp, (Example: HelloWorldApp => /Cards/HelloWorld). This method gives the cardview the ability to define the subpath underneath the card app.  For example, the AddressesApp would have a route of /Cards/Addresses and the edit view GetRoute() would return $"{addressId}/Edit". This would be composited together into final cardroute of **/cards/Addresses/123123/Edit**.

## ICardView.GetPersistentProperties()

This enumerates the list of properties which should be persisted into the session storage for the card view. 

## ICardView.GetBindableProperties()

This enumerates the list of properties which are OK to data bind to. This mostly a security mechanism, as we don't want to allow Action payloads to map to internals of any objects in the system. The default implementation uses [BindProperty] attribute and other indicators to decide which properties are bindable.

## ICardView.GetModel()/ICardView.SetModel()

Some templating engines strongly typed models that need to be persisted/hooked up.  CardApp will handle these special, and the cardview will get the object in a way that allows it to do "Special" things with the model. For Example with MVC we need to stuff the model into the AspNet ViewBag, and not just reflection.

## ICardView.OnInitialize()

This method will be called when a card view session is initialized. It will only be called once for the lifetime of the view in the session. 

> NOTE: This is a sync only call.

## ICardView.OnActionAsync()

This method is called by CardApp to ask a cardview to process an Action.Execute verb.  The expectation is that card view processes the action and changes state (either by changing memory or calling App methods like ShowView(), CloseView(), CancelView(), etc.)

> NOTE: Unless you need to you don't really have to implement this.  There is a helper method on CardApp called **OnActionReflectionAsync** which uses reflection.  We weren't able to bake this into a base class because Razor/MVC require us to derive from their class.
>
> NOTE 2: It may be this needs to be refactored into a static helper or somethign.

```C#
        public async virtual Task OnActionAsync(AdaptiveCardInvokeAction action, CancellationToken cancellationToken)
        {
            await this.App.OnActionReflectionAsync(action, cancellationToken);
        }
```



## ICardView.OnResumeView()

When a **child** card calls **App.CloseView()** the **parent** card 's **OnResumeView()** will be called with the **CardResult** object:

| Property    | Type   | Description                                                  |
| ----------- | ------ | ------------------------------------------------------------ |
| **Name**    | String | Name of the card that completed                              |
| **Success** | Bool   | True if CloseView() was called, False if CancelView() was called |
| **Result**  | Object | the result passed to CloseView(). NOTE: The default if nothing is passed is the **ICardView.GetModel()** |
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
        public async Task OnResumeView(CardResult cardResult, CancellationToken cancellationToken)

```

## ICardView.RenderCardAsync()

This method is the meat of the CardView class.  Implementations should use existing state of the cardview/memory and return an AdaptiveCard. You can create an adaptive card any way you would like by implementing a CardView with RenderCardAsync.

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



## ICardView.OnSearchChoices()

**OnSearchChoices()** will be called when a **Input.ChoiceSet** defines a dynamic filted query. This is defined as a ICardView method so that the search code can be colocated with the consumption.

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

## 

![image](https://user-images.githubusercontent.com/17789481/197365048-6a74c3d5-85cd-4c04-a07a-eef2a46e0ddf.png)
