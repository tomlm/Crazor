

![image](https://user-images.githubusercontent.com/17789481/197238565-e3f895d0-6def-4d41-aba2-721d5432b1ef.png)

# The Blazor CardView Class

The **Blazor CardView** Implements **ICardView** using a razor template **.razor** file.

There are 3 base classes

## CardView class

The **CardView** defines a **Crazor CardView** template which does not have a **Model** defined and an untyped **App**.

* It has **App** property of **CardApp**.
* **Model** is not defined.

## CardView&lt;AppT&gt; class

The **CardView&lt;AppT&gt;** defines a **Crazor CardView** template which does not have a **Model** defined but it has a strongly typed **CardApp**

* It has **App** property of **AppT**.
* **Model** is not defined.

This allows you to get intellisense and strong type binding over **App** property giving access to custom methods and memory defined on a custom **CardApp** class.

## CardView&lt;AppT, ModelT&gt; class

The **CardView&lt;AppT,ModelT&gt;** defines a **Crazor CardView** template with a **Model** defined and a strongly typed **CardApp**.

* It has a strongly typed **App** property of **AppT**.
* It has a strongly typed **Model** property of **ModelT**

This allows you to get intellisense and strong type binding to the **CardApp** for your application and intellisense and strong type binding to a **ModelT** data model.

# Example 

An example .razor template binding to the model and app properties.

```xml
@inherits CardView&lt;CountersApp, MyModel&gt;
    
<Card Version="1.5">
    <TextBlock Size="AdaptiveTextSize.Large">The @App.Name Counter is: @Model.Counter</TextBlock>
</Card>
```



# Blazor Components for Adaptive Cards

The Crazor.Blazor package defines Blazor components for defining Adaptive Card layouts.

## Containers

| Name        | Description                           |
| ----------- | ------------------------------------- |
| &lt;Card&gt;      | Defines an Card Adaptive Element      |
| &lt;Container&gt; | Defines an Container Adaptive Element |
| &lt;ColumnSet&gt; | Defines a ColumnSet Adaptive Element  |
| &lt;Column&gt;    | Defines a Column Adaptive Element     |
| &lt;ActionSet&gt; | Defines an ActionSet Adaptive Element |
| &lt;ImageSet&gt;  | Defines an ImageSet Adaptive Element  |

## Elements

| Name                              | Description                              |
| --------------------------------- | ---------------------------------------- |
| &lt;TextBlock/&gt;                      | Defines a TextBlock Adaptive Element     |
| &lt;Image /&gt;                         | Defines an Image Adaptive Element        |
| &lt;Media /&gt;                         | Defines a Media Adaptive Element         |
| &lt;Table /&gt; &lt;TableRow/&gt;&lt;TableCell/&gt; | Defines a Table Adaptive Element         |
| &lt;RichTextBlock/&gt;                  | Defines a RichTextBlock Adaptive Element |
| &lt;Factset/&gt;                        | Defines a FactSet Adaptive Element       |

## Inputs

| Name              | Description                                 |
| ----------------- | ------------------------------------------- |
| &lt;InputText/&gt;      | Defines a Input.Text Adaptive Element       |
| &lt;InputChoiceSet/&gt; | Defines an Input.ChoiceSet Adaptive Element |
| &lt;InputDate/&gt;      | Defines an Input.Date Adaptive Element      |
| &lt;InputTime/&gt;      | Defines an Input.Time Adaptive Element      |
| &lt;InputNumber/&gt;    | Defines an Input.Number Adaptive Element    |
| &lt;InputToggle/&gt;    | Defines an Input.Toggle Adaptive Element    |



## Actions

| Name                       | Description                                                  |
| -------------------------- | ------------------------------------------------------------ |
| &lt;ActionExecute /&gt;          | Creates an **Action.Execute** Action                         |
| &lt;ActionShowCard /&gt;         | Creates an **Action.ShowCard** Action                        |
| &lt;ActionOpenUrl /&gt;          | Creates an **Action.OpenUrl** action                         |
| &lt;ActionSubmit /&gt;           | Creates an **Action.Submit** action (NOTE: you should use Action.Execute unless you really know what you are doing.) |
| &lt;ActionToggleVisibility /&gt; | Creates an **Action.ToggleVisibility** action                |



### Action Templates
In addition to the standard AdaptiveCard actions Crazor.Blazor defines razor components for a number of frequently defined actions.

| Name                     | Description                                                  |
| ------------------------ | ------------------------------------------------------------ |
| &lt;ActionOK/&gt;              | Action.Execute with **OnOK** verb which by default will call **CloseView(Model)** if the model is valid. |
| &lt;ActionCancel /&gt;         | Action.Execute with **OnCancel** verb which by default will call **CancelView(Message)** |
| &lt;ActionCloseView&gt;        | Action.Execute with **OnCloseView** verb which by default will call **CloseView()** regardless of state of model. |
| &lt;ActionShowView /&gt;       | Action.Execute with **OnShowView** verb which by default will call **ShowView(Route)** |
| &lt;ActionReplaceView /&gt;    | Action.Execute with **OnReplaceView** verb which by default will call **ReplaceView(Route)** |
| &lt;ActionShowTaskModule /&gt; | Action.Execute which show a Teams TaskModule window for the Route |
| &lt;ActionLogin /&gt;          | Action.Execute with **OnLogin** verb which by default will force an SSO OAuth flow if the user is not authenticated. |
| &lt;ActionLogout /&gt;         | Action.Execute with **OnLogout** verb which by default will log the user out, clearing SSO Credentials. |



# Example

```c#
@using Crazor.Blazor;
@inherits CardView

<Card Version="1.4"&gt;
    <TextBlock Size="AdaptiveTextSize.Large"&gt;Session Counter:@Counter</TextBlock>

    <ActionExecute Title="Increment" Verb="@nameof(OnIncrement)" />
    <ActionExecute Title="Decrement" Verb="@nameof(OnDecrement)" />
</Card>

@code {
    public int Counter { get; set; }

    public void OnIncrement()
        => this.Counter++;

    public void OnDecrement()
        => this.Counter--;
}
```

![image](https://user-images.githubusercontent.com/17789481/197365048-6a74c3d5-85cd-4c04-a07a-eef2a46e0ddf.png)
