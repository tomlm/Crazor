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
    public class InputTagHelper : ReflectionTagHelper
    {
        [HtmlAttributeName]
        public string Binding { get; set; }

        [HtmlAttributeNotBound]
        public object BindingValue { get; set; }

        [HtmlAttributeNotBound]
        public string BindingDisplayName { get; set; }

        public override void Init(TagHelperContext context)
        {
            base.Init(context);

            if (this.View != null)
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
                    var property = this.View.GetType().GetProperty(this.Binding);
                    if (property != null)
                    {
                        this.BindingValue = this.View.GetPropertyValue(this.Binding);
                        var dna = property.GetCustomAttribute<DisplayNameAttribute>();
                        this.BindingDisplayName = dna?.DisplayName ?? Binding;
                    }
                }
            }

            base.Init(context);
        }


        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            await base.ProcessAsync(context, output);

            var properties = this.GetType().GetProperties()
                .Where(p => p.GetCustomAttribute<BindingAttribute>(true) != null);
            StringBuilder sb = new StringBuilder();
            foreach (var property in properties)
            {
                string attributeName = property.GetCustomAttribute<HtmlAttributeNameAttribute>()?.Name ?? property.Name;
                var bindValueAttribute = property.GetCustomAttribute<BindingAttribute>();
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
                    if (property.PropertyType == typeof(bool))
                    {
                        value = value.ToString().ToLower();
                    }

                    TagHelperAttribute tagHelperAttribute;
                    if (output.Attributes.TryGetAttribute(attributeName, out tagHelperAttribute))
                    {
                        output.Attributes.Remove(tagHelperAttribute);
                    }
                    output.Attributes.Add(attributeName, value);
                }
            }

            if (output.Attributes.TryGetAttribute(nameof(Binding), out var binding))
            {
                output.Attributes.Remove(binding);
            }

        }
    }
}
