#r "nuget: Newtonsoft.Json, 13.0.1"
#r "nuget: MedallionShell, 1.6.2"
#r "nuget: CShell, 1.2.4"

using System.IO;
using System.Threading.Tasks;
using CShellNet;
using Medallion.Shell;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

await new Script().Main(Args);

// goto https://github.com/tomlm/CShell/blob/master/README.md for documentation

// NOTE: To make this debugabble in visual studio code, simply run 
//      dotnet script init 
// command in this folder.  This will create launch.json configuration so you can set breakpoints and step through the script

class Script : CShell
{
    public async Task Main(IList<string> args)
    {
        dynamic output = await Cmd("az account show").AsJson();
        Console.WriteLine("Register bot. NOTE: Run this in same directory as .csproj");
        Console.WriteLine();
        Console.WriteLine($"==== {output.name} =====");

        Console.WriteLine("What is the name of your bot?");
        var botName = Console.ReadLine().Trim();

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

        Console.WriteLine("What resource group do you want to use?");
        output = await Cmd("az group list").AsJson();
        Console.WriteLine("0. ** Create new group");
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
        Console.WriteLine($"Group is: {groupName}");

        Console.WriteLine($"==== Creating AD Application for {botName}");

        output = await Cmd($"az ad app create --display-name {botName}").AsJson();
        string appId = (string)output.appId;

        Console.WriteLine("==== Generating MicrosoftAppPassword");
        output = await Cmd($"az ad app credential reset --id {appId}").AsJson();
        string password = (string)output.password;
        string tenantId = (string)output.tenant;

        Console.WriteLine($"==== Creating bot registration for {botName}");
        output = await Cmd($"az bot create --resource-group {groupName} --appid {appId} --kind registration --name {botName} --endpoint {uri.AbsoluteUri} --password {password}").AsJson();
        output = await Cmd($"az bot msteams create --resource-group {groupName} --name {botName}").AsJson();
        //         output = await Cmd($"az bot msteams create --resource-group {groupName} --name {botName}").AsJson();
        Console.WriteLine("Settings information for bot registration:");
        dynamic result = new JObject();
        result.BotName = botName;
        result.MicrosoftAppType = "MultiTenant";
        result.MicrosoftAppId = appId;
        result.MicrosoftAppPassword = password;
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
            output = await Cmd($"az webapp config appsettings set --resource-group {groupName} --name {webAppName} --settings {sb.ToString()}").AsJson();
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

            await Cmd($"dotnet user-secrets set MicrosoftAppPassword \"{password}\"").AsString();
        }

        Console.WriteLine();
        Console.WriteLine($"{botName} - {appId} successfully created for {uri.AbsoluteUri}!");
    }

    public async Task<string> CreateGroup(string botName)
    {
        Console.WriteLine($"What region do you want for your resource group {botName}?");
        var region = Console.ReadLine().Trim();

        Console.WriteLine($"==== Creating resource group in {region}");
        dynamic output = await Cmd($"az group create --name {botName} --location {region}").AsJson();
        return botName;
    }
}
