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
using System.Xml.Serialization;
using Newtonsoft.Json;

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

        [HtmlAttributeName]
        [HtmlAttributeIgnore]
        public string Class { get; set; }

        [HtmlAttributeNotBound]
        public CardView View { get; set; }

        public override void Init(TagHelperContext context)
        {
            var viewContext = this.ViewContext;
            while (this.View == null)
            {
                // ((RazorView)page.ViewContext.View).RazorPage;
                if (viewContext!.View is RazorView rv)
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
            await Task.CompletedTask;

            // process class
            if (View?.App?.Stylesheet != null && this.Class != null)
            {
                var targetElement = this.GetType().GetCustomAttribute<HtmlTargetElementAttribute>();
                if (targetElement != null)
                {
                    foreach (var id in this.Class.Split(' '))
                    {
                        if (View.App.Stylesheet.TryGetValue($"{targetElement.Tag}.{id}", out var el))
                        {
                            foreach (var property in el.GetType().GetProperties().Where(p => p.Name != "Id" && p.GetCustomAttribute<XmlAttributeAttribute>() != null))
                            {
                                var xmlAttribute = property.GetCustomAttribute<XmlAttributeAttribute>()!;
                                var propertyName = String.IsNullOrEmpty(xmlAttribute.AttributeName) ? property.Name : xmlAttribute.AttributeName;
                                var value = property.GetValue(el);
                                if (value != null)
                                {
                                    var defaultAttribute = property.GetCustomAttribute<DefaultValueAttribute>();
                                    if (defaultAttribute != null)
                                    {
                                        if (property.PropertyType == typeof(bool))
                                        {
                                            if ((bool)defaultAttribute.Value! != Convert.ToBoolean(value))
                                            {
                                                output.Attributes.SetAttribute(propertyName, value.ToString()!.ToLower());
                                            }
                                        }
                                        else if (property.PropertyType == typeof(int) ||
                                                 property.PropertyType == typeof(uint))
                                        {
                                            if (Convert.ToInt64(defaultAttribute.Value) != Convert.ToInt64(value))
                                            {
                                                output.Attributes.SetAttribute(propertyName, value.ToString());
                                            }
                                        }
                                        else if (property.PropertyType == typeof(float) ||
                                                 property.PropertyType == typeof(double))
                                        {
                                            if (Convert.ToDouble(defaultAttribute.Value) != Convert.ToDouble(value))
                                            {
                                                output.Attributes.SetAttribute(propertyName, value.ToString());
                                            }
                                        }
                                        else if (property.PropertyType == typeof(String) )
                                        {
                                            if (!String.IsNullOrEmpty(value.ToString()))
                                            {
                                                output.Attributes.SetAttribute(propertyName, value.ToString());
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            var properties = this.GetType().GetProperties()
                .Where(p => p.GetCustomAttribute<HtmlAttributeNameAttribute>(true) != null &&
                            p.GetCustomAttribute<HtmlAttributeIgnoreAttribute>() == null);
            StringBuilder sb = new StringBuilder();
            foreach (var property in properties)
            {
                string attributeName = property.GetCustomAttribute<HtmlAttributeNameAttribute>()?.Name ?? property.Name;
                var bindValueAttribute = property.GetCustomAttribute<BindingAttribute>();
                var value = property.GetValue(this);

                if (value != null)
                {
                    var dva = property.GetCustomAttribute<DefaultValueAttribute>();

                    if (dva != null && Object.Equals(value, dva.Value))
                    {
                        continue;
                    }

                    if (property.PropertyType == typeof(bool) || property.PropertyType == typeof(bool?))
                    {
                        output.Attributes.SetAttribute(attributeName, value.ToString()!.ToLower());
                    }
                    else
                    {
                        output.Attributes.SetAttribute(attributeName, value.ToString());
                    }
                }
            }
        }
    }
}
