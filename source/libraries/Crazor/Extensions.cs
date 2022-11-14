using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Crazor.Encryption;
using Crazor.Interfaces;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Neleus.DependencyInjection.Extensions;
using OpBot;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Diag = System.Diagnostics;
using AdaptiveCards.Rendering;
using AdaptiveCards;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.Bot.Schema;
using System.Xml.Linq;

namespace Crazor
{
    public static class Extensions
    {
        public static IServiceCollection AddCrazor(this IServiceCollection services)
        {
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
            services.TryAddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();
            services.TryAddScoped<CardAppFactory>();
            services.TryAddScoped<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();
            services.TryAddScoped<IBot, CardActivityHandler>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.TryAddScoped<IUrlHelper, UrlHelperProxy>();

            // add Apps
            var cardAppServices = services.AddByName<CardApp>();
            services.AddTransient(typeof(CardApp));
            HashSet<string> cardAppNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var cardAppType in AppDomain.CurrentDomain.GetAssemblies().SelectMany(asm => asm.DefinedTypes.Where(t => t.IsAssignableTo(typeof(CardApp)) && t.IsAbstract == false)))
            {
                if (cardAppType != typeof(CardApp))
                {
                    services.AddTransient(cardAppType);
                    var name = cardAppType.Name.EndsWith("App") ? cardAppType.Name.Substring(0, cardAppType.Name.Length - 3) : cardAppType.Name;
                    cardAppNames.Add(name);
                    cardAppServices.Add(name, cardAppType);
                }
            }

            // add in folders with no CardApp
            foreach (var folder in Directory.EnumerateDirectories(Path.Combine(Environment.CurrentDirectory, $"Cards")))
            {
                var appName = Path.GetFileName(folder);
                if (!cardAppNames.Contains(appName))
                {
                    cardAppServices.Add(appName, typeof(CardApp));
                }
            }
            cardAppServices.Build();

            // add TabModules
            var cardTabModuleServices = services.AddByName<CardTabModule>();
            foreach (var tabModuleType in AppDomain.CurrentDomain.GetAssemblies().SelectMany(asm => asm.DefinedTypes.Where(t => t.IsAssignableTo(typeof(CardTabModule)) && t.IsAbstract == false)))
            {
                services.AddTransient(tabModuleType);
                cardTabModuleServices.Add(tabModuleType.FullName, tabModuleType);
            }
            cardTabModuleServices.Build();

            // add IQueryCommand implementations
            var queryCommmandServices = services.AddByName<IMessagingExtensionQuery>();
            foreach (var queryCommandType in AppDomain.CurrentDomain.GetAssemblies().SelectMany(asm => asm.DefinedTypes.Where(t => t.IsAssignableTo(typeof(IMessagingExtensionQuery)) && t.IsAbstract == false)))
            {
                services.AddTransient(queryCommandType);
                queryCommmandServices.Add(queryCommandType.FullName, queryCommandType);
            }
            queryCommmandServices.Build();

            // add Cardviews
            var cardViewServices = services.AddByName<ICardView>();
            foreach (var cardView in AppDomain.CurrentDomain.GetAssemblies().SelectMany(asm => asm.DefinedTypes.Where(t => t.IsAbstract == false && t.ImplementedInterfaces.Contains(typeof(ICardView)))))
            {
                var cardViewType = cardView.AsType();
                if (cardViewType.Name != "CardView`1" && cardViewType.Name != "CardView`2")
                {
                    services.AddTransient(cardViewType);
                    cardViewServices.Add(cardViewType.FullName, cardViewType);
                }
            }
            cardViewServices.Build();

            // add card Razor pages support
            var mvcBuilder = services.AddRazorPages()
                 .AddRazorOptions(options =>
                 {
                     options.ViewLocationFormats.Add("/Cards/{0}.cshtml");
                 });

            HttpHelper.BotMessageSerializerSettings.Formatting = Formatting.None;
            HttpHelper.BotMessageSerializer.Formatting = Formatting.None;
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
                ((JObject)action.Data)["_verb"] = action.Verb;
                action.Verb = null;
            }
            var json = JsonConvert.SerializeObject(card);
            json = json.Replace(AdaptiveExecuteAction.TypeName, AdaptiveSubmitAction.TypeName);
            return JsonConvert.DeserializeObject<AdaptiveCard>(json)!;
        }

        public static Activity CreateActionInvokeActivity(this IActivity sourceActivity, string? verb)
        {
            return CreateActionInvokeActivity((Activity)sourceActivity, verb);
        }

        public static Activity Clone(this IActivity activity)
        {
            return JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject((Activity)activity))!;
        }

        public static Activity Clone(this Activity activity)
        {
            return JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject(activity))!;
        }

        public static Activity CreateActionInvokeActivity(this Activity sourceActivity, string? verb)
        {
            var activity = sourceActivity.Clone();
            var invokeValue = new AdaptiveCardInvokeValue()
            {
                Action = new AdaptiveCardInvokeAction()
                {
                    Verb = verb ?? Constants.SHOWVIEW_VERB,
                    Data = new JObject()
                }
            };
            activity!.Value = invokeValue;
            return activity;
        }

        public static Activity CreateReply(this IActivity activity, string? text = null, string? locale = null)
        {
            return ((Activity)activity).CreateReply(text, locale);
        }

        public static Activity CreateLoadRouteActivity(this IActivity sourceActivity, string view, string path)
        {
            return CreateLoadRouteActivity((Activity)sourceActivity, view, path);
        }

        public static Activity CreateLoadRouteActivity(this Activity sourceActivity, string view, string path)
        {
            var activity = sourceActivity.Clone();
            var invokeValue = new AdaptiveCardInvokeValue()
            {
                Action = new AdaptiveCardInvokeAction()
                {
                    Verb = Constants.LOADROUTE_VERB,
                    Data = new LoadRouteModel
                    {
                        View = view ?? Constants.DEFAULT_VIEW,
                        Path = path
                    }
                }
            };
            activity!.Value = invokeValue;
            return activity;
        }

        public static async Task<SessionData> GetSessionDataFromInvokeAsync(this AdaptiveCardInvokeValue invokeValue, IEncryptionProvider encryptionProvider, CancellationToken cancellationToken)
        {
            // Get session data from the invoke payload
            var data = await encryptionProvider.DecryptAsync((string)((dynamic)invokeValue.Action.Data)._sessiondata, cancellationToken);
            var sessionData = SessionData.FromString(data);
            return sessionData;
        }

        public static async Task<SessionData> GetSessionDataFromActionAsync(this AdaptiveExecuteAction action, IEncryptionProvider encryptionProvider, CancellationToken cancellationToken)
        {
            // Get session data from the invoke payload
            var data = await encryptionProvider.DecryptAsync((string)((dynamic)action.Data)._sessiondata, cancellationToken);
            var sessionData = SessionData.FromString(data);
            return sessionData;
        }

        public static IEnumerable<T> GetElements<T>(this AdaptiveTypedElement element)
            where T : AdaptiveTypedElement
        {
            var visitor = new AdaptiveVisitor();
            visitor.Visit(element);
            return visitor.Elements.OfType<T>();
        }

        public static void SetTargetProperty(this object targetObject, PropertyInfo? targetProperty, object value)
        {
            if (targetProperty != null)
            {
                if (value != null)
                {
                    var targetType = targetProperty.PropertyType;
                    if (targetType.Name == "Nullable`1")
                    {
                        targetType = targetType.GenericTypeArguments[0];
                    }

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
                        case "String":
                            value = value.ToString();
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

    }
}
