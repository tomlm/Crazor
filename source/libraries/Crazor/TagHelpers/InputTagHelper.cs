#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using Crazor.Attributes;
using DataAnnotationsValidator;

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

            // remove binding, because we don't want it emitted, we are going to process it here.
            if (output.Attributes.TryGetAttribute(nameof(Binding), out var binding))
            {
                output.Attributes.Remove(binding);
            }

            var properties = this.GetType().GetProperties()
                .Where(p => p.GetCustomAttribute<BindingAttribute>(true) != null);
            StringBuilder sb = new StringBuilder();
            foreach (var property in properties)
            {
                string attributeName = property.GetCustomAttribute<HtmlAttributeNameAttribute>()?.Name ?? property.Name;
                // if there is an BindingAttribute, then we change the value to be the appropriate value
                // One of: PropertyName | DisplayName | the property value
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

                // only emit values that we have
                if (value != null)
                {
                    if (property.PropertyType == typeof(bool))
                    {
                        // xml only likes "true" not "True".
                        value = value.ToString().ToLower();
                    }

                    TagHelperAttribute tagHelperAttribute;
                    // remove the attribute if it's already been emitted by the base class, so we can add the right value.
                    if (output.Attributes.TryGetAttribute(attributeName, out tagHelperAttribute))
                    {
                        output.Attributes.Remove(tagHelperAttribute);
                    }

                    // add the binding value.
                    output.Attributes.Add(attributeName, value);
                }
            }
        }
    }
}
