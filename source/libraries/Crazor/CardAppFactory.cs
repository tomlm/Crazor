// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Microsoft.Bot.Connector;

namespace Crazor
{
    public class CardAppFactory
    {
        private Dictionary<string, Type> _cardApps = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        private IServiceProvider _serviceProvider;

        public CardAppFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
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

        public CardApp Create(CardRoute cardRoute, IConnectorClient client = null)
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
                cardApp.ConnectorClient = client;
                return cardApp;
            }
            throw new ArgumentNullException(nameof(cardRoute));
        }
    }
}