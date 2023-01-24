

```RegisterBot --resource-group [groupName] --name [botName] -endpoint [endpoint]```



Creates a bot registration for [botName] pointing to [endpoint] with teams channel enabled.



RegisterBot will automatically update the MicrosoftAppId/MicrosoftAppPassword settings for your bot web service based on the host.

| Host              | Action                                                                          |
| ----------------- | ------------------------------------------------------------------------------- |
| azurewebsites.net | modify the remote web app settings to have correct settings/secrets             |
| ngrok.io          | modify the local project settings/user secrets to have correct settings/secrets |