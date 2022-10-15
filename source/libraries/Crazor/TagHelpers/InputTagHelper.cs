﻿#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using Crazor.Attributes;
using DataAnnotationsValidator;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Crazor.TagHelpers
{
    /// <summary>
    /// Shows errors when present for a given input id as TextBlock Attention
    /// </summary>
    public class InputTagHelper : ReflectionTagHelper
    {

        [HtmlAttributeName(nameof(IsRequired))]
        [DefaultValue(false)]
        public Boolean IsRequired { get; set; }

        [HtmlAttributeName(nameof(Label))]
        [DefaultValue(null)]
        [Binding(BindingType.DisplayName)]
        public String Label { get; set; }

        [HtmlAttributeName(nameof(ErrorMessage))]
        [DefaultValue(null)]
        public String ErrorMessage { get; set; }


        [HtmlAttributeName]
        public string Binding { get; set; }

        public PropertyInfo BindingProperty { get; set; }

        public object BindingValue { get; set; }

        public string BindingDisplayName { get; set; }

        public override void Init(TagHelperContext context)
        {
            base.Init(context);

            if (this.View != null)
            {
                var id = this.Id ?? this.Binding;

                if (!String.IsNullOrEmpty(this.Binding))
                {
                    this.BindingValue = this.View;
                    var parts = this.Binding.Split('.');
                    foreach (var part in parts)
                    {
                        this.BindingProperty = this.BindingValue?.GetType().GetProperty(part);
                        if (this.BindingProperty != null)
                        {
                            this.BindingValue = this.BindingProperty?.GetValue(this.BindingValue);
                        }
                        else
                        {
                            throw new ArgumentNullException($"Could not find property path {part} of {this.Binding}");
                        }
                    }
                    var dna = this.BindingProperty.GetCustomAttribute<DisplayNameAttribute>();
                    this.BindingDisplayName = dna?.DisplayName ?? parts.Last();
                }
            }
        }


        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            await base.ProcessAsync(context, output);

            // remove binding, because we don't want it emitted, we are going to process it here.
            if (output.Attributes.TryGetAttribute(nameof(Binding), out var binding))
            {
                output.Attributes.Remove(binding);
            }

            // Process BindingAttributes
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

                    if (output.Attributes.TryGetAttribute(attributeName, out var att))
                    {
                        output.Attributes.Remove(att);
                    }
                    output.Attributes.Add(attributeName, value);
                }
            }

            // if we don't have required, but binding property has [Required] then set it
            if (output.Attributes[nameof(IsRequired)] == null && BindingProperty.GetCustomAttribute<RequiredAttribute>() != null)
            {
                output.Attributes.SetAttribute(nameof(IsRequired), "true");
            }

        }
    }
}
