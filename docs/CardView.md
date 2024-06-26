

![image](https://user-images.githubusercontent.com/17789481/197238565-e3f895d0-6def-4d41-aba2-721d5432b1ef.png)

# Conceptual Model

The crazor service hosts **multiple** **Card applications**.  Each card application represents a micro-app experience that can be used independently and is made up of a views that are defined using Adaptive Cards.

## Card Applications

 A card application is a mini application which is based on AdaptiveCards, using Razor as the templating engine. 

## Card Views

Each card application is made up of one or more multiple **Card Views**. A **card view** is a class that implements ICardView interface to create an Adaptive card based view in the application. 

Crazor implementations of **ICardView** use **reflection**, **annotations** and **conventions** to define the card which is produced.

There are 3 classes in a CardView implementation which reflect typing of the **App** and **Model** properties.
* **CardView** - The **CardView** template has an untyped **Model** and an untyped **App**.
* **CardView&lt;AppT&gt;** - The **CardView** template has an typed **ModelT** and an untyped **App**.
* **CardView&lt;AppT, ModelT&gt; class** - The **CardView** template has an typed **ModelT** and an typed **AppT**.

# Action Verb handlers

![image](https://user-images.githubusercontent.com/17789481/190311953-6cdb8a4d-eebf-4833-af58-915220a4d838.png)

Adaptive cards actions define a ***verb*** which is a unique string identifying the action to take.  **Crazor** automatically hooks the verb up to a method on the **CardView **using reflection and convention.  This method is called an ***Action Handler*** or ***Verb Handler***

For example:
```xml
<ActionExecute Title="Do some stuff" Verb="OnDoSomeStuff"/>
```

You write the code to respond to it by defining a method with the same name **OnDoSomeStuff**
```cs
@code {
	public void OnDoSomeStuff()
	{  
		Model.Counters++;
	}
}
```
>  **Recommendation ** is good practice to use @nameof so that your verb and method names stay in sync and you get a build break when you change one without the other.

```xml
<ActionExecute Title="Do some stuff" Verb="@nameof(OnDoSomeStuff)"/>
```

> NOTE: Any handler can be async by using a return type of **Task**
>
> example: 
>
> ```public async Task OnFoo(CancellationToken ct) {...}```

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

* Defining a property with **[Parameter]** attribute on it 


![image](https://user-images.githubusercontent.com/17789481/190312063-0de73827-cd0d-4236-98bc-4ab829802a73.png)

## Easier Two-way binding

The input controls all support smart two-way binding via the **Binding** property. The **Binding** property defines a shortcut for Id and Value binding on the input control. 

Example:

```xml
<InputText Binding="Model.Name"  .../>
```

***Binding="Model.Name"*** is a shortcut for ***Id="Model.Name"*** and ***Value="@Model.Name"***  
*(or to say another way, The **Id** of the property is the NAME of the property, and the **Value** of the property is the VALUE of the property)*

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
@code {
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

> NOTE: You can disable this validation errors on an input control by setting **ShowErrors="false"**

[See **Validation** for more info on validation errors](docs/Validation.md)

# Life cycle handlers

In addition to the verb based handlers there are a couple of additional of life cycle handlers which are sometimes useful to override.

## OnInitialized()/OnInitializedAsync()

When a card state is new the **OnInitialized()**/**OnInitializedAsync()** method will be called giving you the opportunity to inspect incoming model and/or route to a different cardview. It will only be called once for the lifetime of that view.

```C#
    public override async Task OnInitialized(CancellationToken ct) 
    {
        if (App.Dice == null)
            ShowView("Settings");
        
        await base.OnInitialized(ct);
    }
```



## OnResumeView/OnResumeViewAsync()

When a **child** card calls **CloseView()** or **CancelView()** the **parent** card 's **OnResumeView()/OnResumeViewAsync()** will be called with the **CardResult** object:

| Property    | Type   | Description                                                  |
| ----------- | ------ | ------------------------------------------------------------ |
| **Name**    | String | Name of the card that completed                              |
| **Success** | Bool   | True if CloseView() was called, False if CancelView() was called |
| **Result**  | Object | the result passed to CloseView(). NOTE: The default is the **ICardView.GetModel()** |
| **Message** | String | The message passed to CancelView()                           |

## OnSearchChoices/OnSearchChoicesAsync()

**OnSearchChoices()/OnSearchChoicesAsync()** will be called when a **Input.ChoiceSet** defines a dynamic filted query. 

Example Markup:

```xml
    <InputChoiceSet Binding="Number" Style="Filtered"/>
        <Choice Title="1" Value="1" />
        <Choice Title="2" Value="2" />
        <Choice Title="3" Value="3" />
        <Choice Title="4" Value="4" />
        <DataQuery Dataset="Numbers" />
    </Input.ChoiceSet>
```

And method

```c#
public override async Task&lt;AdaptiveChoice[]&gt; OnSearchChoicesAsync(SearchInvoke search, CancellationToken cancellationToken)
{
    if (search.Dataset == "Numbers")
    {
        return await ...Get Number choices...
    }
    return Array.Empty<AdaptiveChoice>();
}
```

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
