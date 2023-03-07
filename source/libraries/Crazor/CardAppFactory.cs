// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Crazor.Interfaces;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using System.Reflection;

namespace Crazor
{
    public class CardAppFactory
    {
        private Dictionary<string, Type> _cardApps = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        private IServiceProvider _serviceProvider;

        public CardAppFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            foreach (var cardAppType in GetCardAppTypes())
            {
                if (cardAppType != typeof(CardApp))
                {
                    var name = cardAppType.Name.EndsWith("App") ? cardAppType.Name.Substring(0, cardAppType.Name.Length - 3) : cardAppType.Name;
                    this.Add(name, cardAppType);
                }
            }

            // Automatically register CardApp for Default.cshtml in folders so you don't have to define one unless you need one.
            // We do this by enumerating all ICardView implementations 
            foreach (var cardViewType in Utils.GetAssemblies()
                .SelectMany(asm => asm.DefinedTypes.Where(t => t.GetInterface(nameof(ICardView)) != null)))
            {
                // .cshtml files class names will be "Cards_{AppName}_Default" 
                // we want to register the Folder as the a CardApp if it hasn't been registered already
                var parts = cardViewType.Name.Split("_");
                if (parts.Length >= 3 && parts[0].ToLower() == "cards" && parts[2].ToLower() == "default")
                {
                    var appName = parts[1];
                    if (!this.HasRegistration(appName))
                    {
                        this.Add(appName, typeof(CardApp));
                    }
                }
                else
                {
                    parts = cardViewType.FullName!.Split('.');
                    string? appName = parts.SkipWhile(p => p.ToLower() != "cards").Skip(1).FirstOrDefault();
                    if (appName != null && !this.HasRegistration(appName))
                    {
                        this.Add(appName, typeof(CardApp));
                    }
                }
            }
        }

        public bool HasRegistration(string name) => _cardApps.ContainsKey(name);

        public void Add(string name, Type type)
        {
            ArgumentNullException.ThrowIfNull(name);
            ArgumentNullException.ThrowIfNull(type);
            if (!type.IsAssignableTo(typeof(CardApp)))
                throw new Exception($"{type.Name} is not a card app type");

            _cardApps.Add(name, type);
        }

        public IEnumerable<string> GetNames() => _cardApps.Keys.OrderBy(n => n);

        public CardApp Create(CardRoute cardRoute, ITurnContext turnContext = null)
        {
            if (_cardApps.TryGetValue(cardRoute.App, out var cardAppType))
            {
                var cardApp = (CardApp)_serviceProvider.GetService(cardAppType);
                if (cardAppType == typeof(CardApp))
                {
                    // this is folder with no app defined
                    cardApp.Name = cardRoute.App;
                }
                cardApp.Route = cardRoute;
                cardApp.TurnContext = turnContext;
                return cardApp;
            }
            throw new ArgumentNullException(nameof(cardRoute));
        }

        internal static IEnumerable<TypeInfo> GetCardAppTypes()
        {
            foreach (var assembly in Utils.GetAssemblies())
            {
                foreach (var type in assembly.DefinedTypes)
                {
                    if (type.IsAssignableTo(typeof(CardApp)) && type.IsAbstract == false)
                    {
                        yield return type;
                    }
                }
            }
        }
    }
}