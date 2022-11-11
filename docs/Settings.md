

![image](https://user-images.githubusercontent.com/17789481/197238565-e3f895d0-6def-4d41-aba2-721d5432b1ef.png)

# Crazor Settings

Here are settings **Crazor** relies on:

| Name                     | Required                       | Default Value                             | Example                                 | Description                                     |
| ------------------------ | ------------------------------ | ----------------------------------------- | --------------------------------------- | ----------------------------------------------- |
| **HostUri**              | Yes                            | *none*                                    | https://crazordemobot.azurewebsites.net | The path to the web page service                |
| **MicrosoftAppType**     | Yes                            | *none*                                    | MultiTenant                             | One of:[MultiTenant                             |
| **MicrosoftAppId**       | Yes                            | *none*                                    | {GUID}                                  | AppId of your bot                               |
| **MicrosoftAppPassword** | if MultiTenant or SingleTenant | *none*                                    | *password*                              | The AD Password for your bot                    |
| **TeamsAppId**           | No                             | MicrosoftAppId will be default if not set | {Guid}                                  | The Teams AppID registration from manfiest.json |
| **BotName**              | Yes                            | *none*                                    | Billy Bob's Bot                         | This is used for the header of the cards        |
| **BotIcon**              | Yes                            | /images/boticon.png                       | /images/boticon.png                     | image used for the bot                          |
| **AboutIcon**            | No                             | /images/about.png                         | /images/about.png                       | image used for about command in menu            |
| **SettingsIcon**         | No                             | /images/settings.png                      | /images/settings.png                    | image used for settings command in menu         |
| **RefreshIcon**          | No                             | /images/refresh.png                       | /images/refresh.png                     | Image used for refresh command in menu          |
| **OpenLinkIcon**         | No                             | /images/OpenLink.png                      | /images/OpenLink.png                    | image used for openlink command in menu         |

