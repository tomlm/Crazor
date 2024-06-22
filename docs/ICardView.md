

![image](https://user-images.githubusercontent.com/17789481/197238565-e3f895d0-6def-4d41-aba2-721d5432b1ef.png)

# Conceptual Model

The web services hosts **multiple** **Card applications**.  Each card application represents a micro-app experience that can be used independently and is made up of a views that are defined using Adaptive Cards.

## Card Applications

 A card application is a mini application which is based on AdaptiveCards, using Razor as the templating engine. 

## Card Views

Each card application is made up of one or more multiple **Card Views**. A **card view** is a class that implements ICardView interface to create an Adaptive card based view in the application. 

# The ICardView Interface

**ICardView** defines the interface for the **CardApp** application to ask for an AdaptiveCard to be rendered.  

The **ICardView** fulfils 2 key roles:

* It provides the the interface for the **CardApp** to interact with the CardView template to produce a card
  * BindProperties(), RenderCardAsync(), OnActionAsync(), etc.

* It provides standardized properties and methods for the **author** to interact with
  * App, IsModelValid, ValidationErrors, etc.

```c#
    public interface ICardView
    {
        /// <summary>
        /// Name of the template
        /// </summary>
        string Name { get; }

        /// <summary>
        /// App reference
        /// </summary>
        CardApp App { get; set; }

        /// <summary>
        /// Validation errors for the current view.
        /// </summary>
        Dictionary<string, HashSet<string>> ValidationErrors { get; }

        /// <summary>
        /// Is the current view valid?
        /// </summary>
        bool IsModelValid { get; set; }

        /// <summary>
        /// Get any custom route data for the cardview
        /// </summary>
        /// <returns></returns>
        string GetRoute();

        /// <summary>
        /// Get Model (specificallly @model style model)
        /// </summary>
        /// <returns></returns>
        object? GetModel();

        /// <summary>
        /// Set the Model (if returned by GetModel)
        /// </summary>
        /// <param name="model"></param>
        void SetModel(object model);

        /// <summary>
        /// Enumerate properties on the view which are persistent
        /// </summary>
        /// <returns></returns>
        IEnumerable<PropertyInfo> GetPersistentProperties();

        /// <summary>
        /// Enumerate properties on the view which are bindable and persistent
        /// </summary>
        /// <returns></returns>
        IEnumerable<PropertyInfo> GetBindableProperties();

        /// <summary>
        /// Render the card 
        /// </summary>
        /// <param name="isPreview">IsPreview is signal that anonymous preview card should be returned.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Task AdaptiveCard</returns>
        Task<AdaptiveCard?> RenderCardAsync(bool isPreview, CancellationToken cancellationToken);

        /// <summary>
        /// OnInitializedAsync() - Initalize members
        /// </summary>
        /// <remarks>
        /// This will be called only once to initialize the instance data of the cardview.
        /// </remarks>
        /// <returns>Task</returns>
        Task OnInitializedAsync(CancellationToken cancellationToken);

        /// <summary>
        /// OnValidateModelAsync() - Called to validate model, 
        /// </summary>
        /// <remarks>
        /// sets IsModelValid and fill ValidationErrors collection
        /// </remarks>
        /// <param name="cancellationToken"></param>
        /// <returns>Task</returns>
        Task OnValidateModelAsync(CancellationToken cancellationToken);

        /// <summary>
        /// OnActionAsync() - Called to process an incoming verb action.
        /// </summary>
        /// <remarks>
        /// The default implementation uses reflection to find the name of the method and invoke it.
        /// </remarks>
        /// <param name="action">the action to process</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Task</returns>
        Task OnActionAsync(AdaptiveCardInvokeAction action, CancellationToken cancellationToken);

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
        Task OnResumeViewAsync(CardResult cardResult, CancellationToken cancellationToken);

        /// <summary>
        /// Called to search for choices.
        /// </summary>
        /// <param name="search"></param>
        /// <param name="services"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<AdaptiveChoice[]> OnSearchChoicesAsync(SearchInvoke search, CancellationToken cancellationToken);
    }
```



## ICardView.Name

This is simply the name of the view. 

## ICardView.App

This gives the CardView logic access to the CardApp. 

## ICardView.IsModelValid

When the CardApp performs data validation, the state of the validation is set via the property, again allowing the author to adjust the template output to reflect that the model is valid or not.

## ICardView.ValidationErrors

The CardApp performs data validation, and any errors will be made accessible to the CardView via the shared ValidationErrors object. This allows the author to adjust the template output based on the validation errors. (injecting errors in the produced card for example.)

## ICardView.GetRoute()

The route for the CardApp is defined by the CardApp, (Example: HelloWorldApp => /Cards/HelloWorld). This method gives the cardview the ability to define the subpath underneath the card app.  For example, the AddressesApp would have a route of /Cards/Addresses and the **EditAddress** view GetRoute() might return $"{addressId}/Edit". This would be composited together into final route of **/cards/Addresses/123123/Edit**.

## ICardView.GetPersistentProperties()

This enumerates the list of properties which should be persisted into the session storage for the card view. 

## ICardView.GetBindableProperties()

This enumerates the list of properties which are OK to data bind to. This mostly a security mechanism, as we don't want to allow Action payloads to map to internals of any objects in the system. 

## ICardView.GetModel()/ICardView.SetModel()

These give the ability for the CardApp class to get and set the model for persistance.

## ICardView.OnInitializedAsync()

When a card state is new the **OnInitializedAsync()** method will be called giving you the opportunity to inspect incoming model and/or route to a different cardview. It will only be called once for the lifetime of that view.

```C#
    public override async Task OnInitialized(CancellationToken ct) 
    {
        if (App.Dice == null)
            ShowView("Settings");
        
        await base.OnInitialized(ct);
    }
```

## ICardView.OnValidateModelAsync()

The OnValidateModelAsync method is called to validate that the model is valid setting the **IsModelValid** and **ValidationErrors** properties.  

The default implementation uses reflection and data annotation attributes to validate the model. 

## ICardView.OnActionAsync()

OnActionAsync() is called to process a incoming verb. The default implementation uses reflection to look for action verb handler methods that match the incoming action verb.  So if the action.Verb = "OnFoo" it will look for a method called **OnFoo**. The method can be synchronous or async. 

The ids of input fields will automatically be passed as arguments.  

So if you have a **Input.Text** with **Id="name"** both signatures are valid:

```c#
public void OnFoo(string name /* from the input.text id='name'*/ )
{
    ...
}

public async Task OnFoo(string name /* from the input.text id='name'*/, CancellationToken ct)
{
    ...
}
```

### Built in verb action handlers

There are some verbs which are really common and so there are canonical actions and default implementations to reduce the amount of boiler plate code:

| Verb              | Description                                                  |
| ----------------- | ------------------------------------------------------------ |
| **OnRefresh**     | A **Refresh** secondary action is automatically added to all outbound cards with a verb of **OnRefresh**. There is no default handler, but as simply processing this action creates a refreshed view. |
| **OnOK**          | If a Action has a verb of **OnOK**, and the model is valid the default behavior is to call **CloseView(this.Model)**. The <ActionOK/> component uses this verb. |
| **OnCancel**      | If a action has a verb of **OnCancel**, the default behavior is to call **CancelView()** The <ActionCancel/> component uses this verb. |
| **OnLogin**       | If an action has a verb of **OnLogin** the default behavior is to force a SSO Oauth card flow. The <ActionLogin/> component uses this verb. |
| **OnLogout**      | If an action has a verb of **OnLogout** the default behavior is to sign the user out. The <ActionLogout/> component uses this verb. |
| **OnShowView**    | If an action has a verb of **OnShowView** the default behavior is to call **ShowView(data.Route)**. The <ActionShowView Route="xxx"/> uses this verb |
| **OnReplaceView** | If an action has a verb of **OnReplaceView** the defeault behavarior is to call **ReplaceView(data.Route)**. The <ActionReplaceView Route="xxx"/>  uses this verb. |
| **OnLoad**        | There are paths where cards are initialized based on a route (for example link unfurling). In this case there is a **OnLoad** verb which represents that action. The default behavior is to load that card. |

## 

## ICardView.OnResumeView()

When a **child** card calls **CloseView()** or **CancelView()** the **parent** card 's **OnResumeView()** will be called with the **CardResult** object:

| Property    | Type   | Description                                                  |
| ----------- | ------ | ------------------------------------------------------------ |
| **Name**    | String | Name of the card that completed                              |
| **Success** | Bool   | True if CloseView() was called, False if CancelView() was called |
| **Result**  | Object | the result passed to CloseView(). NOTE: The default is the **ICardView.GetModel()** |
| **Message** | String | The message passed to CancelView()                           |

## ICardView.RenderCardAsync()

This method is the meat of the CardView class.  Implementations should use existing state of the cardview/memory and return an AdaptiveCard. You can create an adaptive card any way you would like by implementing a CardView with RenderCardAsync.

**IsPreview** is passed when the card that is being rendered should be a anonymous preview card (aka, you are sending a card to a recipient and you want to send a preview of the card so that it can be rendered for them base on THEIR credentials). 

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
        return await myDb.GetNumbersAsync(...);
    }
    return Array.Empty<AdaptiveChoice>();
}
```



![image](https://user-images.githubusercontent.com/17789481/197365048-6a74c3d5-85cd-4c04-a07a-eef2a46e0ddf.png)
