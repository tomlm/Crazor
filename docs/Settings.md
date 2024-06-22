

![image](https://user-images.githubusercontent.com/17789481/197238565-e3f895d0-6def-4d41-aba2-721d5432b1ef.png)

# Crazor Settings

Settings of course are in any asp.net settings provider, typically they are in appsettings.json.

## Core Service Settings

Core service settings:

| Name                     | Required                               | Default Value                             | Example                                 | Description                                                  |
| ------------------------ | -------------------------------------- | ----------------------------------------- | --------------------------------------- | ------------------------------------------------------------ |
| **HostUri**              | Yes                                    | *none*                                    | https://crazordemobot.azurewebsites.net | The path to the web service                                  |
| **MicrosoftAppType**     | Yes                                    | *none*                                    | MultiTenant                             | One of:[MultiTenant, SingleTenant, UserManagedIdentity]      |
| **MicrosoftAppId**       | Yes                                    | *none*                                    | {GUID}                                  | AppId of your bot                                            |
| **MicrosoftAppPassword** | if **MultiTenant** or **SingleTenant** | *none*                                    | *password*                              | The AD Password for your bot. If you use **UserManagedIdentity** this is ignored. |
| **TeamsAppId**           | No                                     | MicrosoftAppId will be default if not set | {Guid}                                  | The Teams AppID registration from manifest.json              |
| **BotName**              | Yes                                    | *none*                                    | Billy Bob's Bot                         | This is used for the header of the cards                     |



## Icon settings

There are some icons that get injected into your card, and these settings allow you to point to url to use for the icons

| Name             | Required | Default Value        | Example              | Description                                                  |
| ---------------- | -------- | -------------------- | -------------------- | ------------------------------------------------------------ |
| **BotIcon**      | **Yes**  | /images/boticon.png  | /images/boticon.png  | full color image used for the bot 192x192                    |
| **OutlineIcon**  | **Yes**  | /images/outline.png  | /images/outline.png  | transparent PNG outline icon. The border color needs to be white. Size 32x32 |
| **RefreshIcon**  | No       | /images/refresh.png  | /images/refresh.png  | Image used for refresh command in menu                       |
| **OpenLinkIcon** | No       | /images/OpenLink.png | /images/OpenLink.png | image used for openlink command in menu                      |
