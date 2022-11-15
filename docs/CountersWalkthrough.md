

![image](https://user-images.githubusercontent.com/17789481/197238565-e3f895d0-6def-4d41-aba2-721d5432b1ef.png)


# Counters example walkthrough

We are going to create a data binding sample application which increments counters. It will show

* **[SharedMemory] and [SessionMemory] attributes**
* **Hooking verbs to methods**

## 1. Create Counters folder for your app in /cards

The **Cards** folder is a special folder that creates an area for your applications to live (just like Pages organizes your web pages). 

The convention is that each app is a sub-folder in the Cards folder, so we create a folder **/Cards/Counters**

## 2. Create a CountersApp.cs file

On this step we are going to define a **CountersApp** class.  

Create  **/Cards/Counters/CountersApp.cs** and define **CountersApp**

```C#
    public class CountersApp : CardApp
    {
        public CountersApp(IServiceProvider services): base(services)
        { }

        [SharedMemory]
        public int SharedCounter { get; set; } = 0;
    }
```

You can see that we have 

* defined a **SharedCounter** property and placed a **[SharedMemory]** attribute on it.  

The values defined on the **CountersApp** class are shared by all CardView templates in the folder, and their persistence scope is defined by the attributes we put on it [(go to Memory documentation for more details)](/docs/Memory.md)

## 3. Create a Default.cshtml file

Just like with HelloWorld, the CountersApp will load the **Default.cshtml** file as the initial view for the application, but this time we are going to 

* Display the value of **App.SharedCounter**  and **App.SessionCounter**
* Add actions that increment the value of the counter.

You will notice that we use **@inherits CardView<CountersApp>**.  This makes our CardView expose a **App** property as a strongly typed property so we get intellisense over the properties we defined on the class, namely the **SharedCounter** and **SessionCounter** properties.

Create **/Cards/Counters/Default.cshtml**

```xml
@using CrazorDemoBot.Cards.Counters
@inherits CardView<CountersApp>

<Card Version="1.5">
    <TextBlock Size="ExtraLarge" Weight="Bolder">Counters</TextBlock>
    <TextBlock Size="Large">Shared Counter:@App.SharedCounter</TextBlock>
    <TextBlock Size="Large">Session Counter:@Counter</TextBlock>
    
    <Action.Execute Title="+ Shared" Verb="@nameof(OnIncrementShared)" />
    <Action.Execute Title="+ Session" Verb="@nameof(OnIncrementLocal)" />
</Card>

@functions {
    public int Counter { get; set; }

    public void OnIncrementLocal() => Counter++;
    public void OnIncrementShared() => App.SharedCounter++;
}
```

Things to notice:

* The local property **Counter** is automatically persisted with session scope as part of the card view. *All properties on a CardView are persisted as session scope, unless you opt out by using **[TempMemory]** attribute*
* We have methods hooked up to the verbs which are simply the methods to call to change the properties.  

That's it.  Now run the application and go to http://localhost:{yourport}/Cards/Counters 

You should see something like this:

![image-20221103120318266](assets/image-20221103120318266.png)

As you click on it, the card is refreshing itself and updating the values.  If you copy and paste the link to another browser window you will see that the shared values are shared and the session values are per window.