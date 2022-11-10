

![image](https://user-images.githubusercontent.com/17789481/197238565-e3f895d0-6def-4d41-aba2-721d5432b1ef.png)

# Teams stuff

In the **Teams** folder there are 3 files

* **color.png** - the colorful icon that teams will display for your bot on cards
* **outline.png** - 1 transparency that will be used to pin your icon to the side channel
* **manifest.json** - the metadata about your bot that teams wants

# manifest.json 

The manifest describes all of the data teams needs to interact with your crazor based bot.

## Edit extension metadata

The extension metadata describes your bot and information about you as a developer

![image-20221103082948629](assets/image-20221103082948629.png)

1. **Set the $.id to your bot Id **
2. **update name, developer info**

## **Add your bot**

This enables your bot to receive and response to messages. 

![image-20221102145809634](assets/image-20221102145809634-1667489426324-2.png)

1. update botId with your **BotId** = ***your MicrosoftAppId***

## To add link unfurling for your cards

![image-20221104004207183](assets/image-20221104004207183.png)This enables your bot to unfurl your web site links into cards.

![image-20221102145929240](assets/image-20221102145929240.png)

1. update **BotId** =  ***your MicrosoftAppId***
2. update the **composeExtensions[].messageHandlers.domains** => domain for your web site to link unfurl
3. add your web site domain to **validDomains**



## To add a card as a Task Module

![image-20221104003856059](assets/image-20221104003856059.png)

![image-20221104004023935](assets/image-20221104004023935.png)

To add your card as a Task Module you edit the **composeExtensions** section to add **commands**

![image-20221102150248160](assets/image-20221102150248160.png)

For each card you want to surface as custom command task module:

1. Set **Id = ** ***path to your card*** (Example: "**/Cards/Addresses**")
2. Set **context** to define where the card shows up
   1. **compose** (for inserting card into edit box)
   2. **commandBox** (for starting card from command box)
   3. **message** (for starting card from context of a message)
3. Update the title, taskinfo etc. appropriately.

## To add a card as a Tab Module

![image-20221104004105515](assets/image-20221104004105515.png)You can add your card as a tab by editing the **staticTabs** section.

```json
 "staticTabs": [
	...
     {
      "entityId": "/Cards/Addresses",
      "name": "Addresses",
      "contentBotId": "26dcf7b5-ee37-4a9f-95ad-ea80feecf39e",
      "scopes": [ "personal" ]
    }
   ...
```

For each card you want to have be a tab add a section to **staticTabs** collection:

1. set **name** to the name of the tab
2. set **entityId** => ***path to your card*** (Example: "**/Cards/Addresses**")
3. set **contentBotId** => Your **MicrosoftAppId**

## To Add a CardView as a messaging query extension

![image-20221110091940559](assets/image-20221110091940559.png)

Edit team manifest to register a command **type="query"** with CommandId => route to your card.

```json
{
    "id": "/Cards/Nuget/Details",
    "type": "query",
    "description": "Search Nuget for packages",
    "title": "Nuget",
    "initialRun": true,
    "parameters": [
        {
            "name": "search",
            "description": "Enter in package name you want",
            "title": "Package"
        }
    ]
},
```

In your cardview implement **OnSearch()** method to return search results.





## 

# Side-loading your teams manifest

Zip **all 3 files** up into a .zip file and **import** into **teams** 

1. go to **Store** in teams![image-20221103094440162](assets/image-20221103094440162.png)
2. Click on **Manage your Apps**![image-20221102161013896](assets/image-20221102161013896.png)
3. Click on Upload an App![image-20221102161035140](assets/image-20221102161035140.png)
4. When it comes up, click on **ADD TO TEAMS**

You should now be able to do link unfurling and commands for your crazor based project.



![image](https://user-images.githubusercontent.com/17789481/197365048-6a74c3d5-85cd-4c04-a07a-eef2a46e0ddf.png)
