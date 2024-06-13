using Crazor.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Crazor.Blazor
{
    public static class Extensions
    {
        /// <summary>
        /// Add Crazor.Blazor Dependencies.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddCrazorBlazor(this IServiceCollection services)
        {
            // add card view types for razor templates
            foreach (var cardViewType in Utils.GetAssemblies().SelectMany(asm => asm.DefinedTypes
                    .Where(t => t.IsAbstract == false && t.IsAssignableTo(typeof(ICardView)) && t.IsAssignableTo(typeof(ComponentBase)))
                    .Where(t => (t.Name != "CardView" && t.Name != "CardView`1" && t.Name != "CardView`2" && t.Name != "CardViewBase`1" && t.Name != "EmptyCardView"))))
            {

                services.AddTransient(cardViewType);
            }

            return services;
        }

        /// <summary>
        /// Use Crazor.Blazor dependencies
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseCrazorBlazor(this IApplicationBuilder builder)
        {
            return builder;
        }


        /// <summary>
        /// Add Crazor components to Razor Components
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static RazorComponentsEndpointConventionBuilder AddCrazorComponents(this RazorComponentsEndpointConventionBuilder builder)
            => builder.AddAdditionalAssemblies(typeof(Crazor.Blazor.Pages.Cards).Assembly);
    }
}