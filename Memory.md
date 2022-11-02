

![image](https://user-images.githubusercontent.com/17789481/197238565-e3f895d0-6def-4d41-aba2-721d5432b1ef.png)


# State 
Cards are inherently about both 1:1 interactions with a user as well as shared interactions among a group of users. 

Crazor makes it easy to manage the state of your application's memory around these 2 scopes. 

The **CardApp** class represents your application state and is accessible from any **CardView**. 

Data on the CardApp and CardView classes which are marked with **memory scope attributes** are automatically persisted for you between each round-trip with the server, saving you from the tedium of managing loading and saving properties into a key value store.

## Memory Scope attributes

Any property which has **[SessionMemory]** or **[SharedMemory]** will automatically be persisted in the KeyValue IStorage provider.

* **[SharedMemory]** - The values for these properties are persisted and the same (shared) for all users interacting with the card.
* **[SessionMemory]** - The values for these properties are scoped only to the user interacting with a card in the app they are using.

> NOTE: 
>
> The **[SharedMemory]** attribute can be used on **CardApp** class properties only.
>
> The **[SessionMemory]** attribute can be used on **CardApp** and **CardView** class properties



## Defining the SharedMemory Id

The id which is used to key off of for the **[SharedMemory]** is called the **SharedId**.  The default value for the sharedId is the name of the application, but you can override it by implementing the **GetSharedId()** method.

For example, if you want your shared memory to associated with the id of the record your card is bound to, you might do something like this:

```C#
public string override GetSharedId() => this.MyRecord.Id;
```

This will have the effect that properties with **[SharedMemory]** on it will automatically be scoped the to record the app is bound to.

## Custom State

Every time a an interaction happens with your application the following process happens:

1. The **CardApp** is instantiated
2. **CardApp.LoadAppAsync(...)** is called.  The default implementation loads state using **[Memory]** attributes 
3. The action is processed via **CardApp.OnActionExecutAsync(...)** is called, which dispatches to the current **CardView**
4. **CardApp.SaveAppAsync(...)** is called. The default implementation saves state using **[Memory]** attributes

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



> See [Counters ](https://opcardbot.azurewebsites.net/cards/Counters/39983982398) for an example of state attributes
>
> See [Counters source](https://github.com/microsoft/crazor/tree/main/source/samples/OpBot/Cards/Counters) for source code



