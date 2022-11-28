// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Neleus.DependencyInjection.Extensions;

namespace Crazor
{
    public class CardAppFactory
    {
        private IServiceByNameFactory<CardApp> _cardAppByName;

        public CardAppFactory(IServiceByNameFactory<CardApp> serviceByName)
        {
            _cardAppByName = serviceByName;
        }

        public IEnumerable<string> GetNames() => _cardAppByName.GetNames().OrderBy(n => n);

        public CardApp Create(CardRoute cardRoute)
        {
            var cardName = _cardAppByName.GetNames().SingleOrDefault(cardName => String.Equals(cardName, cardRoute.App, StringComparison.OrdinalIgnoreCase));
            if (cardName != null)
            {
                var cardApp = _cardAppByName.GetByName(cardName);
                if (cardApp.GetType() == typeof(CardApp))
                {
                    cardApp.Name = cardName;
                }
                cardApp.Route = cardRoute;
                return cardApp;
            }
            throw new ArgumentNullException(nameof(cardRoute));
        }
    }
}