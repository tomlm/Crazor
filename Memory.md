

![image](https://user-images.githubusercontent.com/17789481/197238565-e3f895d0-6def-4d41-aba2-721d5432b1ef.png)


# Memory State 
Cards are inherently about both 1:1 interactions with a user as well as shared interactions among a group of users. 

**Crazor** makes it easy to manage the state of your application's memory around these 2 scopes. 

The **CardApp** class represents your application state and is accessible from any **CardView** via the **App** property.

Data on the **CardApp** and **CardView** classes which are marked with **memory scope attributes** are automatically persisted on each round-trip with the server, saving you from the tedium of managing loading and saving properties into a key value store and allowing you to write your application as a stateful application in a stateless server.

## Memory Scope attributes

Properties which have the **[SessionMemory]** or **[SharedMemory]** will automatically be persisted in the KeyValue **IStorage** provider.

* **[SharedMemory]** - The values for these properties are persisted and the same for all users interacting with the app,
* **[SessionMemory]** - The values for these properties are scoped only to the user interacting with the app.

> NOTE: 
>
> * **CardApp** supports **[SharedMermory]** and **[SessionMemory]** attributes.
>
> * **CardView** supports only **[SessionMemory]** attributes



## Defining the SharedMemoryId

The id which is used to key off of for the **[SharedMemory]** is called the **SharedId**.  The default value for the sharedId is the name of the application, but you can override it by implementing the **GetSharedId()** method.

For example, if you want your shared memory to associated with the id of the record your card is bound to, you might do something like this:

```C#
public string override GetSharedId() => this.MyRecord.Id;
```

Any properties with **[SharedMemory]** on it will be scoped to the value returned by this.

## Custom State

Every time a an interaction happens with your application the following process happens:

1. The **CardApp** is instantiated
2. **CardApp.LoadAppAsync(...)** is called.  The default implementation loads state according to **[Memory]** attributes 
3. **CardApp.OnActionExecutAsync(...)** is called, which dispatches to the current **CardView**, executes verbs, binds template to a card, etc.
4. **CardApp.SaveAppAsync(...)** is called. The default implementation saves state according to **[Memory]** attributes

To manage your own state from your own data base all you need to do is override **OnLoadAppAsync()** and **OnSaveAppAsync()**.  

```C#
public SomeData MyData {get;set;}

public async virtual Task LoadAppAsync(string? sharedId, string? sessionId, Activity activity, CancellationToken cancellationToken)
{
    await base.LoadAppAsync(sharedId, sessionId, activity, cancellationToken);
    
    MyData = await myDB.Load(...);
}

public async virtual Task SaveAppAsync(CancellationToken cancellationToken)
{
    await myDB.Save(...);

    await base.SaveAppAsync(cancellationToken);
}
```

>  NOTE 1: If you want **[SessionMemory]** and **[SharedMemory]** attributes to continue to work you need to call the base implementation when overriding these methods.

>  NOTE 2: You should not put **[SessionMemory]** or **[SharedMemory]** attributes on properties which are loaded and saved to external data sources as that will cause the data be persisted twice, once to the custom store and once to the IStorage key value store.

![image](https://user-images.githubusercontent.com/17789481/197365048-6a74c3d5-85cd-4c04-a07a-eef2a46e0ddf.png)
