﻿// See https://aka.ms/new-console-template for more information
using CShellNet;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Text;

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
            DisplayHelp();
            return;
        }

        Echo = false;
        var cmdResult = await Cmd("az -h").Execute();
        if (!cmdResult.Success)
        {
            Console.WriteLine("==== You need to install Azure CLI to use RegisterBot!!!!");
            throw new CommandResultException(cmdResult);
        }

        string endpoint = args.SkipWhile(arg => arg != "--endpoint" && arg != "-e").Skip(1).FirstOrDefault();

        IConfigurationRoot configuration = await GetConfiguration(endpoint);

        dynamic output = await Cmd("az account show").AsJson();
        string tenantId = output.tenantId;
        Console.WriteLine($"\n==== Subscription: {output.name} tenantId: {tenantId} =====");

        string botName = args.SkipWhile(arg => arg != "--name" && arg != "-n").Skip(1).FirstOrDefault()
            ?? configuration.GetValue<string>("BotName")
            ?? await GetBotName();

        string groupName = args.SkipWhile(arg => arg != "--resource-group" && arg != "-g").Skip(1).FirstOrDefault()
            ?? configuration.GetValue<string>("resource-group")
            ?? await GetGroupName(botName);

        endpoint = endpoint
            ?? configuration.GetValue<string>("HostUri")
            ?? await GetEndpoint();

        string appId = args.SkipWhile(arg => arg.ToLower() != "--appid").Skip(1).FirstOrDefault() ??
                configuration.GetValue<String>("MicrosoftAppId") ??
                configuration.GetValue<string>("AzureAd:ClientId");
        
        Uri uri = new Uri(endpoint);
        if (uri.AbsolutePath == "/")
        {
            uri = new Uri(uri, "/api/cardapps");
        }

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
            if (oauth2.value == "access_as_user")
            {
                oauth2PermissionScope = oauth2;
                break;
            }
        }

        if (oauth2PermissionScope == null)
        {
            oauth2PermissionScope = JObject.FromObject(new
            {
                adminConsentDescription = "Office can call the appplication API as the current user.",
                adminConsentDisplayName = "Office can access user profile",
                id = Guid.NewGuid().ToString(),
                isEnabled = true,
                type = "User",
                userConsentDescription = "Office can call this application API with the same rights as the user.",
                userConsentDisplayName = "Office can access the user profile and make requests on behalf of the user. ",
                value = "access_as_user"
            });
            oauth2PermissionScopes.Add(oauth2PermissionScope);
            var payload = new { api = new { oauth2PermissionScopes = oauth2PermissionScopes } };
            output = await AzRest("patch", $"https://graph.microsoft.com/beta/applications/{application.id}", payload);
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
            output = await AzRest("patch", $"https://graph.microsoft.com/beta/applications/{application.id}", payload);
        }

        // ===== Configuring redirectUriSettings
        Console.WriteLine("\n===== Configuring redirectUriSettings");

        var redirectUris = new HashSet<string>(((JArray)application.web.redirectUris).Select(el => el.ToString()).Cast<string>());

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

        //if (authSetting != null)
        //{
        //    await Cmd($"az bot authsetting delete -g {groupName} --name {botName} --setting-name Default").Execute();
        //}

        output = await Cmd($"az bot authsetting create -g {groupName} --name {botName} --setting-name Default --client-id {appId} --client-secret {appPassword} --service Aadv2 --provider-scope-string User.Read,User.ReadBasic.All --parameters TenantId={tenantId} TokenExchangeUrl={appIdUri}").AsJson();

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
            List<string> settings = new List<string>();
            settings.Add($"BotName={botName}");
            settings.Add($"HostUri={new Uri(uri, "/").AbsoluteUri} ");
            settings.Add($"MicrosoftAppType=MultiTenant");
            settings.Add($"MicrosoftAppId={appId}");
            settings.Add($"TeamsAppId={appId}");
            settings.Add($"AzureAd:ClientId={appId}");
            settings.Add($"MicrosoftAppPassword={appPassword}");

            settings.Add($"AzureAD:ClientSecret={appPassword} ");
            output = await Cmd($"az webapp config appsettings set --resource-group {groupName} --name {webAppName} --settings {String.Join(' ', settings)}").AsJson();
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

    private static void DisplayHelp()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "Crazor.readme.md";

        using (Stream stream = assembly.GetManifestResourceStream(resourceName))
        using (StreamReader reader = new StreamReader(stream))
        {
            Console.WriteLine(reader.ReadToEnd());
        }
    }

    private async Task<IConfigurationRoot> GetConfiguration(string endpoint)
    {
        if (endpoint != null)
        {
            var uri = new Uri(endpoint);
            if (uri.Host.EndsWith("azurewebsites.net"))
            {
                Console.WriteLine("==== Detecting deployment settings");

                dynamic output = await Cmd($"az webapp list --query [?defaultHostName=='{uri.Host}']").AsJson();
                dynamic webApp = output[0];
                var groupName = webApp?.resourceGroup;

                output = await Cmd($"az webapp config appsettings list -g {groupName} -n {webApp.name}").AsJson();
                Dictionary<string, string> settings = new Dictionary<string, string>();
                settings["resource-group"] = groupName;
                foreach (var kv in output)
                {
                    settings[(string)kv.name] = (string)kv.value;
                }
                settings["HostUri"] = new Uri(uri, "/").AbsoluteUri;

                return new ConfigurationManager()
                    .AddInMemoryCollection(settings)
                    .Build();
            }
        }

        var csproj = dir("*.csproj").FirstOrDefault();
        string secretId = "Unknown";
        if (csproj != null)
        {
            var secret = File.ReadAllText(csproj).Split('\r', '\f', '\n').Select(t => t.Trim()).First(l => l.StartsWith("<UserSecretsId>"));
            var iStart = secret.IndexOf('>') + 1;
            var iEnd = secret.IndexOf('<', iStart);
            secretId = secret.Substring(iStart, iEnd - iStart);
        }

        return new ConfigurationManager()
            .AddJsonFile(Path.Combine(CurrentFolder.FullName, "appsettings.json"))
            .AddJsonFile(Path.Combine(CurrentFolder.FullName, "appsettings.development.json"))
            .AddUserSecrets(secretId)
            .AddEnvironmentVariables()
            .Build();
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

    public async Task<dynamic> AzRest(string verb, string uri, object payload)
    {
        var body = JsonConvert.SerializeObject(payload, Formatting.None);
        File.WriteAllText("body.json", body);
        var cmdText = $"az rest --method {verb} --uri {uri} --headers Content-Type=application/json --body @body.json";
        var result = await Cmd(cmdText).AsJson();
        File.Delete("body.json"); 
        return result;
    }
}
