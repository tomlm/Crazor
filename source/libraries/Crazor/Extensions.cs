using Crazor.Attributes;
using Crazor.Encryption;
using Crazor.Interfaces;
using Crazor.Rendering;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Diag = System.Diagnostics;

namespace Crazor
{
    public static class CrazorExtensions
    {
        private static readonly System.Text.Json.JsonSerializerOptions DEFAULT_JSON_SERIALIZER_OPTIONS = new()
        {
            WriteIndented = true,
        };

        public static IServiceCollection AddCrazor(this IServiceCollection services, params string[] sharedAssemblies)
        {
            return services.AddCrazor((options) => { }, sharedAssemblies);
        }

        public static IServiceCollection AddCrazor(this IServiceCollection services, Action<ServiceOptions> options, params string[] sharedAssemblies)
        {
            if (sharedAssemblies != null)
            {
                foreach (var assembly in sharedAssemblies)
                {
                    Assembly.Load(assembly);
                }
            }

            return AddCrazor(
                services,
                options);
        }

        public static string GetCardRoute<CardViewT>(this ICardView cardView)
            where CardViewT : ICardView
        {
            var route = typeof(CardViewT).Name;
            var cardRouteAtt = typeof(CardViewT).GetCustomAttribute<CardRouteAttribute>();
            if (cardRouteAtt != null)
            {
                // only untemplated routes can be bound this way.
                if (!cardRouteAtt.Template.Contains('{'))
                    route = cardRouteAtt.Template;
            }
            return route;
        }


        //private static XmlSerializer xmlSerializer = new XmlSerializer(typeof(AdaptiveCard), defaultNamespace: AdaptiveCard.ContentType);

        //public static string ToXml(this AdaptiveCard card)
        //{
        //    try
        //    {
        //        XmlWriterSettings settings = new XmlWriterSettings()
        //        {
        //            Encoding = new UnicodeEncoding(false, false), // no BOM in a .NET string
        //            Indent = true,
        //        };

        //        using (StringWriter textWriter = new StringWriter())
        //        {
        //            using (XmlWriter xmlWriter = XmlWriter.Create(textWriter, settings))
        //            {
        //                xmlSerializer.Serialize(xmlWriter, card, new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty }));
        //            }
        //            return textWriter.ToString(); //This is the output as a string
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        Diag.Debug.WriteLine(err.ToString());
        //    }
        //    return string.Empty;
        //}

        public static IServiceCollection AddCrazor(this IServiceCollection services, Action<ServiceOptions> options)
        {
            services.AddSingleton<ServiceOptions>(provider =>
            {
                // Get custom options from users
                var serviceOptions = new ServiceOptions();

                serviceOptions.ChannelOptions["Default"] = new ChannelOptions()
                {
                    SupportsCardHeader = false,
                    SupportsTaskModule = false,
                    SchemaVersion = new AdaptiveSchemaVersion(1, 5),
                    Authentication = AuthenticationFlags.None
                };

                serviceOptions.ChannelOptions[Channels.Msteams] = new ChannelOptions()
                {
                    SupportsCardHeader = true,
                    SupportsTaskModule = true,
                    SchemaVersion = new AdaptiveSchemaVersion(1, 5),
                    Authentication = AuthenticationFlags.SSO | AuthenticationFlags.OAuth
                };

                serviceOptions.ChannelOptions[ChannelsEx.M365Extensions] = new ChannelOptions()
                {
                    SupportsCardHeader = true,
                    SupportsTaskModule = true,
                    SchemaVersion = new AdaptiveSchemaVersion(1, 5),
                    Authentication = AuthenticationFlags.SSO | AuthenticationFlags.OAuth
                };

                var configuration = provider.GetRequiredService<IConfiguration>();
                var host = configuration.GetValue<Uri>("HostUri")!.Host;
                serviceOptions.ChannelOptions[host] = new ChannelOptions()
                {
                    SupportsCardHeader = false,
                    SupportsTaskModule = false,
                    SchemaVersion = new AdaptiveSchemaVersion(1, 5)
                };

                if (options != null)
                {
                    // give user opportunity to tweak default settings.
                    options.Invoke(serviceOptions);
                }
                return serviceOptions;
            });

            services.AddHttpClient();
            services.TryAddSingleton<IStorage>((sp) =>
            {
                Diag.Trace.TraceWarning(@"There is no IStorage provider registered for Crazor cards to use.");
                Diag.Trace.TraceWarning("The MemoryStorage provider is being used which is only suitable for local development becuase it is not durable.");
                Diag.Trace.TraceWarning("Add an IStorage provider via dependency injection in your program.cs.  For example to register Azure BlobStorage as your provider:");
                Diag.Trace.TraceWarning("    var storageKey = builder.Configuration.GetValue<string>(\"AzureStorage\");");
                Diag.Trace.TraceWarning("    if (!String.IsNullOrEmpty(storageKey))");
                Diag.Trace.TraceWarning("    {");
                Diag.Trace.TraceWarning("        builder.Services.AddSingleton<IStorage, BlobsStorage>(sp => new BlobsStorage(storageKey, \"opbot\"));");
                Diag.Trace.TraceWarning("    }");
                return new MemoryStorage();
            });
            services.TryAddSingleton<IEncryptionProvider, NoEncryptionProvider>();
            services.AddSingleton<IRouteResolver, RouteResolver>();

            // CardAppContext is scoped data for a request
            services.AddScoped<CardAppContext>();

            // register CardAppFactory and card apps
            services.AddScoped<CardAppFactory>();
            services.AddTransient<CardApp>();
            services.AddCardAppTypes();

            services.AddCustomCardViewTypes();

            return services;
        }

        public static IServiceCollection AddCardAppTypes(this IServiceCollection services)
        {
            // add App types as transient dependency injection types
            foreach (var cardAppType in CardAppFactory.GetCardAppTypes())
            {
                services.AddTransient(cardAppType);
            }
            return services;
        }

        public static IServiceCollection AddCustomCardViewTypes(this IServiceCollection services)
        {
            // add card view types for razor templates
            foreach (var cardViewType in Utils.GetAssemblies().SelectMany(asm => asm.DefinedTypes
                        .Where(t => t.IsAbstract == false && t.IsAssignableTo(typeof(ICardView)) && t.IsAssignableTo(typeof(CustomCardView)))))
            {
                services.AddTransient(cardViewType);
            }
            return services;
        }

        public static AdaptiveCard TransformActionExecuteToSubmit(this AdaptiveCard card)
        {
            foreach (var action in card.GetElements<AdaptiveExecuteAction>())
            {
                if (action.Data == null)
                {
                    action.Data = new JObject();
                }
                else if (action.Data is string str)
                {
                    action.Data = JObject.Parse(str);
                }
                ((JObject)action.Data)[Constants.SUBMIT_VERB] = action.Verb;
                action.Verb = null!;
            }
            var json = JsonConvert.SerializeObject(card);
            json = json.Replace(AdaptiveExecuteAction.TypeName, AdaptiveSubmitAction.TypeName);
            return JsonConvert.DeserializeObject<AdaptiveCard>(json)!;
        }

        public static IInvokeActivity CreateActionInvokeActivity(this IActivity sourceActivity, string? verb)
        {
            return CreateActionInvokeActivity((Activity)sourceActivity, verb, null);
        }

        public static Activity Clone(this IActivity activity)
        {
            return JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject((Activity)activity))!;
        }

        public static Activity Clone(this Activity activity)
        {
            return JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject(activity))!;
        }

        public static IInvokeActivity CreateActionInvokeActivity(this IActivity sourceActivity, string? verb = null, JObject? data = null)
        {
            return sourceActivity.CreateActionInvokeActivity(new AdaptiveCardInvokeValue()
            {
                Action = new AdaptiveCardInvokeAction()
                {
                    Type = AdaptiveExecuteAction.TypeName,
                    Verb = verb ?? Constants.REFRESH_VERB,
                    Data = data ?? new JObject()
                }
            });
        }

        public static IInvokeActivity CreateActionInvokeActivity(this IActivity sourceActivity, AdaptiveCardInvokeValue invokeValue)
        {
            var activity = sourceActivity.Clone();
            activity.Type = ActivityTypes.Invoke;
            activity.Name = "adaptiveCard/action";
            activity!.Value = JObject.FromObject(invokeValue);
            return activity.AsInvokeActivity();
        }

        public static Activity CreateReply(this IActivity activity, string? text = null, string? locale = null)
        {
            return ((Activity)activity).CreateReply(text, locale);
        }

        /// <summary>
        /// Reply to the user with a message and card
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="route">route for the card</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns></returns>
        public static async Task<ResourceResponse> ReplyWithCardAsync(this ITurnContext turnContext, string message, AdaptiveCard card, CancellationToken cancellationToken)
        {
            var replyActivity = turnContext.Activity.CreateReply(message);
            replyActivity.Attachments.Add(new Attachment(AdaptiveCard.ContentType, content: card));
            return await turnContext.SendActivityAsync(replyActivity, cancellationToken);
        }

        public static IInvokeActivity CreateLoadRouteActivity(this IActivity sourceActivity, string route)
        {
            var activity = sourceActivity.Clone();
            activity.Type = ActivityTypes.Invoke;
            activity.Name = "adaptiveCard/action";
            var invokeValue = new AdaptiveCardInvokeValue()
            {
                Action = new AdaptiveCardInvokeAction()
                {
                    Type = AdaptiveExecuteAction.TypeName,
                    Verb = Constants.LOAD_VERB,
                    Data = new JObject() { { Constants.ROUTE_KEY, route } }
                }
            };
            activity!.Value = JObject.FromObject(invokeValue, new JsonSerializer() { NullValueHandling = NullValueHandling.Ignore });
            return activity.AsInvokeActivity();
        }

        public static IEnumerable<T> GetElements<T>(this AdaptiveElement element)
            where T : AdaptiveElement
        {
            var visitor = new AdaptiveVisitor();
            visitor.Visit(element);
            return visitor.Elements.OfType<T>();
        }

        public static IEnumerable<T> GetElements<T>(this AdaptiveCard card)
        {
            var visitor = new AdaptiveVisitor();
            visitor.Visit(card);
            return visitor.Elements.OfType<T>();
        }

        public static void ApplyRouteAttributes(this CardApp app, CardRoute route)
        {
            ApplyRouteAttributesToTarget(app, route);

            // map App. routedata to app
            foreach (var routeProperty in route.RouteData.Properties().Where(p => p.Name.StartsWith("App.")))
            {
                ObjectPath.SetPathValue(app, routeProperty.Name.Substring(4), routeProperty.Value.ToString(), false);
            }
        }

        public static void ApplyRouteAttributes(this ICardView cardView, CardRoute route)
        {
            ApplyRouteAttributesToTarget(cardView, route);
        }

        private static void ApplyRouteAttributesToTarget(object target, CardRoute route)
        {
            // map Route attributes for target
            foreach (var targetProperty in target.GetType().GetProperties().Where(prop => prop.GetCustomAttribute<FromCardRouteAttribute>() != null))
            {
                var fromRouteName = targetProperty.GetCustomAttribute<FromCardRouteAttribute>()?.Name ?? targetProperty.Name;
                if (fromRouteName != null)
                {
                    var dataProperty = route.RouteData.Properties().Where(p => p.Name.ToLower() == fromRouteName.ToLower()).SingleOrDefault();
                    if (dataProperty != null)
                    {
                        target.SetTargetProperty(targetProperty, dataProperty.Value);
                    }
                }
            }

            // map query parameters for target
            foreach (var targetProperty in target.GetType().GetProperties().Where(prop => prop.GetCustomAttribute<FromCardQueryAttribute>() != null))
            {
                var fromQueryName = targetProperty.GetCustomAttribute<FromCardQueryAttribute>()?.Name ??
                    targetProperty.Name;
                if (fromQueryName != null)
                {
                    var dataProperty = route.RouteData.Properties().Where(p => p.Name.ToLower() == fromQueryName.ToLower()).SingleOrDefault();
                    if (dataProperty != null)
                    {
                        target.SetTargetProperty(targetProperty, dataProperty.Value);
                    }
                }
            }
        }


        public static void SetTargetProperty(this object targetObject, PropertyInfo? targetProperty, object value)
        {
            if (targetProperty != null)
            {
                if (value != null)
                {
                    if (value.GetType() != targetProperty.PropertyType)
                    {
                        var targetType = targetProperty.PropertyType;
                        if (targetType.Name == "Nullable`1" && targetType.GenericTypeArguments.Any())
                        {
                            targetType = targetType.GenericTypeArguments[0];
                        }
                        if (value.GetType() != targetProperty.PropertyType)
                        {
                            switch (targetType.Name)
                            {
                                case "Byte":
                                    value = Convert.ToByte(Convert.ToDouble(value.ToString()));
                                    break;
                                case "Int16":
                                    value = Convert.ToInt16(Convert.ToDouble(value.ToString()));
                                    break;
                                case "Int32":
                                    value = Convert.ToInt32(Convert.ToDouble(value.ToString()));
                                    break;
                                case "Int64":
                                    value = Convert.ToInt64(Convert.ToDouble(value.ToString()));
                                    break;
                                case "UInt16":
                                    value = Convert.ToUInt16(Convert.ToDouble(value.ToString()));
                                    break;
                                case "UInt32":
                                    value = Convert.ToUInt32(Convert.ToDouble(value.ToString()));
                                    break;
                                case "UInt64":
                                    value = Convert.ToUInt64(Convert.ToDouble(value.ToString()));
                                    break;
                                case "Single":
                                    value = Convert.ToSingle(Convert.ToDouble(value.ToString()));
                                    break;
                                case "Double":
                                    value = Convert.ToDouble(value.ToString());
                                    break;
                                case "Boolean":
                                    value = Convert.ToBoolean(value.ToString());
                                    break;
                                case "DateTime":
                                    value = Convert.ToDateTime(value.ToString());
                                    break;
                                case "DateTimeOffset":
                                    value = new DateTimeOffset(Convert.ToDateTime(value.ToString()));
                                    break;
                                case "DateOnly":
                                    if (value is DateTime dt)
                                        value = DateOnly.FromDateTime(dt);
                                    else if (value is DateTimeOffset dto)
                                        value = DateOnly.FromDateTime(dto.DateTime);
                                    else
                                        value = DateOnly.Parse(value?.ToString()!);
                                    break;
                                case "TimeSpan":
                                    if (value is DateTime dt2)
                                        value = dt2.TimeOfDay;
                                    else if (value is DateTimeOffset dto)
                                        value = dto.TimeOfDay;
                                    else
                                        value = TimeSpan.Parse(value?.ToString()!);
                                    break;
                                case "TimeOnly":
                                    if (value is DateTime dt3)
                                        value = TimeOnly.FromDateTime(dt3);
                                    else if (value is DateTimeOffset dto)
                                        value = TimeOnly.FromDateTime(dto.DateTime);
                                    else
                                        value = TimeOnly.Parse(value?.ToString()!);
                                    break;
                                case "String":
                                    value = value?.ToString()!;
                                    break;
                                default:
                                    if (targetType.IsEnum)
                                    {
                                        value = Enum.Parse(targetType, value.ToString()!);
                                    }
                                    else if (value is JToken jt)
                                    {
                                        value = jt.ToObject(targetType)!;
                                    }
                                    break;
                            }
                        }
                    }
                    targetProperty.SetValue(targetObject, value);
                }
                else
                {
                    targetProperty.SetValue(targetObject, (targetProperty.PropertyType.IsValueType) ? Activator.CreateInstance(targetProperty.PropertyType) : null);
                }
            }
        }

        public static AdaptiveCardInvokeValue CreateInvokeValue(this AdaptiveExecuteAction action, ITurnContext turnContext)
        {
            var activity = turnContext.Activity.Clone();
            return new AdaptiveCardInvokeValue()
            {
                Action = new AdaptiveCardInvokeAction()
                {
                    Id = action.Id,
                    Verb = action.Verb,
                    Data = action.Data,
                }
            };
        }

        public static string ToJson(this Activity activity)
        {
            return System.Text.Json.JsonSerializer.Serialize(activity, DEFAULT_JSON_SERIALIZER_OPTIONS);
        }
    }
}
