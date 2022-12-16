// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using AdaptiveCards;
using Crazor.Attributes;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Crazor.Blazor.Components.AdaptiveCards
{
    /// <summary>
    /// Shows errors when present for a given input id as TextBlock Attention
    /// </summary>
    public class Input<AdaptiveInputT> : ElementComponent<AdaptiveInputT>
        where AdaptiveInputT : AdaptiveInput
    {

        [Parameter]
        public Boolean? IsRequired { get; set; }

        [Parameter]
        [Binding(BindingType.DisplayName)]
        public String Label { get => Item.Label; set => Item.Label = value; }

        /// <summary>
        /// Client side verification error message.
        /// </summary>
        [Parameter]
        public String ErrorMessage { get => Item.ErrorMessage; set => Item.ErrorMessage = value; }

        /// <summary>
        /// Binding proeprty name. Set this to the path of the property to bind this tag to.
        /// </summary>
        [Parameter]
        public string? Binding { get; set; }

        /// <summary>
        /// Set to false to hide the validation errors
        /// </summary>
        [Parameter]
        public Boolean? ShowErrors { get; set; }

        public PropertyInfo? BindingProperty { get; set; }

        public object? BindingValue { get; set; }

        public string? BindingDisplayName { get; set; }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            var id = this.Id ?? this.Binding;

            if (!String.IsNullOrEmpty(this.Binding))
            {
                this.BindingValue = this;
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

            // Process BindingAttributes
            var properties = this.GetType().GetProperties().Where(p => p.GetCustomAttribute<BindingAttribute>(true) != null);
            StringBuilder sb = new StringBuilder();
            foreach (var property in properties)
            {
                string attributeName = property.Name;
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

                    this.SetTargetProperty(property, value);
                }
            }

            // if we don't have required, but binding property has [Required] then set it
            if (this.IsRequired == null && BindingProperty?.GetCustomAttribute<RequiredAttribute>() != null)
            {
                this.IsRequired = true;

                // --- Client side validation....
                var requiredAttribute = BindingProperty?.GetCustomAttribute<RequiredAttribute>();
                if (this.ErrorMessage == null && requiredAttribute?.ErrorMessage != null)
                {
                    this.ErrorMessage = requiredAttribute.ErrorMessage;
                }
            }

            // Add server side error messages.
            if (ShowErrors == null || ShowErrors.Value == true)
            {
                //if (View != null)
                //{
                //    if (View.ValidationErrors.TryGetValue(this.Binding ?? this.Id ?? String.Empty, out var errors))
                //    {
                //        if (errors.Any())
                //        {
                //            sb = new StringBuilder();
                //            sb.AppendLine();
                //            foreach (var error in errors)
                //            {
                //                sb.AppendLine($"<TextBlock Spacing=\"None\" Color=\"Attention\">{error}</TextBlock>");
                //            }
                //            output.PostElement.SetHtmlContent(sb.ToString());
                //        }
                //    }
                //}
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
