#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Microsoft.AspNetCore.Razor.TagHelpers;
using AdaptiveCards;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using Crazor.Attributes;
using DataAnnotationsValidator;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace Crazor.TagHelpers
{
    /// <summary>
    /// Shows errors when present for a given input id as TextBlock Attention
    /// </summary>
    public class ReflectionTagHelper : TagHelper
    {
        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext? ViewContext { get; set; }

        public TagHelperContent? ChildContent { get; set; }

        [BindValue(BindingType.PropertyName)]
        [HtmlAttributeName]
        public string? Id { get; set; }

        [HtmlAttributeName]
        public string Binding { get; set; }

        [HtmlAttributeNotBound]
        public object BindingValue { get; set; }

        [HtmlAttributeNotBound]
        public string BindingDisplayName { get; set; }

        public CardView View { get; set; }

        public override void Init(TagHelperContext context)
        {
            CardView cardView = null;
            var viewContext = this.ViewContext;
            while (cardView == null)
            {
                // ((RazorView)page.ViewContext.View).RazorPage;
                if (viewContext.View is RazorView rv)
                {
                    if (rv.RazorPage is CardView cv)
                    {
                        cardView = cv;
                        break;
                    }
                    else
                    {
                        ViewContext = rv.RazorPage.ViewContext;
                    }
                }
                break;
            }

            if (cardView != null)
            {
                var id = this.Id ?? this.Binding;
                //if (!String.IsNullOrEmpty(id))
                //{
                //    if (cardView.ValidationErrors.TryGetValue(id, out var errors))
                //    {
                //        this.ValidationErrors = errors.ToArray<string>();
                //    }
                //}

                if (!String.IsNullOrEmpty(this.Binding))
                {
                    var property = cardView.GetType().GetProperty(this.Binding);
                    if (property != null)
                    {
                        this.BindingValue = cardView.GetPropertyValue(this.Binding);
                        var dna = property.GetCustomAttribute<DisplayNameAttribute>();
                        this.BindingDisplayName = dna?.DisplayName ?? Binding;
                    }
                }
            }

            base.Init(context);
        }


        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var properties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.Name != nameof(Binding))
                .Where(p => p.GetCustomAttribute<HtmlAttributeNameAttribute>(true) != null);
            StringBuilder sb = new StringBuilder();
            foreach (var property in properties)
            {
                string attributeName = property.GetCustomAttribute<HtmlAttributeNameAttribute>()?.Name ?? property.Name;
                var bindValueAttribute = property.GetCustomAttribute<BindValueAttribute>();
                var value = property?.GetValue(this);
                if (value == null && Binding != null && bindValueAttribute != null)
                {
                    switch (bindValueAttribute.Policy)
                    {
                        case BindingType.PropertyName:
                            value = Binding;
                            break;
                        case BindingType.DisplayName:
                            value = BindingDisplayName;
                            break;
                        case BindingType.Value:
                            value = BindingValue;
                            break;
                    }
                }

                if (value != null)
                {
                    var dva = property.GetCustomAttribute<DefaultValueAttribute>();

                    if (dva != null && Object.Equals(value, dva.Value))
                    {
                        continue;
                    }

                    if (property.PropertyType == typeof(bool))
                    {
                        output.Attributes.Add(attributeName, value.ToString().ToLower());
                    }
                    else
                    {
                        output.Attributes.Add(attributeName, value.ToString());
                    }
                }
            }
        }
    }
}
