

![image](https://user-images.githubusercontent.com/17789481/197238565-e3f895d0-6def-4d41-aba2-721d5432b1ef.png)


# Counters example walkthrough

We are going to create a data binding sample application which increments counters. It will show

* **[SharedMemory] and [SessionMemory] attributes**
* **Hooking verbs to methods**

## 1. Create Counters folder for your app in /cards

The **Cards** folder is a special folder that creates an area for your applications to live (just like Pages organizes your web pages). 

The convention is that each app is a sub-folder in the Cards folder, so we create a folder **/Cards/Counters**

## 2. Create a Default.cshtml file

Just like with HelloWorld, the CountersApp will load the **Default.cshtml** file as the initial view for the application, but this time we are going to 

* Display the value of **Counter**  property
* Add **verb handler** that increment the value of the counter.

Create **/Cards/Counters/Default.cshtml**

```xml
@inherits CardView

<Card Version="1.5">
    <TextBlock Size="ExtraLarge" Weight="Bolder">Counters</TextBlock>
    <TextBlock Size="Large">Session Counter:@Counter</TextBlock>

    <Action.Execute Title="+ Session" Verb="@nameof(OnIncrement)" />
</Card>

@functions {
    public int Counter { get; set; }

    public void OnIncrement() => Counter++;
}
```

Things to notice:

* The local property **Counter** is automatically persisted with session scope as part of the card view. 
* We have methods hooked up to the verbs which are simply the methods to call to change the properties.  

That's it.  Now run the application and go to http://localhost:{yourport}/Cards/Counters 

You should see something like this:

![image-20221115162303805](assets/image-20221115162303805.png)

As you click on it, the card is refreshing itself and updating the values.  If you copy and paste the link to another browser window you will see that the shared values are shared and the session values are per window.

# Adding a CardApp 

Now we will are going to modify the app to have a counter which is shared among all viewers of the card.

* **Counter** => Is a value which each person who interacts with the card sees.
* **SharedCounter** => will be a value that all people who interact with the card see and share.

## 1. Create a CountersApp.cs file

We are going to define a **CountersApp** class which derives from **CardApp**.  We use this class to define properties and methods which all templates in the folder have access to. Depending on the **MemoryAttribute** applied the value will automatically be persisted according to the scope policy of the memory attribute.

In this case we are going to use **[AppMemory]** attribute because it scopes the memory to all users who interact with the card.

Create  **/Cards/Counters/CountersApp.cs** and define **CountersApp**

```C#
    public class CountersApp : CardApp
    {
        public CountersApp(IServiceProvider services): base(services)
        { }

        [AppMemory]
        public int SharedCounter { get; set; } = 0;
    }
```

The values defined on the **CountersApp** class are shared by all CardView templates in the folder, and their persistence scope is defined by the attributes we put on it [(go to Memory documentation for more details)](/docs/Memory.md)

## 2. Update the Default.cshtml file to know about CountersApp

Now we will modify the Default .cshtml to interact with the **CountersApp.**

* Edit the default.cshtml to **@inherits CardView<CountersApp>** .  This tells the template that **App** property is of type **CountersApp**

* Update to bind to **App.SharedCounter** and add a verb handler to increment it.

**/Cards/Counters/Default.cshtml** should look like this

```xml
@using CrazorDemoBot.Cards.Counters
@inherits CardView<CountersApp>

<Card Version="1.5">
    <TextBlock Size="ExtraLarge" Weight="Bolder">Counters</TextBlock>
    <TextBlock Size="Large">Session Counter:@Counter</TextBlock>
    <TextBlock Size="Large">Shared Counter:@App.SharedCounter</TextBlock>
    
    <Action.Execute Title="+ Session" Verb="@nameof(OnIncrement)" />
	<Action.Execute Title="+ Shared" Verb="@nameof(OnIncrementShared)"/>
</Card>

@functions {
    public int Counter { get; set; }

    public void OnIncrement() => Counter++;
    public void OnIncrementShared() => App.SharedCounter++;
}
```

Things to notice:

* The local property **Counter** is automatically persisted with **session scope** as part of the card view. 
* The **App** property **SharedCounter** is persisted with **app scope** as part of the card application.

You should see something like this:

![image-20221103120318266](assets/image-20221103120318266.png)