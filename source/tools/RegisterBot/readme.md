# RegisterBot 
```RegisterBot [--endpoint endpoint] [--name botName] [--resource-group groupName] [--help]```

Creates or updates a bot registration for [botName] pointing to [endpoint] with teams channel and SSO enabled.

# Arguments:
| Argument                         | Description                                                                                   |
| -------------------------------- | --------------------------------------------------------------------------------------------- |
| -e, --endpoint endpoint          | (optional) If not specified the endpoint will stay the same as project settings               | 
| -n, --name botName               | (optional) If not specified the botname will be pulled from settings or interactively asked   |
| -g, --resource-group groupName   | (optional) If not specified the groupname will be pulled from settings or interactively asked |

> NOTE:
> * This tool requires Azure CLI to be installed, logged and the correct subscription to be set.
> * This needs to be run in a folder with a csproj.
> * If you have an existing AD App in your csproj it in that will be used to create the bot registration.

If the endpoint host name is:

| Host                 | Action                                                                               |
| -------------------- | ------------------------------------------------------------------------------------ |
| xx.azurewebsites.net | it modifies the remote web app settings to have correct settings/secrets             |
| xx.ngrok.io          | it modifies the local project settings/user secrets to have correct settings/secrets |

