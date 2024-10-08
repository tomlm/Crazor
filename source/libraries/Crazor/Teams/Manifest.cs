﻿using Crazor.Attributes;
using Crazor.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Reflection;

namespace Crazor.Teams
{
#pragma warning disable // Disable all warnings

    public class Manifest
    {
        public Manifest(IConfiguration configuration, IRouteResolver routeResolver)
        {
            var hostUri = configuration.GetValue<Uri>("HostUri") ?? throw new ArgumentNullException("HostUri setting");
            var botId = configuration.GetValue<String>("MicrosoftAppId") ?? throw new ArgumentNullException("MicrosoftAppId");
            Id = botId;
            if (hostUri.Host != "localhost")
                PackageName = String.Join(".", hostUri.Host.Split('.').Reverse());
            else
                PackageName = String.Join(".", AppDomain.CurrentDomain.FriendlyName.Split('.').Reverse());
            ValidDomains = new List<string>
            {
                hostUri.Host,
                "token.botframework.com"
            };
            Developer.Name = "TBD";
            Developer.WebsiteUrl = hostUri.AbsoluteUri;
            Developer.PrivacyUrl = new Uri(hostUri, "/Privacy").AbsoluteUri;
            Developer.TermsOfUseUrl = new Uri(hostUri, "/TermsOfUse").AbsoluteUri;
            Name.Full = configuration.GetValue<string>("BotName");
            Name.Short = configuration.GetValue<string>("BotName");
            Description.Short = configuration.GetValue<string>("BotName");
            Description.Full = $"{configuration.GetValue<string>("BotName")} - {configuration.GetValue<Uri>("HostUri").Host}";
            Icons.Color = Path.GetFileName(configuration.GetValue<string>("BotIcon")?.ToLower() ?? "boticon.png");
            Icons.Outline = Path.GetFileName(configuration.GetValue<string>("OutlineIcon")?.ToLower() ?? "outline.png");
            AccentColor = "#FFFFFF";
            Bots.Add(new Bot()
            {
                BotId = botId,
                Scopes = new List<BotScope>() { BotScope.Personal, BotScope.Groupchat, BotScope.Team },

            });

            WebApplicationInfo = new WebApplicationInfo()
            {
                Id = botId,
                Resource = $"api://{hostUri.Host.ToLower()}/botid-{botId}"
            };

            StaticTabs.Add(new StaticTab()
            {
                EntityId = "about",
                Scopes = new List<StaticTabScope>() { StaticTabScope.Personal }
            });

            ComposeExtensions.Add(new ComposeExtension()
            {
                BotId = botId,
                MessageHandlers = new List<MessageHandler>()
                {
                    // link unfurler registration for domain
                    new MessageHandler()
                    {
                        Type = MessageHandlerType.Link,
                        Value = new Value()
                        {
                            Domains = new List<string>() { hostUri.Host },
                        }
                    }
                }
            });

            foreach (var cardViewType in Utils.GetAssemblies().SelectMany(asm => asm.GetTypes().Where(t => t.IsAbstract == false && t.IsAssignableTo(typeof(ICardView)))))
            {
                // get route for cardtype
                if (routeResolver.GetRouteForCardViewType(cardViewType, out var entityId))
                {
                    // add commands
                    var taskInfoAttribute = cardViewType.GetCustomAttribute<TaskInfoAttribute>();
                    var commandInfoAttribute = cardViewType.GetCustomAttribute<CommandInfoAttribute>();
                    if (commandInfoAttribute != null)
                    {
                        if (entityId.Contains("{"))
                        {
                            throw new ArgumentOutOfRangeException($"You cannot define a command on a CardView with a dynamic route {entityId}.  The route must be static.");
                        }
                        List<Parameter> parameters = null;

                        if (commandInfoAttribute.Type == CommandType.Query)
                        {
                            var teamsParameterAttributes = cardViewType.GetCustomAttributes<QueryParameterAttribute>();
                            if (teamsParameterAttributes != null)
                            {
                                parameters = new List<Parameter>();

                                foreach (var attr in teamsParameterAttributes)
                                {
                                    parameters.Add(new Parameter()
                                    {
                                        Name = attr.Name,
                                        Title = attr.Description,
                                        Description = attr.Description,
                                        InputType = attr.InputType,
                                        Value = attr.Value,
                                    });
                                }
                            }
                        }

                        ComposeExtensions[0].Commands.Add(new Command()
                        {
                            Id = entityId,
                            Type = commandInfoAttribute.Type,
                            Title = commandInfoAttribute.Title,
                            Context = commandInfoAttribute.Context?.Split(',').Select(t => Enum.Parse<CommandContext>(t.Trim(), ignoreCase: true)).ToList(),
                            Description = commandInfoAttribute.Description,
                            FetchTask = true,
                            InitialRun = true,
                            TaskInfo = new TaskInfo()
                            {
                                Title = taskInfoAttribute?.Title ?? commandInfoAttribute.Title,
                                Width = taskInfoAttribute?.Width ?? "medium",
                                Height = taskInfoAttribute?.Height ?? "medium"
                            },
                            Parameters = parameters
                        });
                    }

                }
            }
        }

        [JsonProperty("$schema", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Uri Schema { get; set; } = new Uri("https://developer.microsoft.com/en-us/json-schemas/teams/v1.14/MicrosoftTeams.schema.json");

        /// <summary>
        /// The version of the schema this manifest is using. This schema version supports extending Teams apps to other parts of the Microsoft 365 ecosystem. More info at https://aka.ms/extendteamsapps.
        /// </summary>
        [JsonProperty("manifestVersion", Required = Required.Always)]
        public string ManifestVersion { get; set; } = "1.14";

        /// <summary>
        /// The version of the app. Changes to your manifest should cause a version change. This version string must follow the semver standard (http://semver.org).
        /// </summary>
        [JsonProperty("version", Required = Required.Always)]
        public string Version { get; set; } = "1.0";

        /// <summary>
        /// A unique identifier for this app. This id must be a GUID.
        /// </summary>
        [JsonProperty("id", Required = Required.Always)]
        public string Id { get; set; }

        /// <summary>
        /// A unique identifier for this app in reverse domain notation. E.g: com.example.myapp
        /// </summary>
        [JsonProperty("packageName", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string PackageName { get; set; }

        [JsonProperty("localizationInfo", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public LocalizationInfo LocalizationInfo { get; set; }

        [JsonProperty("developer", Required = Required.Always)]
        public AppDeveloper Developer { get; set; } = new AppDeveloper();

        [JsonProperty("name", Required = Required.Always)]
        public AppName Name { get; set; } = new AppName();

        [JsonProperty("description", Required = Required.Always)]
        public AppDescription Description { get; set; } = new AppDescription();

        [JsonProperty("icons", Required = Required.Always)]
        public Icons Icons { get; set; } = new Icons();

        /// <summary>
        /// A color to use in conjunction with the icon. The value must be a valid HTML color code starting with '#', for example `#4464ee`.
        /// </summary>
        [JsonProperty("accentColor", Required = Required.Always)]
        public string AccentColor { get; set; } = "#FFFFFF";

        /// <summary>
        /// These are tabs users can optionally add to their channels and 1:1 or group chats and require extra configuration before they are added. Configurable tabs are not supported in the personal scope. Currently only one configurable tab per app is supported.
        /// </summary>
        [JsonProperty("configurableTabs", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<ConfigurableTab> ConfigurableTabs { get; set; }

        /// <summary>
        /// A set of tabs that may be 'pinned' by default, without the user adding them manually. Static tabs declared in personal scope are always pinned to the app's personal experience. Static tabs do not currently support the 'teams' scope.
        /// </summary>
        [JsonProperty("staticTabs", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<StaticTab> StaticTabs { get; set; } = new List<StaticTab>();

        /// <summary>
        /// The set of bots for this app. Currently only one bot per app is supported.
        /// </summary>
        [JsonProperty("bots", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<Bot> Bots { get; set; } = new List<Bot>();

        /// <summary>
        /// The set of Office365 connectors for this app. Currently only one connector per app is supported.
        /// </summary>
        [JsonProperty("connectors", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<Connector> Connectors { get; set; } = new List<Connector>();

        /// <summary>
        /// Subscription offer associated with this app.
        /// </summary>
        [JsonProperty("subscriptionOffer", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public SubscriptionOffer SubscriptionOffer { get; set; }

        /// <summary>
        /// The set of compose extensions for this app. Currently only one compose extension per app is supported.
        /// </summary>
        [JsonProperty("composeExtensions", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<ComposeExtension> ComposeExtensions { get; set; } = new List<ComposeExtension>();

        /// <summary>
        /// Specifies the permissions the app requests from users.
        /// </summary>
        [JsonProperty("permissions", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<Permission> Permissions { get; set; } = new List<Permission>();

        /// <summary>
        /// Specify the native features on a user's device that your app may request access to.
        /// </summary>
        [JsonProperty("devicePermissions", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<DevicePermission> DevicePermissions { get; set; } = new List<DevicePermission>();

        /// <summary>
        /// A list of valid domains from which the tabs expect to load any content. Domain listings can include wildcards, for example `*.example.com`. If your tab configuration or content UI needs to navigate to any other domain besides the one use for tab configuration, that domain must be specified here.
        /// </summary>
        [JsonProperty("validDomains", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<string> ValidDomains { get; set; } = new List<string>();

        /// <summary>
        /// Specify your AAD App ID and Graph information to help users seamlessly sign into your AAD app.
        /// </summary>
        [JsonProperty("webApplicationInfo", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public WebApplicationInfo WebApplicationInfo { get; set; }

        /// <summary>
        /// Specify the app's Graph connector configuration. If this is present then webApplicationInfo.id must also be specified.
        /// </summary>
        [JsonProperty("graphConnector", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public GraphConnector GraphConnector { get; set; }

        /// <summary>
        /// A value indicating whether or not show loading indicator when app/tab is loading
        /// </summary>
        [JsonProperty("showLoadingIndicator", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public bool ShowLoadingIndicator { get; set; } = false;

        /// <summary>
        /// A value indicating whether a personal app is rendered without a tab header-bar
        /// </summary>
        [JsonProperty("isFullScreen", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public bool IsFullScreen { get; set; } = false;

        [JsonProperty("activities", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Activities Activities { get; set; }

        /// <summary>
        /// A list of tenant configured properties for an app
        /// </summary>
        [JsonProperty("configurableProperties", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<ConfigurableProperties> ConfigurableProperties { get; set; }

        /// <summary>
        /// List of 'non-standard' channel types that the app supports. Note: Channels of standard type are supported by default if the app supports team scope.
        /// </summary>
        [JsonProperty("supportedChannelTypes", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<SupportedChannelType> SupportedChannelTypes { get; set; }

        /// <summary>
        /// A value indicating whether an app is blocked by default until admin allows it
        /// </summary>
        [JsonProperty("defaultBlockUntilAdminAction", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public bool DefaultBlockUntilAdminAction { get; set; } = false;

        /// <summary>
        /// The url to the page that provides additional app information for the admins
        /// </summary>
        [JsonProperty("publisherDocsUrl", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string PublisherDocsUrl { get; set; }

        /// <summary>
        /// The install scope defined for this app by default. This will be the option displayed on the button when a user tries to add the app
        /// </summary>
        [JsonProperty("defaultInstallScope", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public DefaultInstallScope DefaultInstallScope { get; set; }

        /// <summary>
        /// When a group install scope is selected, this will define the default capability when the user installs the app
        /// </summary>
        [JsonProperty("defaultGroupCapability", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public DefaultGroupCapability DefaultGroupCapability { get; set; }

        /// <summary>
        /// Specify meeting extension definition.
        /// </summary>
        [JsonProperty("meetingExtensionDefinition", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public MeetingExtensionDefinition MeetingExtensionDefinition { get; set; }

        /// <summary>
        /// Specify and consolidates authorization related information for the App.
        /// </summary>
        [JsonProperty("authorization", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Authorization Authorization { get; set; }
    }
}