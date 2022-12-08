// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Crazor.Interfaces;
using Microsoft.Bot.Connector;

namespace Crazor
{
    public class CardViewFactory
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

        public CardView Create(string tabId)
        {
            if (_views.TryGetValue(tabId, out var TabModuleType))
            {
                return (CardView)_serviceProvider.GetService(TabModuleType);
            }
            throw new ArgumentNullException(nameof(tabId));
        }
    }
}