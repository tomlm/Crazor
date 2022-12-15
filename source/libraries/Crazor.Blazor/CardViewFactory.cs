// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Crazor.Blazor.ComponentRenderer;
using Crazor.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Crazor.Blazor
{
    public class CardViewFactory : ICardViewFactory
    {
        private Dictionary<string, Type> _views = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        private IServiceProvider _serviceProvider;

        public CardViewFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            foreach (var cardViewType in CardView.GetCardViewTypes())
            {
                this.Add(cardViewType.FullName, cardViewType);
            }
        }

        public void Add(string name, Type type)
        {
            ArgumentNullException.ThrowIfNull(name);
            ArgumentNullException.ThrowIfNull(type);
            _views[name] = type;
        }

        public IEnumerable<string> GetNames() => _views.Keys.OrderBy(n => n);

        public ICardView Create(string typeName)
        {
            if (!_views.TryGetValue(typeName, out var cardViewType))
            {
                throw new Exception($"{typeName} is not a known type");
            }

            var card = (ICardView)_serviceProvider.GetRequiredService(cardViewType);
            card.AssignInjectAttributeProperties(_serviceProvider);
            return card;
        }

    }
}