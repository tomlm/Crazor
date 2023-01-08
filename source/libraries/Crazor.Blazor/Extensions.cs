// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Crazor.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Crazor.Blazor
{
    public static class Extensions
    {
        /// <summary>
        /// Add Crazor Dependencies.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddCrazorBlazor(this IServiceCollection services)
        {
            var cardViewTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(asm => asm.DefinedTypes
                    .Where(t => t.IsAbstract == false && t.IsAssignableTo(typeof(ICardView)) && t.IsAssignableTo(typeof(ComponentBase)))
                    .Where(t => (t.Name != "CardView" && t.Name != "CardView`1" && t.Name != "CardView`2" && t.Name != "CardViewBase`1" && t.Name != "EmptyCardView"))).ToList();

            // add CardViews 
            foreach (var cardViewType in cardViewTypes)
            {
                services.AddCardView(cardViewType);
            }

            return services;
        }

    }
}