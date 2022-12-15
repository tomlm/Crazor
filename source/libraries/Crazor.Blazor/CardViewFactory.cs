// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Crazor.Blazor.ComponentRenderer;
using Crazor.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Crazor.Blazor
{
    public class CardViewFactory : ICardViewFactory
    {
        private IServiceProvider _serviceProvider;

        public CardViewFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ICardView Create(Type cardViewType)
        {
            var card = (ICardView)_serviceProvider.GetRequiredService(cardViewType);
            card.AssignInjectAttributeProperties(_serviceProvider);
            return card;
        }
    }
}