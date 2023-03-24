// See https://aka.ms/new-console-template for more information
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using CShellNet;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
            Console.WriteLine("RegisterBot --resource-group [groupName] --name [botName] --endpoint [endpoint] (--appId [appId])");
            Console.WriteLine("Creates or updates a bot registration for [botName] pointing to [endpoint] with teams channel enabled.");
            Console.WriteLine();
            Console.WriteLine("NOTE:");
            Console.WriteLine("* This needs to be run in a folder with a csproj.");
            Console.WriteLine("* If you have an existing AD App in your csproj it in that will be used to create the bot registration.");
            Console.WriteLine();
            Console.WriteLine("If the endpoint host is:");
            Console.WriteLine("| Host              | Action                                                                          |");
            Console.WriteLine("| ----------------- | ------------------------------------------------------------------------------- |");
            Console.WriteLine("| azurewebsites.net | modify the remote web app settings to have correct settings/secrets             |");
            Console.WriteLine("| ngrok.io          | modify the local project settings/user secrets to have correct settings/secrets |");
            return;
        }
        IConfigurationRoot configuration = GetConfiguration();

        Echo = false;
        dynamic output = await Cmd("az account show").AsJson();
        string tenantId = output.tenantId;
        Console.WriteLine($"\n==== Subscription: {output.name} tenantId: {tenantId} =====");

        string botName = args.SkipWhile(arg => arg != "--name" && arg != "-n").Skip(1).FirstOrDefault()
            ?? configuration.GetValue<string>("BotName")
            ?? await GetBotName();

        string groupName = args.SkipWhile(arg => arg != "--resource-group" && arg != "-g").Skip(1).FirstOrDefault()
            ?? await GetGroupName(botName);

        string endpoint = args.SkipWhile(arg => arg != "--endpoint" && arg != "-e").Skip(1).FirstOrDefault()
            ?? configuration.GetValue<string>("HostUri")
            ?? await GetEndpoint();

        string appId = args.SkipWhile(arg => arg != "--appid").Skip(1).FirstOrDefault() ??
                configuration.GetValue<String>("MicrosoftAppId") ??
                configuration.GetValue<string>("AzureAd:ClientId");

        Uri uri = new Uri(new Uri(endpoint), "/api/cardapps");

        // validate groupname exists
        var commandResult = await Cmd($"az group show --resource-group {groupName}").AsResult();
        if (!commandResult.Success)
        {
            Console.WriteLine(commandResult.StandardError);
            Console.WriteLine(commandResult.StandardOutput);
            return;
        }

        Echo = true;

        if (appId == null)
        {
            Console.WriteLine($"\n==== Creating AD Application for project");
            output = await Cmd($"az ad app create --display-name {botName}").AsJson();
            // output = await Run($"dotnet msidentity --register-app --tenant-id {tenantId} --enable-id-token --json --redirect-uris http://localhost:33872/ https://localhost:44330/ https://localhost:7035/ http://localhost:5284/\r\n\r\n").AsJson();
            appId = (string)output.appId;
        }

        // ==== Setting redirect Uris for {appId};
        Console.WriteLine($"\n==== getting application for {botName}/{appId}");
        dynamic application = await Cmd($"az ad app show --id {appId}").AsJson();

        // ===== Configuring oauth2PermissionScopes
        Console.WriteLine($"\n===== configuring OAuth2PermissionScopes for {appId}");
        dynamic oauth2PermissionScopes = application.api.oauth2PermissionScopes;
        dynamic oauth2PermissionScope = null;
        foreach (dynamic oauth2 in oauth2PermissionScopes)
        {
            if (oauth2.value == "User.Read")
            {
                oauth2PermissionScope = oauth2;
                break;
            }
        }

        if (oauth2PermissionScope == null)
        {
            oauth2PermissionScope = JObject.FromObject(new
            {
                adminConsentDescription = "Office can call the app's web API as the current user.",
                adminConsentDisplayName = "Office can access user profile",
                id = Guid.NewGuid().ToString(),
                isEnabled = true,
                type = "User",
                userConsentDescription = "Office can call this app's API with the same rights as the user.",
                userConsentDisplayName = "Office can access the user profile and make requests on behalf of the user. ",
                value = "User.Read"
            });
            oauth2PermissionScopes.Add(oauth2PermissionScope);
            var payload = new { api = new { oauth2PermissionScopes = oauth2PermissionScopes } };
            var body = JsonConvert.SerializeObject(payload, Formatting.None).Replace("\"", "\\\"");
            output = await Cmd($"az rest --method patch --uri https://graph.microsoft.com/beta/applications/{application.id} --headers Content-Type=application/json --body \"{body}\"").AsJson();
        }

        // ===== Configuring preAuthorizedApplications
        Console.WriteLine("\n===== Configuring preAuthorizedApplications");
        dynamic preAuthorizedApplications = application.api.preAuthorizedApplications;
        var clientAppIds = new string[]
        {
            "bc59ab01-8403-45c6-8796-ac3ef710b3e3",
            "d3590ed6-52b3-4102-aeff-aad2292ab01c",
            "0ec893e0-5785-4de6-99da-4ed124e5296c",
            "4765445b-32c6-49b0-83e6-1d93765276ca",
            "5e3ce6c0-2b1f-4285-8d4b-75ee78787346",
        };
        var patch = false;
        foreach (var clientAppId in clientAppIds)
        {
            bool found = false;
            foreach (var preAuthorizedApplication in preAuthorizedApplications)
            {
                if (preAuthorizedApplication.appId == clientAppId)
                {
                    found = true;
                    var ids = new HashSet<string>(((JArray)preAuthorizedApplication.delegatedPermissionIds).Select(el => el.ToString()));
                    if (!ids.Contains((string)oauth2PermissionScope.id))
                    {
                        preAuthorizedApplication.delegatedPermissionIds.Add(oauth2PermissionScope.id);
                        patch = true;
                    }
                }
            }

            if (!found)
            {
                preAuthorizedApplications.Add(JObject.FromObject(new
                {
                    appId = clientAppId,
                    delegatedPermissionIds = new JArray() { oauth2PermissionScope.id }
                }));
                patch = true;
            }
        }

        if (patch)
        {
            JArray preAuthorizedApplicationsFixed = new JArray();
            foreach (var preAuthorizedApplication in preAuthorizedApplications)
            {
                preAuthorizedApplicationsFixed.Add(new JObject()
                {
                    { "appId", preAuthorizedApplication.appId },
                    // bug in API requires this to be permissionIds, not delegatedPerrmisionIds
                    { "permissionIds", preAuthorizedApplication.delegatedPermissionIds }
                });
            }

            var payload = new { api = new { preAuthorizedApplications = preAuthorizedApplicationsFixed } };
            var body = JsonConvert.SerializeObject(payload, Formatting.None).Replace("\"", "\\\"");
            output = await Cmd($"az rest --method patch --uri https://graph.microsoft.com/beta/applications/{application.id} --headers Content-Type=application/json --body \"{body}\"").AsJson();
        }

        // ===== Configuring redirectUriSettings
        Console.WriteLine("\n===== Configuring redirectUriSettings");

        var redirectUris = ((JArray)application.web.redirectUris).Where(url =>
        {
            var uri = new Uri(url.ToString());
            return uri.Host != "token.botframework.com";
        }).Select(el => el.ToString()).ToList<string>();

        redirectUris.Add($"https://{uri.Host}/signin-oidc");
        foreach (var domain in new string[] { "token.botframework.com", "europe.token.botframework.com", "unitedstates.token.botframework.com", "token.botframework.azure.us" })
        {
            redirectUris.Add($"https://{domain}/signin-oidc");
            redirectUris.Add($"https://{domain}/.auth/web/redirect");
        }

        output = await Cmd($"az ad app update --id {appId} --web-redirect-uris {String.Join(" ", redirectUris)}").AsJson();

        // Configure Application Id Uri for App
        Console.WriteLine($"\n===== configure application ID URI for {botName}/{appId}");
        var appIdUri = $"api://{uri.Host.ToLower()}/BotId-{appId}";
        output = await Cmd($"az ad app update --id {appId} --identifier-uris {appIdUri} ").AsJson();

        // Create Bot or update endpoint
        Console.WriteLine($"\n==== looking up {botName}");
        var cmd = await Cmd($"az bot show -g {groupName} --name {botName}").AsResult();
        if (!cmd.Success)
        {
            // Creating bot registration
            Console.WriteLine($"\n==== Creating bot registration for {botName}");
            // output = await Cmd($"az bot create --resource-group {groupName} --appid {appId} --kind registration --name {botName} --endpoint {uri.AbsoluteUri} --password {password} --app-type MultiTenant").AsJson();
            output = await Cmd($"az bot create --resource-group {groupName} --appid {appId} --name {botName} --endpoint {uri.AbsoluteUri} --app-type MultiTenant").AsJson();
            output = await Cmd($"az bot msteams create --resource-group {groupName} --name {botName}").AsJson();
        }
        else
        {
            // Update bot registration endpoint
            output = cmd.AsJson();
            if (output.properties.endpoint != uri.AbsoluteUri)
            {
                Console.WriteLine($"\n==== Updating bot endpoint for {botName}");
                cmd = await Cmd($"az bot update --resource-group {groupName} --name {botName} --endpoint {uri.AbsoluteUri}").Execute();
            }
        }

        // refreshing client secret
        Console.WriteLine($"\n==== Refreshing {botName}/{appId} client secret");
        output = await Cmd($"az ad app credential reset --id {appId}").AsJson();
        var appPassword = (string)output.password;

        // Update OAuth settings
        Console.WriteLine($"\n==== Updating Bot OAuth/SSO settings");
        dynamic authSettings = await Cmd($"az bot authsetting list -g {groupName} -n {botName}").AsJson();
        dynamic authSetting = null;
        foreach (var item in authSettings)
        {
            if (item.properties.name == "Default")
            {
                authSetting = item;
                break;
            }
        }

        if (authSetting != null)
        {
            await Cmd($"az bot authsetting delete -g {groupName} --name {botName} --setting-name Default").Execute();
        }

        output = await Cmd($"az bot authsetting create -g {groupName} --name {botName} --setting-name Default --client-id {appId} --client-secret {appPassword} --service Aadv2 --provider-scope-string \"User.Read,User.ReadBasic.All\" --parameters TenantId={tenantId} TokenExchangeUrl={appIdUri}").AsJson();

        // Saving settings
        Console.WriteLine($"\n==== Updating project/service settings");
        Console.WriteLine("Settings:");
        dynamic result = new JObject();
        result.BotName = botName;
        result.HostUri = uri.AbsoluteUri;
        result.MicrosoftAppType = "MultiTenant";
        result.MicrosoftAppId = appId;
        result.MicrosoftAppPassword = appPassword;
        result.TeamsAppId = appId;
        Console.WriteLine(result.ToString());

        if (uri.Host.EndsWith("azurewebsites.net"))
        {
            Console.WriteLine($"\n==== Updating {uri.Host} settings");
            var webAppName = uri.Host.Split('.').First();
            StringBuilder sb = new StringBuilder();
            output = await Cmd($"az webapp config appsettings set --resource-group {groupName} --name {webAppName} --settings BotName={botName}").AsJson();
            output = await Cmd($"az webapp config appsettings set --resource-group {groupName} --name {webAppName} --settings HostUri={new Uri(uri, "/").AbsoluteUri} ").AsJson();
            output = await Cmd($"az webapp config appsettings set --resource-group {groupName} --name {webAppName} --settings MicrosoftAppType=MultiTenant ").AsJson();
            output = await Cmd($"az webapp config appsettings set --resource-group {groupName} --name {webAppName} --settings MicrosoftAppId={appId} ").AsJson();
            output = await Cmd($"az webapp config appsettings set --resource-group {groupName} --name {webAppName} --settings TeamsAppId={appId} ").AsJson();
            output = await Cmd($"az webapp config appsettings set --resource-group {groupName} --name {webAppName} --settings AzureAd:ClientId={appId} ").AsJson();

            output = await Cmd($"az webapp config appsettings set --resource-group {groupName} --name {webAppName} --settings MicrosoftAppPassword={appPassword} ").AsJson();
            output = await Cmd($"az webapp config appsettings set --resource-group {groupName} --name {webAppName} --settings AzureAD:ClientSecret={appPassword} ").AsJson();

        }
        else if (uri.Host.EndsWith("ngrok.io") || uri.Host == "localhost")
        {
            Console.WriteLine($"\n==== Updating appsettings.Development.json");
            dynamic settings = JObject.Parse(File.ReadAllText(@"appsettings.Development.json"));
            settings.BotName = botName;
            settings.HostUri = new Uri(uri, "/").AbsoluteUri;
            settings.MicrosoftAppType = "MultiTenant";
            settings.MicrosoftAppId = appId;
            settings.TeamsAppId = appId;
            settings["AzureAd"] = new JObject() { { "ClientId", appId } };

            File.WriteAllText("appsettings.Development.json", ((JObject)settings).ToString());
            await Cmd($"dotnet user-secrets set MicrosoftAppPassword {appPassword}").AsString();
            await Cmd($"dotnet user-secrets set AzureAD:ClientSecret {appPassword}").AsString();
        }

        Console.WriteLine();
        Console.WriteLine($"{botName} - {appId} successfully configured for {uri.AbsoluteUri}!");
    }

    private IConfigurationRoot GetConfiguration()
    {
        var csproj = dir("*.csproj").FirstOrDefault();
        string secretId = "Unknown";
        if (csproj != null)
        {
            var secret = File.ReadAllText(csproj).Split('\r', '\f', '\n').Select(t => t.Trim()).First(l => l.StartsWith("<UserSecretsId>"));
            var iStart = secret.IndexOf('>') + 1;
            var iEnd = secret.IndexOf('<', iStart);
            secretId = secret.Substring(iStart, iEnd - iStart);
        }

        var cm = new ConfigurationManager()
            .AddJsonFile(Path.Combine(CurrentFolder.FullName, "appsettings.json"))
            .AddJsonFile(Path.Combine(CurrentFolder.FullName, "appsettings.development.json"))
            .AddUserSecrets(secretId)
            .AddEnvironmentVariables();
        var configuration = cm.Build();
        return configuration;
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

        Console.WriteLine($"\n==== Creating resource group in {region}");
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
