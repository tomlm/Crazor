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

        [Binding(BindingType.PropertyName)]
        [HtmlAttributeName]
        public string? Id { get; set; }

        [HtmlAttributeNotBound]
        public CardView View { get; set; }

        public override void Init(TagHelperContext context)
        {
            var viewContext = this.ViewContext;
            while (this.View == null)
            {
                // ((RazorView)page.ViewContext.View).RazorPage;
                if (viewContext.View is RazorView rv)
                {
                    if (rv.RazorPage is CardView cv)
                    {
                        this.View = cv;
                        break;
                    }
                    else
                    {
                        ViewContext = rv.RazorPage.ViewContext;
                    }
                }
                break;
            }

            base.Init(context);
        }


        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var properties = this.GetType().GetProperties()
                .Where(p => p.GetCustomAttribute<HtmlAttributeNameAttribute>(true) != null);
            StringBuilder sb = new StringBuilder();
            foreach (var property in properties)
            {
                string attributeName = property.GetCustomAttribute<HtmlAttributeNameAttribute>()?.Name ?? property.Name;
                var bindValueAttribute = property.GetCustomAttribute<BindingAttribute>();
                var value = property?.GetValue(this);

                if (value != null)
                {
                    var dva = property.GetCustomAttribute<DefaultValueAttribute>();

                    if (dva != null && Object.Equals(value, dva.Value))
                    {
                        continue;
                    }

                    if (property.PropertyType == typeof(bool) || property.PropertyType == typeof(bool?))
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
