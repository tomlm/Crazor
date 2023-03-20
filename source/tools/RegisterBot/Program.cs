// See https://aka.ms/new-console-template for more information
using System.Text;
using CShellNet;
using Newtonsoft.Json.Linq;


// goto https://github.com/tomlm/CShell/blob/master/README.md for documentation

// NOTE: To make this debugabble in visual studio code, simply run 
//      dotnet script init 
// command in this folder.  This will create launch.json configuration so you can set breakpoints and step through the script

class Script : CShell
{
    static async Task Main(string[] args)
    {
        await new Script().Main(args);
    }

    public async Task Main(IList<string> args)
    {
        ThrowOnError = false;

        if (args.Any(a => a == "--help" || a == "-h"))
        {
            Console.WriteLine("RegisterBot --resource-group [groupName] --name [botName] -endpoint [endpoint]");
            Console.WriteLine();
            Console.WriteLine("Creates a bot registration for [botName] pointing to [endpoint] with teams channel enabled.");
            Console.WriteLine();
            Console.WriteLine("If the endpoint host is:");
            Console.WriteLine("| Host              | Action                                                                          |");
            Console.WriteLine("| ----------------- | ------------------------------------------------------------------------------- |");
            Console.WriteLine("| azurewebsites.net | modify the remote web app settings to have correct settings/secrets             |");
            Console.WriteLine("| ngrok.io          | modify the local project settings/user secrets to have correct settings/secrets |");
            return;
        }
        Echo = false;
        dynamic output = await Cmd("az account show").AsJson();
        Console.WriteLine($"==== Subscription: {output.name} =====");

        string groupName = args.SkipWhile(arg => arg != "--resource-group").Skip(1).FirstOrDefault();
        string botName = args.SkipWhile(arg => arg != "--name").Skip(1).FirstOrDefault();
        string endpoint = args.SkipWhile(arg => arg != "--endpoint").Skip(1).FirstOrDefault();
        botName = !String.IsNullOrEmpty(botName) ? botName : await GetBotName();
        endpoint = !String.IsNullOrEmpty(endpoint) ? endpoint : await GetEndpoint();
        groupName = !String.IsNullOrEmpty(groupName) ? groupName : await GetGroupName(botName);

        Uri uri = new Uri(endpoint);
        // validate groupname exists
        var commandResult = await Cmd($"az group show --resource-group {groupName}").AsResult();
        if (!commandResult.Success)
        {
            Console.WriteLine(commandResult.StandardError);
            Console.WriteLine(commandResult.StandardOutput);
            return;
        }

        string appId = null;
        output = await Cmd($"az ad app list --show-mine").AsJson();
        foreach (var item in output)
        {
            if (item.displayName == botName)
            {
                appId = item.appId;
                break;
            }
        }

        Echo = true;
        if (appId == null)
        {
            Console.WriteLine($"==== Creating AD Application for {botName}");
            output = await Cmd($"az ad app create --display-name {botName}").AsJson();
            appId = (string)output.appId;

        }
        
        Console.WriteLine("==== Generating MicrosoftAppPassword");
        output = await Cmd($"az ad app credential reset --id {appId}").AsJson();
        string password = (string)output.password;
        string tenantId = (string)output.tenant;

        Console.WriteLine($"===== Configuring SSO for {botName}");
        // output = await Cmd($"az ad app update --id {appId} --identifier-uris api://{endpoint.ToLower()}/BotId-{appId}").AsJson();
        output = await Cmd($"az ad app update --id {appId} --identifier-uris api://{uri.Host.ToLower()}/botid-{appId}").AsJson();

        output = await Cmd($"az ad app permission add --api {appId} --api-permissions 00000000-0000-0000-0000-00000000000000000=Role --id 00000000-0000-0000-0000-00000000000000000").AsJson();

        Console.WriteLine($"==== looking up {botName}");
        var cmd = await Cmd($"az bot show -g {groupName} --name {botName}").AsResult();
        if (!cmd.Success)
        {
            Console.WriteLine($"==== Creating bot registration for {botName}");
            // output = await Cmd($"az bot create --resource-group {groupName} --appid {appId} --kind registration --name {botName} --endpoint {uri.AbsoluteUri} --password {password} --app-type MultiTenant").AsJson();
            output = await Cmd($"az bot create --resource-group {groupName} --appid {appId} --name {botName} --endpoint {uri.AbsoluteUri} --app-type MultiTenant").AsJson();
            output = await Cmd($"az bot msteams create --resource-group {groupName} --name {botName}").AsJson();
        }
        else
        {
            output = cmd.AsJson();
            if (output.properties.endpoint != uri.AbsoluteUri)
            {
                Console.WriteLine($"==== Updating bot endpoint for {botName}");
                cmd = await Cmd($"az bot update --resource-group {groupName} --name {botName} --endpoint {uri.AbsoluteUri}").Execute();
            }
        }

        Console.WriteLine("Settings:");
        dynamic result = new JObject();
        result.BotName = botName;
        result.HostUri = uri.AbsoluteUri;
        result.MicrosoftAppType = "MultiTenant";
        result.MicrosoftAppId = appId;
        result.MicrosoftAppPassword = password;
        result.TeamsAppId = appId;
        Console.WriteLine(result.ToString());

        if (uri.Host.EndsWith("azurewebsites.net"))
        {
            Console.WriteLine($"==== Updating {uri.Host} settings");
            var webAppName = uri.Host.Split('.').First();
            StringBuilder sb = new StringBuilder();
            sb.Append($"BotName={botName} ");
            sb.Append($"HostUri={new Uri(uri, "/").AbsoluteUri} ");
            sb.Append($"MicrosoftAppType=MultiTenant ");
            sb.Append($"MicrosoftAppId={appId} ");
            sb.Append($"MicrosoftAppPassword={password} ");
            sb.Append($"TeamsAppId={appId} ");
            output = await Cmd($"az webapp config appsettings set --resource-group {groupName} --name {webAppName} --settings {sb}").AsJson();
        }
        else if (uri.Host.EndsWith("ngrok.io") || uri.Host == "localhost")
        {
            Console.WriteLine($"==== Updating appsettings.Development.json");
            dynamic settings = JObject.Parse(File.ReadAllText(@"appsettings.Development.json"));
            settings.BotName = botName;
            settings.HostUri = new Uri(uri, "/").AbsoluteUri;
            settings.MicrosoftAppType = "MultiTenant";
            settings.MicrosoftAppId = appId;
            settings.TeamsAppId = appId;
            File.WriteAllText("appsettings.Development.json", ((JObject)settings).ToString());

            await Cmd($"dotnet user-secrets set MicrosoftAppPassword {password}").AsString();
        }

        Console.WriteLine();
        Console.WriteLine($"{botName} - {appId} successfully configured for {uri.AbsoluteUri}!");
    }

    private async Task<string> GetBotName()
    {
        await Task.CompletedTask;

        Console.WriteLine("What is the name of your bot?");
        var botName = Console.ReadLine().Trim();
        return botName;
    }

    public async Task<string> GetEndpoint()
    {
        await Task.CompletedTask;

        string endpoint;
        Uri uri;
        while (true)
        {
            Console.WriteLine("What is the endpoint for the bot?");
            endpoint = Console.ReadLine().Trim();
            uri = new Uri(endpoint);
            if (uri.Host == "localhost")
            {
                Console.WriteLine("You can't register a localhost endpoint.");
            }
            else
            {
                break;
            }
        }

        if (String.IsNullOrEmpty(uri.AbsolutePath) || uri.AbsolutePath == "/")
        {
            Console.WriteLine("What endpoint are you using?");
            Console.WriteLine("0. /api/messages");
            Console.WriteLine("1. /api/cardapps (Crazor hosted bot)");
            var answer = int.Parse(Console.ReadLine().Trim());
            if (answer == 0)
            {
                uri = new Uri(uri, "/api/messages");
            }
            else if (answer == 1)
            {
                uri = new Uri(uri, "/api/cardapps");
            }
        }

        return uri.AbsoluteUri;
    }

    public async Task<string> CreateGroup(string botName)
    {
        Console.WriteLine($"What region do you want for your resource group {botName}?");
        var region = Console.ReadLine().Trim();

        Console.WriteLine($"==== Creating resource group in {region}");
        dynamic output = await Cmd($"az group create --name {botName} --location {region}").AsJson();
        return botName;
    }

    public async Task<string> GetGroupName(string botName)
    {
        dynamic output = await Cmd("az group list").AsJson();
        Console.WriteLine("What resource group do you want to use?");
        Console.WriteLine($"0. ** Create new group [{botName}]");
        int i = 1;
        foreach (var group in output)
        {
            Console.WriteLine($"{i++}. {group.name}");
        }

        string groupName;
        Console.WriteLine("What group do you want the bot registered in?");
        string groupAnswer = Console.ReadLine().Trim();
        i = int.Parse(groupAnswer);
        if (i == 0)
        {
            groupName = await CreateGroup(botName);
        }
        else
        {
            groupName = output[--i].name;
        }
        return groupName;
    }
}
