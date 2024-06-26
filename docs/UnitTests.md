

![image](https://user-images.githubusercontent.com/17789481/197238565-e3f895d0-6def-4d41-aba2-721d5432b1ef.png)

# Writing Unit Tests for cards

**Crazor** has a companion assembly called **Crazor.Test** which implements helper classes/methods to make it super easy to test the logic of your cards. 

To start you create a MSTest test project

![image-20221130143300942](assets/image-20221130143300942.png)

## Add Crazor packages

Add **Crazor.Test** and **Crazor.Blazor** nuget libraries..

```shell
nuget add package Crazor.Test
nuget add package Crazor.Blazor
```

# Modifications to your Web Project

1. Edit the .csproj to make it a aspnetcore project change ```<Project Sdk="Microsoft.NET.Sdk">``` to ``` <Project Sdk="Microsoft.NET.Sdk.Web">```

   ```c#
   <Project Sdk="Microsoft.NET.Sdk.Web">
   ```

2. add a **Cards** folder (should be a peer to **Pages** folder)

3. add **Cards/_Imports.razor** file containing this:

   ```c#
   @using System.ComponentModel.DataAnnotations;
   @using AdaptiveCards
   @using Crazor
   @using Crazor.Blazor
   @using Crazor.Exceptions
   @using Crazor.Attributes
   @using System.Threading;
   @using System.Threading.Tasks;
   @using Crazor.Blazor.Components.Adaptive;
   ```

   

# Creating a unit test

Now we will create a card and write a unit tests against the card.

## Create a test card Foo

1. Create a test card folder in **cards/Foo**

2. Create Card View
   **(Blazor)** - Create a **Default.razor** file in it

   ```html
   @inherits CardView
   <Card Version="1.5">
   	<TextBlock>Counter=@Counter</TextBlock>
       <ActionExecute Verb="@nameof(OnSubmit)"/>
   </Card>
   
   @code {
   	public int Counter {get;set;}
   
   	public void OnSubmit() => Counter++;
   }
   ```

   

3. Now create a unit test for it by creating a .cs file called TestFoo.cs and deriving your class for **CardTest**

   ```c#
   using AdaptiveCards;
   using Crazor.Test;
   using Crazor.Test.MSTest;
   
   namespace MyTests
   {
       [TestClass]
       public class TestFoo : CardTest
       {
           [TestMethod]
           public async Task TestIncrementCounter()
           {
               // create an instance of the card by binding to it's route.
               await LoadCard("/Cards/Foo")
                   	// add assertion against the card
                       .AssertTextBlock("Counter=0")
                   // submit an action
                   .ExecuteAction("OnSubmit")
                   	// write assertions agains the card that is returned
                       .AssertTextBlock("Counter=1");
           }
       }
   }
   ```

That's it! The pattern is essentially that you 

* use base class method **LoadCard(route)** to instantiate a card.  
  * You write assertions against the card 
* call **ExecuteAction()** to send input into the card.
  * You write assertions against the card 

| Method                        | Description                                                  |
| ----------------------------- | ------------------------------------------------------------ |
| **LoadCard(route)**           | Load a card                                                  |
| **ExecuteAction(verb, data)** | Invoke a verb (mimic a button click on an action).  You can pass any arbitrary data payload to simulate input |

## Assertion Methods

| Method                             | Description                                                  |
| ---------------------------------- | ------------------------------------------------------------ |
| **AssertTextBlock(text)**          | Assert there is a textblock with a text value                |
| **AssertTextBlock(id, text)**      | Assert that TextBlock with Id has a text value               |
| **AssertNoTextBlock(text)**        | Assert that there is no TextBlock with a text value          |
| **AssertHas<T>()**                 | Assert there is an element of type T in the card             |
| **AssertHas<T>(id)**               | Assert there is an element of type T with id in the card     |
| **AssertHasNo<T>()**               | Assert there is no element of type T in the card             |
| **AssertHasNo<T>(id)**             | Assert there is no element of type T with id in the card.    |
| **AssertElement<T>(id, callback)** | Find Element of type T and id and pass to the callback for custom assertion. |
| **AssertElements<T>(callback)**    | Find all elements of type T and pass to the callback for custom assertion |
| **AssertCard(callback)**           | call callback with the card for custom assertions.           |



### Writing a custom assertion extension

All of the assertion methods are extensions to **Task<CardTestContext>**, so it is easy to create your own assertion helpers.

Here is the implementation of **AssertTextBlock**.  You can see that you await the task to get the context.  The context has

* **Card** - the adaptive card 
* **Services** - The dependency injection services provider.

And it always returns the context back out so that the assertions can be chained together in a fluent style.

``` C#
public static async Task<CardTestContext> AssertTextBlock(this Task<CardTestContext> contextTask, string id, string text)
{
    var context = await contextTask;
    var actual = context.Card.GetElements<AdaptiveTextBlock>().SingleOrDefault(el => el.Id == id)?.Text;
    Assert.AreEqual(text, actual, $"TextBlock[{id}] Expected:'{text}' Actual:'{actual}'");
    return context;
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

