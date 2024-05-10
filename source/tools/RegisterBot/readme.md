RegisterBot Version [[VERSION]]

```RegisterBot [--endpoint endpoint] [--name botName] [--resource-group groupName] [--help]```

Creates or updates a bot registration for [botName] pointing to [endpoint] with teams channel and SSO enabled.
This tool will:
* Create or update the Microsoft Application for login
	* the OAuth redirect URIs
	* the App secrets
* Create or update the bot registration
    * Register teams channel
	* Enable SSO
	* Set the messaging endpoint
	* Update the bot secrets

| Argument                         | Description                                                                                   |
| -------------------------------- | --------------------------------------------------------------------------------------------- |
| -e, --endpoint endpoint          | (optional) If not specified the endpoint will stay the same as project settings               | 
| -n, --name botName               | (optional) If not specified the botname will be pulled from settings or interactively asked   |
| -g, --resource-group groupName   | (optional) If not specified the groupname will be pulled from settings or interactively asked |
| -h, --help                       | display help                                                                                  |

If the endpoint host name is:

| Host                 | Action                                                                               |
| -------------------- | ------------------------------------------------------------------------------------ |
| xx.azurewebsites.net | it modifies the remote web app settings to have correct settings/secrets             |
| xx.ngrok.xx          | it modifies the local project settings/user secrets to have correct settings/secrets |
| xx.devtunnels.ms     | it modifies the local project settings/user secrets to have correct settings/secrets |
| localhost            | it modifies the local project settings/user secrets to have correct settings/secrets |

> NOTE:
> * This tool requires Azure CLI to be installed, logged and the correct subscription to be set.
> * This needs to be run in a folder with the csproj which is hosting the bot.
> * If you have an existing AD App in your csproj it in that will be used to create the bot registration.

