

![image](https://user-images.githubusercontent.com/17789481/197238565-e3f895d0-6def-4d41-aba2-721d5432b1ef.png)

# Conceptual Model

The web services hosts multiple **Card applications**.  Each card application represents a micro-app experience that can be used independently and is made up of a views that are defined using Adaptive Cards.

## Card Applications

Your service can host 1:N **Card Applications**.  A card application is a mini application which is based on AdaptiveCards, using Razor as the templating engine. 

## Card Views

Each card application is made up of 1:N **Card Views**. A card view is a razor template, binding the data and logic to create a "screen" in the application. 

# The CardApp Class

The **CardApp** class manages the application lifecycle for a card application. 

The **CardApp** class represents your application state and methods.  Any properties and methods you put onto the app class are accessible from all **CardView** objects in the folder via the **App** property.

To define an application you simply derive a class from **CardApp** and put it in the **Cards/{Name}** folder.

## Properties

Properties on the **CardApp** are available to all **CardView** in the application.

To load and persist the values of any property you have 2 mechanisms:

* Use **[SharedMemory]** and **[SessionMemory]** attributes to automatically persist to a key value store 
* You override **LoadAppAsync()** and **SaveAppAsync()**

> See [Memory](Memory) for more details 

## Methods

You can define any methods you like on the **CardApp**.

A useful pattern is to put methods which manipulate your shared state on the app so that you can consolidate data manipulation methods for all **CardView** classes. For example, you can add CRUD methods which manipulate your data to the **CardApp**, and call **App.Create(...)** from a view to manipulate your data directly.

## CardApp Naming Convention

 **CardApp** names have a naming convention with the name of the **folder**. The name of the **folder name** is what shows up in the urls, and the name of the class is the name of the folder + App like this: **"{*name of the folder*}App"**

* Cards
  * Foobar
    * FoobarApp.cs



> 



![image](https://user-images.githubusercontent.com/17789481/197365048-6a74c3d5-85cd-4c04-a07a-eef2a46e0ddf.png)