

![image](https://user-images.githubusercontent.com/17789481/197238565-e3f895d0-6def-4d41-aba2-721d5432b1ef.png)

# Displaying Validation Errors

The **CardView.ValidationErrors** property is a dictionary of *PropertyPath* -> *List of error messages* .  By default Input tags will automatically insert error messages into the card after the input field based on the ValidationErrors map. You can control this behavior in both MVC and Blazor card views with 2 properties

| Property             | Description                                                  |
| -------------------- | ------------------------------------------------------------ |
| **ShowErrors**       | If set to **ShowErrors=false** then server side validation errors will not be displayed, it is up to the card author to display them |
| **ClientValidation** | By default attributes will be pushed to the client and errors will be displayed there. You can disable this by setting **Client Validation=false** |



#  Validation Display tags

There are 2 built-in tags for display validation errors supported by both **Crazor.MVC** and **Crazor.Blazor**.

| Tag                   | Description                                                  | Example                                      |
| --------------------- | ------------------------------------------------------------ | -------------------------------------------- |
| **ValidationSummary** | This tag will show ALL errors as a block of "Attention" style text | **<ValidationSummary/>**                     |
| **ValidationMessage** | This tag will show the error for a given Id as "Attention" styled text. | **<ValidationMessage Id="Model.Birthday"/>** |

# Processing Validation Errors by hand

To process Validation errors directly you simply write binding code which looks at the **ValidationErrors** collection.

``` C#
@if (ValidationErrors.TryGetValue(this.Id, out var errors))
{
    @foreach (var error in errors)
    {
        <TextBlock Spacing="@None" Color="Attention">@error</TextBlock>
    }
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
