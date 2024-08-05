

![image](https://user-images.githubusercontent.com/17789481/197238565-e3f895d0-6def-4d41-aba2-721d5432b1ef.png)


# Memory 
Cards are small light-weight applications but they pack a punch because of the fact that they are designed to be shared and interacted with across many different scopes.

**Crazor** makes it easy to manage the state of your application's memory by utilizing a common Key-Value store and allowing you to use attributes to define the objects that are stored in that store.

The **CardApp** class represents the state of your application and is accessible from any **CardView** via the **App** property.

Data on the **CardApp** and **CardView** classes which are marked with **memory scope attributes** that define the keys that are used to persist the properties on every round-trip with the server, saving you from the tedium of managing loading and saving properties into a key value store and allowing you to write your application as a stateful application in a stateless server.

Every time a an interaction happens with your application the following process happens:

1. **CardApp.LoadAppAsync(...)** is called.  The default implementation loads state according to **[Memory]** attributes from the IStorage Provider 
2. **CardApp.OnActionExecuteAsync(...)** is called, which dispatches to the current **CardView**, executes verbs, binds data to card templates, etc.
3. **CardApp.SaveAppAsync(...)** is called. The default implementation saves state according to **[Memory]** attributes to the IStorage provider

## Memory attributes

When you put a memory attribute on a property you are defining the **"scope"** of the value...meaning who will have access to that value. Essentially you are defining the key that is used to persist the value...all properties with a given memory attribute on it will be swept up and stored on a record in the key-value store with the key that is behind the attribute.

| Attribute                 | Scope                                               | Description                                                  |
| ------------------------- | --------------------------------------------------- | ------------------------------------------------------------ |
| **[AppMemory]**           | Scoped to the **App.Name**                          | Using **[AppName]** scopes the property to be shared for all users interacting with the app. |
| **[SessionMemory]**       | Scoped to the value of **Route.SessionId**          | Using **[SessionMemory]** scopes the property to be for the current window the user is interacting with, which is accessible on **Route.SessionId**. It is managed directly by Crazor. |
| **[TempMemory]**          | Property is not persisted                           | Using **[TempMemory]** makes the property not persist at all, it will be reset to default value on each action. |
| **[UserMemory]**          | Scoped to the value of **Activity.From.Id**         | Using **[UserMemory]** will scope the property to be the same across all conversations for that application (aka teams) |
| **[ConversationMemory]**  | Scoped to the value of **Activity.Conversation.Id** | Using **[ConversationMemory]** will scope the property to be the same for everyone who is in the same conversation. |
| **[TimeMemory(pattern)]** | Scoped to the **current date** using **pattern**    | Using **[TimedMemory(pattern)]** will scope the property to be persisted given the current time.  For example **[TimedMemory("yyyyMMdd")]** will scope the property to the pattern 20221108, effectively the day as a key. |

**App.SessionId** is automatically managed by Crazor by default. You should probably not muck with it directly

> NOTE: 
>
> * **CardApp** supports all **Memory** attributes
>
> * **CardView** has a default for all properties of **[SessionMemory]**.  You can disable persistence for temporary properties by applying **[TempMemory]** attribute. No other memory attributes are supported on the card view properties.



## Example

```C#
public class MyApp : CardApp
{
    public MyApp(IServiceProvider services)
        : base(services)        {        }

    // Value is the same for everyone 
    [AppMemory]
    public string A { get; set; }

    // Value is unique for each interactive session
    [SessionMemory]
    public int B { get; set; }

    // value is different for each user
    [UserMemory]
    public int C { get; set; }

    // value is shared by everyone in the same day
    [TimeMemory("yyyyMMdd")]
    public Game D {get;set;}

    // value is shared by everyone in the conversation
    [ConversationMemory]
    public Game E {get;set;}
}
```

## Loading data based on route

Let's say you have a Edit.cshtml file which edits a resource route **/Cards/MyApp/{id}/Edit**

* You create a Route attribute which defines the route the view is bound to. 
* You implement OnLoadRoute() to initialize the local view's state for the route.

```html
@inherits CardView
@attribute [Route("{ResourceId}/Edit")]
<Card>
   ...
   <TextBlock>@Resource.Name</TextBlock>
</Card>

@code {
	public string ResourceId {Get;set;}

    public ResourceModel Resource {get;set;}

	public async Task OnLoadRoute(CancellationToken cancellationToken)
	{
		// ... lookup resource using resourceId which will be intiailized from the data in the url/route ...
		this.Resource = await mydb.GetResource(ResourceId);
	}
}
```

Now when someone copies the Url the value in the Url contains the resourceId, and when it is copied into a new environment the OnLoadRoute() method is able to bind to the appropriate data for the url.

## Custom Memory

The memory attributes are annotations which take care of the mechanics of talking to memory, but the underlying key/value storage for the memory is accessible directly.

**Memory** supports the following methods for looking up, saving and deleting an objects:

| Name                             | Description                                                  |
| -------------------------------- | ------------------------------------------------------------ |
| **GetObjectAsync(key)**          | Lookup an object by unique key for the object within the application |
| **GetObjectAsync&lt;T&gt;(key)**       | Lookup an object T by unique key for the object within the application |
| **GetObjectsAsync(keys)**                          | Lookup multiple objects by unique keys within the application |
| **GetObjectsAsync&lt;T&gt;(key)**                        | Lookup multiple objects of type T by unique keys within the application |
| **SaveObjectAsync(key, object)** | Save an object as key                                        |
| **SaveObjectsAsync(IDictionary<string, object> )** | Save multiple objects                                        |
| **DeleteObjectAsync(key)**       | Delete an object by unique key for the object within the application. |
| **DeleteObjectsAsync(keys)**                       | Delete multiple objects by unique key within the application. |

## Override OnLoadAppAsync/OnSaveAppAsync()

A convenient place to manage your own state from your own data base all you need to do is override **OnLoadAppAsync()** and **OnSaveAppAsync()** methods on the **CardApp** class. 

```C#
[TempMemory]
public SomeData MyData {get;set;}

public async virtual Task LoadAppAsync(string? sharedId, string? sessionId, Activity activity, CancellationToken cancellationToken)
{
    // all attribute values are loaded by base.LoadAppAsync()
    await base.LoadAppAsync(sharedId, sessionId, activity, cancellationToken);
 
    MyData = await Memory.GetObjectAsync<SomeData>(key)
}

public async virtual Task SaveAppAsync(CancellationToken cancellationToken)
{
    await Memory.SaveObjectAsync(key, MyData);

    // all memory attribute values are persisted by base.SaveAppAsync()
    await base.SaveAppAsync(cancellationToken);
}
```

>  NOTE1: If you taking over the loading and saving of data into a property you should put a **[TempMemory] attribute** on it so that it is not persisted to session memory, or you will store the data twice.

> NOTE 2: You can make Memory calls from any place, in a CardView or CardApp. It does not need to be in OnLoadAppAsync/OnSaveAppAsync



## Concepts

* [Architecture](Architecture.md) - Describes overall structure of  **Crazor** **application**
* [Card Apps](CardApp.md) - How to create a **CardApp** class to define state and operations against state.
* [Card Views](CardView.md) - General information about Card Views
* [Memory](Memory.md) - Information on persistence and memory 
* [Validation](Validation.md) - Model validation
* [Routing](RoutingCards.md) - Information on customizing urls to support deep linking into cards
* [Authentication](authentication.md) - Authentication
* [Unit tests](UnitTests.md) - Writing unit tests for your cards.



![image](https://user-images.githubusercontent.com/17789481/197365048-6a74c3d5-85cd-4c04-a07a-eef2a46e0ddf.png)
