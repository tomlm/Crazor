# Crazor
**Crazor** is a library which marries Razor with Adaptive Cards to create a super productive 
coding environment focused 100% on building the card.

All of the bot logic is implemented for you, all you do is make templates bound to your data and write
the code behind.  Really.

# Conceptual model 
Conceptually this is an App made up of multiple cards, the logic and data binding.

## App
The CardApp class represents a mini card application or applet.  It is responsible for managing application state for
the application.  Any properties on the CardApp application with **SharedMemory** or **SessionMemory** are 
automatically saved in the KeyValue IStorage provider.

## Memory model
There are 2 memory scopes:
* **Shared** - all properties in the shared scope are tracked and shared with all users interacting with cards of the app.
* **Session** - all properties in the session scope are trackied per user interacting with cards of the app.

## Cards
Each card represents a screen of information bound to data. 

# Programming Model
The programming model is the classes available to program against the conceptual model.

## App => derive frrom CardApp
To define an application you derive a class from CardApp and put it in the Cards/{AppName} folder.

## Cards => CardView<ModelT>
Each card is a view for the application. The view is defined as a razor template via .cshtml.cs file. Just like all razor templates
you can define the model for the view by using a generic.

The CardView has 2 key properties useful for writing data binding:
* @Model => is an instance of the ModelT 
* App => is the shared application object with all shared/session properties

Example razor template binding to the model and app properties.
```xml
<TextBlock>The @App.Name Counter is: @Model.Counter</TextBlock>
```

## Action handlers
Adaptive cards have a *verb* property which is a unique string identifying the action to take.  Crazor automatically hooks 
the verb up to functions defined on the view with appropriate names.

For example:
```xml
<Action.Execute Title="Do some sutff" Verb="DoSomeStuff"/>
```

You can simply write a handler by defining a function.
```cs
@functions {
	public void DoSomeStuff()
	{  
		Model.Counters++;
	}
}
```
Multiple conventions for method names will be attempted:
* void DoSomeStuff()
* Task DoSomeStuffAsync()
* void OnDoSomeStuff()
* Task OnDoSomeStuffAsync()

### OnInitialized()
When a card is navigated to OnInitialized() will be called giving you an opportunity to do any initialization you want.

## Parameter binding
When a method is defined if it has a paramter which is the same as an a property being passed in handler it will automaticaaly be passed.

For example:
```xml
<Input.Text Id="Name" .../>
```
You can get the value from "name".

```C#
public void OnClick(string name)
{

}
```
Parameters are bound from
* input by id
* action.data for the action clicked on

> See **QuizCard** for an example of parameter binding.

## Property binding
Similarly you can define properties two-way bind property between input fields and properties.  This allows you to 
bind the value of an input to the value that's passed in and have two-way binding happen.
```xml
<Input.Text Id="Name" Value="@Name" .../>
@functions {
	[BindProperty]
	public string Name {get;set;}
}
```
In any action handler the Name property will have the value of the input field.

> See **AddressCard** for example of two-way binding.

## Validation attributes
With property binding you can apply validation attributes to get validation automatically on each action handler invocation.
```C#
	[BindProperty]
	[Required]
	[StringLength(50)]
	public string Name {get;set;}
```
* **Model.IsValid** will be true if all validation passes
* **ValidationErrors** will contain a map of property name to an array of error messages for that property.

> See **AddressCard** for example of validation attributes (and TagHelper for displaying them automatically next to the field.)

## Navigation functions
The CardApp maintains a session state property called **CallStack** which is a call stack of cards that have been called.  
Each CardView has 3 methods for controlling that call stack.
* **ShowCard(cardname, model)** This will push the current card on to the stack and load the next card, passing model as the model for the card.
* **Cancel(message)** This will pop the current card off the stack, and the calling card OnXXXXCanceled() will be called with "message" telling you why it was canceled.
* **Close(result)** This will pop the current card off the stack and the calling cards OnXXXCompleted() will be called with the result of the card.

> See **MultiScreenCard** for example of navigating between cards.
> See **AddressCard** for example of passing models between cards/

## Other helper functions
* **AddBannerMessage(message, style)** - gives you ability to add a banner message with background style.  

## TagHelpers
TagHelpers allow you to bundle up a bunch of complex adaptive card layout and replace it with a new tag.
Tag helpers are defined the TagHelper folder by

Creating a RazorTagHelper
* Defining a TagHelper class derived from **RazorTagHelper**
* Add HtmlAttribute properties for the inputs to your tag.
* Create a default.cshtml with **@model XXX ** where XXX is your tagHelper.

You should now be able to use a tag helper as a new tag in templates.
> See **TagHelperCard** for example of **Accordion** and **Person** taghelpers

# Creating a new CardApp
To create a new card app XXX
1. Create Cards/XXX folder
2. Create a class derived from CardApp 
```C#
public class XXXApp : CardApp
```
3. Add any shared/session memory you care about.
4. create default.cshtml file 

## Creating a new card
To create a new card
1. Add a Foo.cshtml to the same folder as the CardApp 
```xml
@inherits CardView<FooModel>
<!--   the Model   ^^^^^^^^ -->

<Card xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
    <TextBlock Size="ExtraLarge" Weight="Bolder">Hello world!</TextBlock>
</Card>
```
2. define a data model FooModel
3. make sure you .cshtml inherits from CardView<FooModel>
```
@inherits CardView<FooModel>
```

## Defining a card data model
You can define the model anywhere, but I kind of like the convention of defining it as code behind for template
so foo.cshtml has the view, foo.cshtml.cs has the datamodel.

# Hosting goo

## Dependency injection stuff.
* **services.AddCardApps()** - Adds all of the dependency injection for everything you need for the bot, defaulting to memory storage implementation.
> NOTE:For deployment in the cloud you will want a storage provider like Azure Storage provider.  
> The sample has it hooked up, you just need to define **AzureStorage** in configuration to enable it.

## Bots stuff
To deploy you will need a bot registration.  In azure portal
1. Create a "MultiTenant" registration only bot, this will give you an appid which you should put into appsettings.json as "MicrosoftAppId"
2. Go to mananage keys, create a new client secret and put it into user-secret configuration as "MicrosoftAppPassword"
3. Change the endpoint for the bot registration to be **https://{YOURSERVICENAME}.azurewebsites.net/api/cardapps**

## Configuration stuff
Configuration needs following keys
* **MicrosoftAppType** - Should be "MultiTenant",
* **MicrosoftAppId** - The appid for your bot registration
* **MicrosoftAppPassword** - The client secret for your bot registration
* **AzureStorage** - The connection string for an Azure Storage account to use.
* **BotUri** - The full uri end point for your registration **https://{YOURSERVICENAME}.azurewebsites.net/api/cardapps**

> NOTE: For BotUri appsettings.json I use the localhost:xxxx/api/cardapps, and in portal I point have it configured with full url.

## Teams stuff
In the Teams folder there is a manifest which is already set up for
* Universal action handling
* Link unfurling
* messaging (if you send a message with your card app name it will return with the card app)
Zip all 3 files up into a .zip file and import into teams and you can chat with your bot/link unfurl, etc.

## Deploying
The web app is jsut a normal Azure web app, just deploy it to the cloud and make sure the configuration is correct for BotUri and AzureStorage

# Project

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft 
trademarks or logos is subject to and must follow 
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.
