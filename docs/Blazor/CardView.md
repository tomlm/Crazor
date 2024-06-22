

![image](https://user-images.githubusercontent.com/17789481/197238565-e3f895d0-6def-4d41-aba2-721d5432b1ef.png)

# The Blazor CardView Class

The **Blazor CardView** Implements **ICardView** using a razor template **.razor** file.

There are 3 base classes

## CardView class

The **CardView** defines a **Crazor CardView** template which does not have a **Model** defined and an untyped **App**.

* It has **App** property of **CardApp**.
* **Model** is not defined.

## CardView<AppT> class

The **CardView<AppT>** defines a **Crazor CardView** template which does not have a **Model** defined but it has a strongly typed **CardApp**

* It has **App** property of **AppT**.
* **Model** is not defined.

This allows you to get intellisense and strong type binding over **App** property giving access to custom methods and memory defined on a custom **CardApp** class.

## CardView<AppT, ModelT> class

The **CardView<AppT,ModelT>** defines a **Crazor CardView** template with a **Model** defined and a strongly typed **CardApp**.

* It has a strongly typed **App** property of **AppT**.
* It has a strongly typed **Model** property of **ModelT**

This allows you to get intellisense and strong type binding to the **CardApp** for your application and intellisense and strong type binding to a **ModelT** data model.

# Example 

An example .razor template binding to the model and app properties.

```xml
@inherits CardView<CountersApp, MyModel>
    
<Card Version="1.5">
    <TextBlock Size="AdaptiveTextSize.Large">The @App.Name Counter is: @Model.Counter</TextBlock>
</Card>
```



# Blazor Components for Adaptive Cards

The Crazor.Blazor package defines Blazor components for defining Adaptive Card layouts.

## Containers

| Name        | Description                           |
| ----------- | ------------------------------------- |
| <Card>      | Defines an Card Adaptive Element      |
| <Container> | Defines an Container Adaptive Element |
| <ColumnSet> | Defines a ColumnSet Adaptive Element  |
| <Column>    | Defines a Column Adaptive Element     |
| <ActionSet> | Defines an ActionSet Adaptive Element |
| <ImageSet>  | Defines an ImageSet Adaptive Element  |

## Elements

| Name                              | Description                              |
| --------------------------------- | ---------------------------------------- |
| <TextBlock/>                      | Defines a TextBlock Adaptive Element     |
| <Image />                         | Defines an Image Adaptive Element        |
| <Media />                         | Defines a Media Adaptive Element         |
| <Table /> <TableRow/><TableCell/> | Defines a Table Adaptive Element         |
| <RichTextBlock/>                  | Defines a RichTextBlock Adaptive Element |
| <Factset/>                        | Defines a FactSet Adaptive Element       |

## Inputs

| Name              | Description                                 |
| ----------------- | ------------------------------------------- |
| <InputText/>      | Defines a Input.Text Adaptive Element       |
| <InputChoiceSet/> | Defines an Input.ChoiceSet Adaptive Element |
| <InputDate/>      | Defines an Input.Date Adaptive Element      |
| <InputTime/>      | Defines an Input.Time Adaptive Element      |
| <InputNumber/>    | Defines an Input.Number Adaptive Element    |
| <InputToggle/>    | Defines an Input.Toggle Adaptive Element    |



## Actions

| Name                       | Description                                                  |
| -------------------------- | ------------------------------------------------------------ |
| <ActionExecute />          | Creates an **Action.Execute** Action                         |
| <ActionShowCard />         | Creates an **Action.ShowCard** Action                        |
| <ActionOpenUrl />          | Creates an **Action.OpenUrl** action                         |
| <ActionSubmit />           | Creates an **Action.Submit** action (NOTE: you should use Action.Execute unless you really know what you are doing.) |
| <ActionToggleVisibility /> | Creates an **Action.ToggleVisibility** action                |



### Action Templates
In addition to the standard AdaptiveCard actions Crazor.Blazor defines razor components for a number of frequently defined actions.

| Name                     | Description                                                  |
| ------------------------ | ------------------------------------------------------------ |
| <ActionOK/>              | Action.Execute with **OnOK** verb which by default will call **CloseView(Model)** if the model is valid. |
| <ActionCancel />         | Action.Execute with **OnCancel** verb which by default will call **CancelView(Message)** |
| <ActionCloseView>        | Action.Execute with **OnCloseView** verb which by default will call **CloseView()** regardless of state of model. |
| <ActionShowView />       | Action.Execute with **OnShowView** verb which by default will call **ShowView(Route)** |
| <ActionReplaceView />    | Action.Execute with **OnReplaceView** verb which by default will call **ReplaceView(Route)** |
| <ActionShowTaskModule /> | Action.Execute which show a Teams TaskModule window for the Route |
| <ActionLogin />          | Action.Execute with **OnLogin** verb which by default will force an SSO OAuth flow if the user is not authenticated. |
| <ActionLogout />         | Action.Execute with **OnLogout** verb which by default will log the user out, clearing SSO Credentials. |



# Example

```c#
@using Crazor.Blazor;
@inherits CardView

<Card Version="1.4">
    <TextBlock Size="AdaptiveTextSize.Large">Session Counter:@Counter</TextBlock>

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
