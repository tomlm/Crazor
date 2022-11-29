﻿// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using AdaptiveCards;
using AdaptiveCards.Rendering;
using Crazor.Encryption;
using Crazor.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Neleus.DependencyInjection.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpBot;
using System.Reflection;
using Diag = System.Diagnostics;

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

            RouteManager routeManager = new RouteManager();

            // add Apps
            var cardAppServices = services.AddByName<CardApp>();
            services.AddTransient(typeof(CardApp));
            HashSet<string> cardAppNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var cardAppType in AppDomain.CurrentDomain.GetAssemblies().SelectMany(asm => asm.DefinedTypes
                                            .Where(t => t.IsAssignableTo(typeof(CardApp)) && t.IsAbstract == false)))
            {
                if (cardAppType != typeof(CardApp))
                {
                    services.AddTransient(cardAppType);
                    var name = cardAppType.Name.EndsWith("App") ? cardAppType.Name.Substring(0, cardAppType.Name.Length - 3) : cardAppType.Name;
                    cardAppNames.Add(name);
                    cardAppServices.Add(name, cardAppType);
                }
            }

            // Automatically register CardApp for Default.cshtml in folders so you don't have to define one unless you need one.
            // We do this by enumerating all ICardView implementations 
            foreach (var cardViewType in AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(asm => asm.DefinedTypes.Where(t => t.GetInterface(nameof(ICardView)) != null)))
            {
                // .cshtml files class names will be "Cards_{AppName}_Default" 
                // we want to register the Folder as the a CardApp if it hasn't been registered already
                var parts = cardViewType.Name.Split("_");
                if (parts.Length >= 3 && parts[0].ToLower() == "cards" && parts[2].ToLower() == "default")
                {
                    var appName = parts[1];
                    if (!cardAppNames.Contains(appName))
                    {
                        cardAppServices.Add(appName, typeof(CardApp));
                    }
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

            // add IMessagingExtensionQuery implementations
            var queryCommmandServices = services.AddByName<IMessagingExtensionQuery>();
            foreach (var queryCommandType in AppDomain.CurrentDomain.GetAssemblies().SelectMany(asm => asm.DefinedTypes.Where(t => t.IsAssignableTo(typeof(IMessagingExtensionQuery)) && t.IsAbstract == false)))
            {
                services.AddTransient(queryCommandType);
                queryCommmandServices.Add(queryCommandType.FullName, queryCommandType);
            }
            queryCommmandServices.Build();

            // add Cardviews to route manager
            var cardViewServices = services.AddByName<ICardView>();
            foreach (var cardView in AppDomain.CurrentDomain.GetAssemblies().SelectMany(asm => 
                            asm.DefinedTypes.Where(t => t.IsAbstract == false && t.ImplementedInterfaces.Contains(typeof(ICardView)))))
            {
                var cardViewType = cardView.AsType();
                if (cardViewType.Name != "CardView" && 
                    cardViewType.Name != "CardView`1" && 
                    cardViewType.Name != "CardView`2" && 
                    cardViewType.Name != "CardViewBase`1" &&
                    cardViewType.Name != "EmptyCardView")
                {
                    services.AddTransient(cardViewType);
                    cardViewServices.Add(cardViewType.FullName, cardViewType);
                    routeManager.Add(cardViewType);
                }
            }
            services.AddSingleton(routeManager);

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

        public static IInvokeActivity CreateActionInvokeActivity(this IActivity sourceActivity, string? verb=null, JObject? data=null)
        {
            var activity = sourceActivity.Clone();
            activity.Type = ActivityTypes.Invoke;
            var invokeValue = new AdaptiveCardInvokeValue()
            {
                Action = new AdaptiveCardInvokeAction()
                {
                    Verb = verb ?? Constants.SHOWVIEW_VERB,
                    Data = data ?? new JObject()
                }
            };
            activity!.Value = invokeValue;
            return activity.AsInvokeActivity();
        }

        public static Activity CreateReply(this IActivity activity, string? text = null, string? locale = null)
        {
            return ((Activity)activity).CreateReply(text, locale);
        }

        public static IInvokeActivity CreateLoadRouteActivity(this IActivity sourceActivity, string route)
        {
            var activity = sourceActivity.Clone();
            activity.Type = ActivityTypes.Invoke;
            var invokeValue = new AdaptiveCardInvokeValue()
            {
                Action = new AdaptiveCardInvokeAction()
                {
                    Verb = Constants.LOADROUTE_VERB,
                    Data = new JObject() { { Constants.ROUTE_KEY, route } }
                }
            };
            activity!.Value = invokeValue;
            return activity.AsInvokeActivity();
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

        /// <summary>
        /// Helper extension to replace elements of a given type.
        /// </summary>
        /// <typeparam name="ElementT"></typeparam>
        /// <param name="card"></param>
        /// <param name="transformer"></param>
        /// <returns></returns>
        public static AdaptiveCard ReplaceElement<ElementT>(this AdaptiveCard card, Func<ElementT, AdaptiveTypedElement> transformer)
            where ElementT : AdaptiveTypedElement
        {
            new ReplaceElement<ElementT>(transformer).Visit(card);
            return card;
        }
    }

    internal class ReplaceElement<ElementT> : AdaptiveVisitor
        where ElementT : AdaptiveTypedElement
    {
        Func<ElementT, AdaptiveTypedElement> _transformer;

        internal ReplaceElement(Func<ElementT, AdaptiveTypedElement> transformer)
        {
            _transformer = transformer;
        }

        protected override void Visit(AdaptiveCard card)
        {
            ReplaceElements(card.Body);
            ReplaceActions(card.Actions);
            base.Visit(card);
        }

        protected override void Visit(AdaptiveContainer container)
        {
            ReplaceElements(container.Items);
            base.Visit(container);
        }

        protected override void Visit(AdaptiveActionSet actionSet)
        {
            ReplaceActions(actionSet.Actions);
            base.Visit(actionSet);
        }


        private void ReplaceElements(List<AdaptiveElement> elements)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                var element = elements[i];
                if (element is ElementT el)
                {
                    elements[i] = (AdaptiveElement)_transformer(el);
                }
            }
        }

        private void ReplaceActions(List<AdaptiveAction> actions)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                var element = actions[i];
                if (element is ElementT el)
                {
                    actions[i] = (AdaptiveAction)_transformer(el);
                }
            }
        }

    }

}
