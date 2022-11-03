

![image](https://user-images.githubusercontent.com/17789481/197238565-e3f895d0-6def-4d41-aba2-721d5432b1ef.png)


# The CardView Class
The **CardView** defines a view for the **CardApp** application as a razor template **.cshtml** file.

To make the .cshtml template a **Crazor template** you insert the **@inherits CardView<>** directive defining that CardView is the base class for the view.

## @inherits CardView<AppT>

The **@inherits CardView<AppT>** defines a **Crazor CardView** template which does not have a **Model** defined.

* It has **App** property of **AppT**.

This allows you to get intellisense and strong type binding to the **CardApp** for your application.

## @inherits CardView<AppT, ModelT>

The **@inherits CardView<AppT,ModelT>** defines a **Crazor CardView** template with a **Model** defined.

* It has an **App property** of **AppT**.
* It has a **Model property** of **ModelT**

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
<Action.Execute Title="Do some stuff" Verb="DoSomeStuff"/>
```

You write the code to respond to it by defining a method with the same name.
```cs
@functions {
	public void DoSomeStuff()
	{  
		Model.Counters++;
	}
}
```
## Verb Handler Naming

When resolving any verb name (example: **ZZZ**) multiple method names will be attempted:

* **DoZZZ()**
* **DoZZZAsync()**
* **OnZZZ()**
* **OnZZZAsync()**

> NOTE: Any handler can be async by using a return type of **Task**

## Built-in verb handlers

If there is not a verb handler defined the following built-in verb handlers  will automatically execute
* **CloseView** - This will call **CloseView(Model),** but only if the the model is valid (aka IsModelValid == true)
* **CancelView** - This will call **CancelView()**, closing the current view and going back to the caller of the view.
* If the **verb does not match** and it **matches the name of a view**  => it will show.

## Verb handler parameter binding

![image](https://user-images.githubusercontent.com/17789481/190312008-c0c144ad-4387-4d84-a883-62b793e1a8c3.png)

Any **input** or **Action.Execute Data** payloads will to be automatically bound to verb handler arguments.



For example:
```xml
<Input.Text Id="Name" .../>
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
<Input.Text Binding="Model.Name"  .../>
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

1. They map validation attributes to client side validation properties to get client side validation as appropriate.
2. They validate server side as well (because server side validation is richer than what Adaptive Cards provides)
3. If there are validation errors they will automatically display the error message next to the input control that has incorrect ddata.

> NOTE: You can disable this validation errors on an input control by setting **ShowErrors="false"**

# Navigation functions

![image](https://user-images.githubusercontent.com/17789481/190312126-9db0ffa6-27ae-4c7a-a311-52df7f4aaaa5.png)

**Crazor** maintains a session based **CallStack** which is a call stack of cards that have been called. The **CardView** class has methods for controlling the call stack.

* **ShowView(cardname, model)** This will push the current card on to the stack and load the next card, passing the model as the model for the card.
* **CancelView(message)** This will pop the current card off the stack, and the calling card OnXXXXCanceled() will be called with "message" telling you why it was canceled.
* **CloseView(result)** This will pop the current card off the stack and the calling cards OnXXXCompleted() will be called with the result of the card.
* **ReplaceView(cardname, model)** This will replace the current card on to the stack with the named card, passing model as the model for the card.

# Other helper functions

* **AddBannerMessage(message, style)** - gives you ability to add a banner message with background style.  

![image](https://user-images.githubusercontent.com/17789481/190333148-9cebaef6-978c-4b13-b964-d1092df8bd95.png)



# Other Handlers

In addition to the verb based handlers there are a couple of additional handlers which are useful.

## OnInitialized

When a card is loaded the first time **OnInitialized()** will be called giving you the opportunity to initialize your properties and/or route to a different cardview.

```C#
    public void OnInitialized() 
    {
        if (App.Dice == null)
            ShowView("Settings");
    }
```

> NOTE: OnInitialized is only synchronous, there is no async version of this method.



## OnResume

When a card is being resumed from another card calling **CancelView()**/**CloseView()** the **OnResume** handler is called. 

OnResume() is passed a CardResult object with the following properties:

| Property | Type   | Description                                                  |
| -------- | ------ | ------------------------------------------------------------ |
| Name     | String | Name of the card that completed                              |
| Success  | Bool   | True if CloseView() was called, False if CancelView() was called |
| Result   | Object | the result passed to CloseView()                             |
| Message  | String | The message passed to CancelView()                           |

Example:

```C#
    public void OnResume(CardResult result)
    {
        // ....
    }
```

and

```C#
    public Task OnResumeAsync(CardResult result, CancellationToken ct)
    {
        // ....
    }
```


