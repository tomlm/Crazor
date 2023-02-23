// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Crazor.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;

namespace Crazor.Mvc
{
    public static class Extensions
    {
        public static IServiceCollection AddCrazorMvc(this IServiceCollection services)
        {
            services.AddSingleton<MvcCardViewFactory>();

            // enumerates types that are .cshtml templates
            foreach (var cardViewType in Utils.GetAssemblies().SelectMany(asm => asm.DefinedTypes
                    .Where(t => t.IsAbstract == false && t.IsAssignableTo(typeof(ICardView)) && t.IsAssignableTo(typeof(RazorPage)))
                    .Where(t => (t.Name != "CardView" && t.Name != "CardView`1" && t.Name != "CardView`2" && t.Name != "CardViewBase`1" && t.Name != "EmptyCardView"))))
            {
                // we use MvcCardViewFactory as instantiater cause .cshtml templates are instantiated using Razor goo.
                services.AddTransient(cardViewType, (sp) => sp.GetRequiredService<MvcCardViewFactory>().Create(cardViewType));
            }

            // add card home pages support
            var mvcBuilder = services.AddRazorPages()
                 .AddRazorOptions(options =>
                 {
                     options.ViewLocationFormats.Add("/Cards/{0}.cshtml");
                 });

            return services;
        }

    }
}
