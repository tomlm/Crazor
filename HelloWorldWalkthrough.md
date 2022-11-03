

![image](https://user-images.githubusercontent.com/17789481/197238565-e3f895d0-6def-4d41-aba2-721d5432b1ef.png)


# Hello World walkthrough

We are going to create a HelloWorld card application.

## 1. Create a folder for your app

The **Cards** folder is a special folder that creates an area for your applications to live (just like Pages organizes your web pages). 

The convention is that each app is a sub-folder in the Cards folder, so we create a folder **/Cards/HelloWorld**

## 2. Create a HelloWorldApp.cs file

Create  **/Cards/HelloWorld/HelloWorldApp.cs** and define **HelloWorldApp**

```C#
    public class HelloWorldApp : CardApp
    {
        public HelloWorldApp(IServiceProvider services) : base(services)
        {}
    }
```

## 3. Create a Default.cshtml file

By default the HelloWorldApp will load the **Default.cshtml** file as the initial view for the application. 

Create **/Cards/HelloWorld/Default.cshtml**

```xml
@using CrazorDemoBot.Cards.HelloWorld
@inherits CardView<HelloWorldApp>

<Card Version="1.5">
    <TextBlock>Hello world!</TextBlock>
</Card>
```

That's it.  Now run the application and go to http://localhost:{yourport}/Cards/HelloWorld 

You should see like this:

![image-20221103115603471](assets/image-20221103115603471.png)

