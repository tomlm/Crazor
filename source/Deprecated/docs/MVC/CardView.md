

![image](https://user-images.githubusercontent.com/17789481/197238565-e3f895d0-6def-4d41-aba2-721d5432b1ef.png)

# The MVC CardView Class

The **CardView** defines a view for the **CardApp** application as a razor template **.cshtml** file.

To make the .cshtml template a **Crazor template** you insert the **@inherits CardView<>** directive defining that CardView is the base class for the view.

## @inherits CardView

The **@inherits CardView** defines a **Crazor CardView** template which does not have a **Model** defined and an untyped **App**.

* It has **App** property of **CardApp**.
* **Model** is not defined.



## @inherits CardView<AppT>

The **@inherits CardView<AppT>** defines a **Crazor CardView** template which does not have a **Model** defined but it has a strongly typed **CardApp**

* It has **App** property of **AppT**.

This allows you to get intellisense and strong type binding over **App** property giving access to custom methods and memory defined on a custom **CardApp** class.

## @inherits CardView<AppT, ModelT>

The **@inherits CardView<AppT,ModelT>** defines a **Crazor CardView** template with a **Model** defined and a strongly typed **CardApp**.

* It has an **App** property of **AppT**.
* It has a **Model** property of **ModelT**

This allows you to get intellisense and strong type binding to the **CardApp** for your application and intellisense and strong type binding to a **ModelT** data model.

Example razor template binding to the model and app properties.
```xml
@inherits CardView<CountersApp, MyModel>
    
<Card Version="1.5">
    <TextBlock>The @App.Name Counter is: @Model.Counter</TextBlock>
</Card>
```

# Verb handlers

![image](https://user-images.githubusercontent.com/17789481/190311953-6cdb8a4d-eebf-4833-af58-915220a4d838.png)

Adaptive cards **Action.Execute** define a ***verb*** which is a unique string identifying the action to take.  **Crazor** automatically hooks 
the verb up to a method on the **CardView**.  This method is called an ***Action Handler*** or ***Verb Handler***

For example:
```xml
<ActionExecute Title="Do some stuff" Verb="OnDoSomeStuff"/>
```

You write the code to respond to it by defining a method with the same name.
```cs
@functions {
	public void OnDoSomeStuff()
	{  
		Model.Counters++;
	}
}
```
>  **Recommendation ** is good practive to use @nameof so that your verb and method names stay in sync and you get a build break when you change one without the other.

```xml
<ActionExecute Title="Do some stuff" Verb="@nameof(OnDoSomeStuff)"/>
```



> NOTE 1 : Any handler can be async by using a return type of **Task**

> NOTE 2: If the **verb does not match** and it **matches the name of a view**  => it will navigate to that view...aka ShowView(verb)

## Verb handler parameter binding

![image](https://user-images.githubusercontent.com/17789481/190312008-c0c144ad-4387-4d84-a883-62b793e1a8c3.png)

Any **input** or **Action.Execute Data** payloads will to be automatically bound to verb handler arguments.



For example:
```xml
<InputText Id="Name" .../>
```
You can get the value for **"name"** by simply adding **string name** as an argument.

```C#
public void OnClick(string name)
{

}
```
Parameters are bound from
* **Id of the Input control** 
* **Action.Execute data** for the action clicked on

## Two-way data binding

If you want an input control to be bound so that the value round-trips you need **two-way data binding**. 

This is accomplished by:

* The input control having the **Id** with the **name** of the property and the **Value** with the **value** of the property

* Defining a property with **[BindProperty]** attribute on it with the same **name**  as the **Id**

![image](https://user-images.githubusercontent.com/17789481/190312063-0de73827-cd0d-4236-98bc-4ab829802a73.png)

## Easier Two-way binding

The input controls all support smart two-way binding via the **Binding** property. The **Binding** property defines a shortcut for Id and Value binding on the input control. 

Example:

```xml
<InputText Binding="Model.Name"  .../>
```

***Binding="Model.Name"*** is a shortcut for ***Id="Model.Name"*** and ***Value="@Model.Name"*** 

# Data Validation

![image](https://user-images.githubusercontent.com/17789481/190312095-542518e7-f9bd-4526-86e1-0e014bd0e4bc.png)

You can apply **data validation attributes** to get validation computed on each action handler invocation.
```C#
	[BindProperty]
	[Required]
	[StringLength(50)]
	public string Name {get;set;}
```
The property **IsModelValid** will be true if all validation attributes are valid. The **ValidationErrors** will contain a map of property name to an array of error messages for that property.

The typical pattern is to only commit and close the data if the validation passes.

```C#
@functions {
    public void OnOK()
    {
        if (IsModelValid)
        {
            App.UpdateAddress(Model);
            CloseView(Model);
        }
    }
}
```

## Adaptive.Input Controls and validation 

Input controls do 2 things automatically related to validation:

1. They **map validation attributes to client side validation** properties to get client side validation as appropriate.
2. They **validate server side as well** (because server side validation is richer than what Adaptive Cards provides)
3. If there are validation errors they will **automatically display the error message** next to the input control that has incorrect ddata.

> NOTE: You can disable this automatic display of validation errors on an input control by setting **ShowErrors="false"**

[See **Validation** for more info on validation errors](docs/Validation.md)

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
        public async Task OnResumeView(CardResult cardResult, CancellationToken cancellationToken)

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



![image](https://user-images.githubusercontent.com/17789481/197365048-6a74c3d5-85cd-4c04-a07a-eef2a46e0ddf.png)
