
![image](https://user-images.githubusercontent.com/17789481/190311749-737493c7-d65a-46c2-b4e0-366c38d5e915.png)

# Crazor
**Crazor** is a library which marries Razor with Adaptive Cards to create a super productive 
coding environment focused 100% on building the card.

All of the bot logic is implemented for you, all you do is make templates bound to your data and write
the code behind.  Really. (See https://opcardbot.azurewebsites.net/ for samples in the cloud)

# Conceptual model 
Conceptually the web services hosts mutiple apps.
Each **App** is made up of multiple cards, the logic and data binding defined in razor templates using AspNetCore concepts.

## App
The CardApp class represents a mini card application or applet.  It is responsible for managing application state for
the application.  

## Cards
Each card represents a screen of information bound to data. 

# Programming Model
The programming model is the classes available to program against the conceptual model.

## App => derive from CardApp
To define an application you derive a class from CardApp and put it in the Cards/{AppName} folder.

### Memory Scope attributes
App contains state which is shared for all cards in the application. 

Any property you add to the CardApp that has **[SessionMemory]** or **[SharedMemory]** will automatically be persisted in the KeyValue IStorage provider.

* **SharedMemory** - The values for these properties are accessible and shared for all users interacting with the card.
* **SessionMemory** - The values for these properties are scoped only to the user interacting with a card. Each user will get their own session property values.

On the App you can override **OnLoadAppStateAsync()** and **OnSaveAppStateAsync()** to manage data from additional data sources.

> See [Counters ](https://opcardbot.azurewebsites.net/cards/Counters/39983982398) for an example of state attributes
 
> See [Counters source](https://github.com/microsoft/crazor/tree/main/source/samples/OpBot/Cards/Counters) for source code


## Cards => CardView&lt;ModelT&gt;
Each card is a view for the application. The view is defined as a razor template via .cshtml.cs file. Just like all razor templates
you can define the model for the view by using a generic.

### Model Definition
![image](https://user-images.githubusercontent.com/17789481/190311890-b39d3b1f-5e0e-4feb-a49d-478d4dbc8dcd.png)

The CardView has 2 key properties useful for writing data binding:
* @Model => is an instance of the ModelT 
* App => is the shared application object with all shared/session properties

Example razor template binding to the model and app properties.
```xml
<TextBlock>The @App.Name Counter is: @Model.Counter</TextBlock>
```

### Action handlers
![image](https://user-images.githubusercontent.com/17789481/190311953-6cdb8a4d-eebf-4833-af58-915220a4d838.png)

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

> See [Counters ](https://opcardbot.azurewebsites.net/cards/Counters/39983982398) for an example of action handler binding
> 
> See [Counters source](https://github.com/microsoft/crazor/tree/main/source/samples/OpBot/Cards/Counters) for source code


#### OnInitialized()
When a card is navigated to OnInitialized() will be called giving you an opportunity to do any initialization you want.

### Parameter binding
![image](https://user-images.githubusercontent.com/17789481/190312008-c0c144ad-4387-4d84-a883-62b793e1a8c3.png)

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

> See [Quiz card](https://opcardbot.azurewebsites.net/cards/Quiz/39983982398) for an example of parameter binding.
> 
> See [Quiz source](https://github.com/microsoft/crazor/tree/main/source/samples/OpBot/Cards/Quiz) for source code

### Two-way Property binding
![image](https://user-images.githubusercontent.com/17789481/190312063-0de73827-cd0d-4236-98bc-4ab829802a73.png)

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

> See [Address Card](https://opcardbot.azurewebsites.net/cards/Address/dfd398) for an example of property binding
> 
> See [Address Card source](https://github.com/microsoft/crazor/tree/main/source/samples/OpBot/Cards/Address) for source code

### Validation attributes
![image](https://user-images.githubusercontent.com/17789481/190312095-542518e7-f9bd-4526-86e1-0e014bd0e4bc.png)

With property binding you can apply validation attributes to get validation automatically on each action handler invocation.
```C#
	[BindProperty]
	[Required]
	[StringLength(50)]
	public string Name {get;set;}
```
* **Model.IsValid** will be true if all validation passes
* **ValidationErrors** will contain a map of property name to an array of error messages for that property.

> See [Address Card](https://opcardbot.azurewebsites.net/cards/Address/dfd398) for an example of validation 
> 
> See [Address Card source](https://github.com/microsoft/crazor/tree/main/source/samples/OpBot/Cards/Address) for source code

### Navigation functions
![image](https://user-images.githubusercontent.com/17789481/190312126-9db0ffa6-27ae-4c7a-a311-52df7f4aaaa5.png)

The CardApp maintains a session state property called **CallStack** which is a call stack of cards that have been called.  
Each CardView has 3 methods for controlling that call stack.
* **ShowCard(cardname, model)** This will push the current card on to the stack and load the next card, passing model as the model for the card.
* **Cancel(message)** This will pop the current card off the stack, and the calling card OnXXXXCanceled() will be called with "message" telling you why it was canceled.
* **Close(result)** This will pop the current card off the stack and the calling cards OnXXXCompleted() will be called with the result of the card.

> See [MultiScreen Card](https://opcardbot.azurewebsites.net/cards/MultiScreen/ddfdfda8) for an example of navigation
> 
> See [MultiScreen Card source](https://github.com/microsoft/crazor/tree/main/source/samples/OpBot/Cards/MultiScreen) for source code
> 
> See [Address Card](https://opcardbot.azurewebsites.net/cards/Address/ddergegdfda8) for an example of of navigation with model passing between cards.
> 
> See [Address Card source](https://github.com/microsoft/crazor/tree/main/source/samples/OpBot/Cards/Address) for source code

## Other helper functions
* **AddBannerMessage(message, style)** - gives you ability to add a banner message with background style.  

## TagHelpers
TagHelpers allow you to bundle up a bunch of complex adaptive card layout and replace it with a new tag.
Tag helpers are defined the TagHelper folder by

Creating a RazorTagHelper
![image](https://user-images.githubusercontent.com/17789481/190312173-244ac183-195a-4030-a1ea-713697c1030e.png)

* Defining a TagHelper class derived from **RazorTagHelper**
* Add HtmlAttribute properties for the inputs to your tag.
* Create a default.cshtml with **@model XXX ** where XXX is your tagHelper.

You should now be able to use a tag helper as a new tag in templates.
> See [TagHelper Card](https://opcardbot.azurewebsites.net/cards/TagHelper/ddeeqhjc8) for an example source
> 
> See [TagHelper Card source](https://github.com/microsoft/crazor/tree/main/source/samples/OpBot/Cards/TagHelper) for taghelpers source

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
1. Create a **MultiTenant** *registration only bot*, this will give you an appid which you should put into appsettings.json as **"MicrosoftAppId"**
2. Go to mananage keys (there is a link on the bot registration page) create a new client secret and put it into user-secret configuration as **"MicrosoftAppPassword"**
3. Set the endpoint for the bot registration to be **https://{YOURSERVICENAME}.azurewebsites.net/api/cardapps** 
> IMPORTANT NOTE: it is **/api/cardapps** NOT /api/**messages**.  This is because the bot controller that you don't have to write
	is injected automaticly into your webapp, and we don't want
	to conflict with any existing /api/messages endpoint.  It turns out that the end point name of /api/messages is completely a convention that
	was just in our samples and has propagated throughout the world even though there is nothing that depends on that name ending in /messages.  

## Configuration stuff
Configuration needs following keys
* **MicrosoftAppType** - Should be **"MultiTenant"**
* **MicrosoftAppId** - The appid for your bot registration id
* **MicrosoftAppPassword** - The client secret for your bot registration (from Active Directory)
* **AzureStorage** - The connection string for an Azure Storage account to use.
* **BotUri** - The full uri end point for your registration **https://{YOURSERVICENAME}.azurewebsites.net/api/cardapps** NOTE: **/api/cardapps**

> NOTE: For local development I set BotUri appsettings.json to localhost:xxxx/api/cardapps, and in portal I have it configured with full deployed url

## Teams stuff
In the Teams folder there is a manifest which is already set up for
* Universal action handling
* Link unfurling
* messaging (if you send a message with your card app name it will return with the card app)
Edit the manifest.json to have your botId.
	
Zip **all 3 files** up into a .zip file and import into teams and you can chat with your bot/link unfurl, etc.

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
