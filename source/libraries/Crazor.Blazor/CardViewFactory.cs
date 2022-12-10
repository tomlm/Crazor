// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Crazor.Interfaces;

namespace Crazor.Blazor
{
    public class CardViewFactory : ICardViewFactory
    {
        private Dictionary<string, Type> _views = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        private IServiceProvider _serviceProvider;

        public CardViewFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Add(string name, Type type)
        {
            ArgumentNullException.ThrowIfNull(name);
            ArgumentNullException.ThrowIfNull(type);
            _views[name] = type;
        }

        public IEnumerable<string> GetNames() => _views.Keys.OrderBy(n => n);

        public ICardView Create(CardRoute route)
        {
            if (_views.TryGetValue(route.View, out var cardViewType))
            {
                return (ICardView)_serviceProvider.GetService(cardViewType);
            }
            throw new ArgumentNullException(route.Route);
        }

        public ICardView Create(string typeName)
        {
            if (!_views.TryGetValue(typeName, out var cardViewType))
            {
                throw new Exception($"{typeName} is not a known type");
            }

            return (ICardView)_serviceProvider.GetService(cardViewType);
        }

        public bool HasView(string nameOrRoute)
        {
            return _views.ContainsKey(nameOrRoute);
        }
    }
}