// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Crazor.Interfaces;
using Microsoft.AspNetCore.Components;

namespace Crazor
{
    /// <summary>
    /// Default CardViewFactory which creates an instance using Activator and honors [inject] attributes to inject dependencies into view.
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
            var card = (ICardView)Activator.CreateInstance(cardViewType);

            // inject dependencies
            var props = cardViewType
                             .GetProperties()
                             .Where(p => p.CanWrite && Attribute.IsDefined(p, typeof(InjectAttribute)));

            foreach (var prop in props)
            {
                prop.SetValue(card, _serviceProvider.GetService(prop.PropertyType), null);
            }

            return card;
        }
    }
}