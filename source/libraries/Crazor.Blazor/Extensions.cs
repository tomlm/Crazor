// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Crazor.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

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
            services.AddTransient<CardViewFactory, CardViewFactory>();

            // add card view types for razor templates
            foreach (var cardViewType in Utils.GetAssemblies().SelectMany(asm => asm.DefinedTypes
                    .Where(t => t.IsAbstract == false && t.IsAssignableTo(typeof(ICardView)) && t.IsAssignableTo(typeof(ComponentBase)))
                    .Where(t => (t.Name != "CardView" && t.Name != "CardView`1" && t.Name != "CardView`2" && t.Name != "CardViewBase`1" && t.Name != "EmptyCardView"))))
            {

                services.AddTransient(cardViewType);
            }

            return services;
        }
    }
}