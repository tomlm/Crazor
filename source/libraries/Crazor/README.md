![image](https://user-images.githubusercontent.com/17789481/197238565-e3f895d0-6def-4d41-aba2-721d5432b1ef.png)


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

