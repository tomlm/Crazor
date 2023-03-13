// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Crazor.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Crazor
{
    /// <summary>
    /// Default CardViewFactory which creates an instance using DI and then [inject] attributes to inject dependencies into view.
    /// </summary>
    public class CardViewFactory 
    {
        private IServiceProvider _serviceProvider;

        public CardViewFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ICardView Create(Type cardViewType)
        {
            var cardView = (ICardView)_serviceProvider.GetRequiredService(cardViewType);

            // inject dependencies
            var props = cardViewType
                             .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                             .Where(p => p.CanWrite && Attribute.IsDefined(p, typeof(InjectAttribute)));

            foreach (var prop in props)
            {
                prop.SetValue(cardView, _serviceProvider.GetService(prop.PropertyType), null);
            }

            return cardView;
        }

    }
}