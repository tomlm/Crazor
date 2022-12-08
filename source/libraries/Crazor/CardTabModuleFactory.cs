// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.DependencyInjection;

namespace Crazor
{
    public class CardTabModuleFactory
    {
        private readonly Dictionary<string, Type> _tabModules = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        private readonly IServiceProvider _services;

        public CardTabModuleFactory(IServiceProvider serviceProvider)
        {
            _services = serviceProvider;
        }

        public void Add(string name, Type type)
        {
            ArgumentNullException.ThrowIfNull(name);
            ArgumentNullException.ThrowIfNull(type);
            if (!type.IsAssignableTo(typeof(CardTabModule)))
                throw new Exception($"{type.Name} is not a card tab module type");

            _tabModules[name] = type;
        }

        public IEnumerable<string> GetNames() => _tabModules.Keys.OrderBy(n => n);

        public CardTabModule Create(string tabId)
        {
            if (tabId.StartsWith("/Cards"))
            {
                var singleTab = _services.GetRequiredService<SingleCardTabModule>();
                singleTab.SetRoute(tabId);
                return singleTab;
            }

            if (_tabModules.TryGetValue(tabId, out var tabType))
            {
                return (CardTabModule)_services.GetService(tabType);
            }
            throw new Exception($"Unknown tab: {tabId}");
        }
    }
}