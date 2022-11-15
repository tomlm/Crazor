

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

| Attribute                                 | Scope                                                | Description                                                  |
| ----------------------------------------- | ---------------------------------------------------- | ------------------------------------------------------------ |
| **[SharedMemory]**                        | Bound to the value of **App.SharedId**               | Using **[SharedMemory]** scopes the property to the value for **App.SharedId**, which is built into the Uri and so fixed and shared across all host applications as that same uri is used. |
| **[SessionMemory]**                       | Bound to the value of **App.SessionId**              | Using **[SessionMemory]** scopes the property to be for the current window the user is interacting with, which is accessible on **App.SessionId**. It is managed directly by Crazor. |
| **[TempMemory]**                          | Not bound                                            | Using **[TempMemory]** makes the property not persist at all, it will be reset to default value on each action. |
| **[UserMemory]**                          | Bound to the value of **Activity.From.Id**           | Using **[UserMemory]** will scope the property to be the same across all conversations for that application (aka teams) |
| **[ConversationMemory]**                  | Bound to the value of **Activity.Conversation.Id**   | Using **[ConversationMemory]** will scope the property to be the same for everyone who is in the same conversation. |
| **[TimeMemory(pattern)]**                 | Bound to the **current date** using **pattern**      | Using **[TimedMemory(pattern)]** will scope the property to be persisted given the current time.  For example **[TimedMemory("yyyyMMdd")]** will scope the property to the pattern 20221108, effectively the day as a key. |
| **[PropertyValueMemory(*propertyName*)]** | Bound to the value of the **property name**          | Using **[PropertyValueMemory]** will scope the property to be persisted using the value of another named property. |
| *Coming soon* **[IdentityMemory]**        | Bound to the **current authenticated user identity** | When using an authorized SSO application this will be scoped to the user who is logged in...shared across all applications that have the same user identity. |

**App.SessionId** is automatically managed by Crazor by default. You should probably not muck with it directly

**App.SharedId** is automatically managed by Crazor by default. If you set ```CardApp.AutoSharedId =false``` then you are responsible for setting the value for **App.SharedId**.  The default its that ```AutoSharedId=true```

> NOTE: 
>
> * **CardApp** supports all **Memory** attributes
>
> * **CardView** the default for properties is **[SessionMemory]**.  You can disable persistence for temporary properties by applying **[TempMemory]** attribute. No other memory attributes are supported on the card view properties.



## Example

```C#
public class MyApp : CardApp
{
    public MyApp(IServiceProvider services)
        : base(services)        {        }

    [SharedMemory]
    public string A { get; set; }

    [SessionMemory]
    public int B { get; set; }

    [UserMemory]
    public int C { get; set; }
    
    [TimeMemory("yyyyMMdd")]
    public Game D {get;set;}

    [ConversationMemory]
    public Game E {get;set;}
}
```



## Custom State

To manage your own state from your own data base all you need to do is override **OnLoadAppAsync()** and **OnSaveAppAsync()**.  

```C#
public SomeData MyData {get;set;}

public async virtual Task LoadAppAsync(string? sharedId, string? sessionId, Activity activity, CancellationToken cancellationToken)
{
    // all attribute values are loaded by base.LoadAppAsync()
    await base.LoadAppAsync(sharedId, sessionId, activity, cancellationToken);
 
    MyData = await myDB.Load(...);
}

public async virtual Task SaveAppAsync(CancellationToken cancellationToken)
{
    await myDB.Save(...);

    // all memory attribute values are persisted by base.SaveAppAsync()
    await base.SaveAppAsync(cancellationToken);
}
```

>  NOTE 1: If you want **[Memory] attributes** to continue to work you need to call the **base** implementation when overriding these methods.

>  NOTE 2: You should not put **[Memory] attributes** on properties which are backed by external data sources as that will cause the data be persisted twice, once to the custom database and once to the key value store.

![image](https://user-images.githubusercontent.com/17789481/197365048-6a74c3d5-85cd-4c04-a07a-eef2a46e0ddf.png)
