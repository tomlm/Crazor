// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Crazor.Attributes;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;

namespace Crazor.Mvc.TagHelpers
{
    /// <summary>
    /// Shows errors when present for a given input id as TextBlock Attention
    /// </summary>
    public class InputTagHelper : ReflectionTagHelper
    {

        [HtmlAttributeName(nameof(IsRequired))]
        public Boolean? IsRequired { get; set; }

        [HtmlAttributeName(nameof(Label))]
        [Binding(BindingType.DisplayName)]
        public String Label { get; set; }

        /// <summary>
        /// Client side verification error message.
        /// </summary>
        [HtmlAttributeName(nameof(ErrorMessage))]
        public String ErrorMessage { get; set; }

        /// <summary>
        /// Binding proeprty name. Set this to the path of the property to bind this tag to.
        /// </summary>
        [HtmlAttributeName(nameof(Binding))]
        [HtmlAttributeIgnore]
        public string? Binding { get; set; }

        /// <summary>
        /// Set to false to hide the validation errors
        /// </summary>
        [HtmlAttributeName(nameof(ShowErrors))]
        [HtmlAttributeIgnore]
        public bool? ShowErrors { get; set; }

        public PropertyInfo? BindingProperty { get; set; }

        public object? BindingValue { get; set; }

        public string? BindingDisplayName { get; set; }

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
                        this.BindingProperty = this.BindingValue?.GetType().GetProperty(part)!;
                        if (this.BindingProperty != null)
                        {
                            this.BindingValue = this.BindingProperty?.GetValue(this.BindingValue)!;
                        }
                        else
                        {
                            throw new Exception($"Invalid Binding='{this.Binding}': property '{part}' does not exist");
                        }
                    }
                    var dnAttr = this.BindingProperty?.GetCustomAttribute<DisplayNameAttribute>();
                    var descAttr = this.BindingProperty?.GetCustomAttribute<DescriptionAttribute>();
                    this.BindingDisplayName = dnAttr?.DisplayName ?? descAttr?.Description ?? MakeTitle(parts.Last());
                }
            }
        }


        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            await base.ProcessAsync(context, output);

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
                    if (property!.PropertyType == typeof(bool))
                    {
                        // xml only likes "true" not "True".
                        value = value.ToString()!.ToLower();
                    }

                    output.Attributes.SetAttribute(attributeName, value);
                }
            }

            // if we don't have required, but binding property has [Required] then set it
            if (output.Attributes[nameof(IsRequired)] == null && BindingProperty?.GetCustomAttribute<RequiredAttribute>() != null)
            {
                output.Attributes.SetAttribute(nameof(IsRequired), "true");

                // --- Client side validation....
                var requiredAttribute = BindingProperty?.GetCustomAttribute<RequiredAttribute>();
                if (output.Attributes[nameof(ErrorMessage)] == null && requiredAttribute?.ErrorMessage != null)
                {
                    output.Attributes.SetAttribute(nameof(ErrorMessage), requiredAttribute?.ErrorMessage);
                }
            }

            // Add server side error messages.
            if (ShowErrors == null || ShowErrors.Value == true)
            {
                if (View != null)
                {
                    if (View.ValidationErrors.TryGetValue(this.Binding ?? this.Id ?? String.Empty, out var errors))
                    {
                        if (errors.Any())
                        {
                            sb = new StringBuilder();
                            sb.AppendLine();
                            foreach (var error in errors)
                            {
                                sb.AppendLine($"<TextBlock Spacing=\"None\" Color=\"Attention\">{error}</TextBlock>");
                            }
                            output.PostElement.SetHtmlContent(sb.ToString());
                        }
                    }
                }
            }
        }

        protected static string MakeTitle(string name)
        {
            if (String.IsNullOrEmpty(name))
            {
                return String.Empty;
            }

            name = $"{Char.ToUpper(name[0])}{name.Substring(1)}";
            StringBuilder sb = new StringBuilder();
            bool isLower = false;
            bool endIsSpace = false;
            foreach (var ch in name)
            {
                if (isLower && Char.IsUpper(ch))
                {
                    sb.Append($" {ch}");
                }
                else if (Char.IsLetterOrDigit(ch))
                {
                    if (endIsSpace)
                        sb.Append(Char.ToUpper(ch));
                    else
                        sb.Append(ch);
                    endIsSpace = false;
                }
                else if (!sb.ToString().EndsWith(' '))
                {
                    sb.Append(' ');
                    endIsSpace = true;
                }
                isLower = Char.IsLower(ch);
            }

            return sb.ToString();
        }

    }
}
