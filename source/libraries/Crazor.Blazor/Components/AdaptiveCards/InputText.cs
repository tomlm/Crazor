// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using AdaptiveCards;
using Crazor.Attributes;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Crazor.Blazor.Components.AdaptiveCards
{

    /// <summary>
    /// Component for Input.Text
    /// </summary>

    public class InputText : Input<AdaptiveTextInput>
    {

        [Parameter]
        public Boolean? IsMultiline { get => Item.IsMultiline; set => Item.IsMultiline = value ?? false; }

        [Parameter]
        public Boolean? IsVisible { get => Item.IsVisible; set => Item.IsVisible = value ?? true; }

        [Parameter]
        public Int32? MaxLength { get => Item.MaxLength; set => Item.MaxLength = value ?? 0; }

        [Parameter]
        public String Placeholder { get => Item.Placeholder ; set=> Item.Placeholder  = value; }

        [Parameter]
        public String Regex { get => Item.Regex; set => Item.Regex = value; }

        [Parameter]
        public Boolean? Separator { get => Item.Separator; set => Item.Separator = value ?? false; }

        [Parameter]
        [DefaultValue(typeof(AdaptiveSpacing), "Default")]
        public AdaptiveSpacing Spacing { get => Item.Spacing; set => Item.Spacing = value; }

        [Parameter]
        [DefaultValue(typeof(AdaptiveTextInputStyle), "Text")]
        public AdaptiveTextInputStyle Style { get; set; }

        [Parameter]
        [Binding(BindingType.Value)]
        public String Value { get => Item.Value; set => Item.Value = value; }

        [Parameter]
        public String Height { get => Item.Height.ToString(); set => Item.Height = value; }

        public override async Task ProcessAsync(ComponentContext context, ComponentOutput output)
        {
            await base.ProcessAsync(context, output);

            // --- Client side validation....
            var regexAttribute = BindingProperty?.GetCustomAttribute<RegularExpressionAttribute>();
            if (output.Attributes[nameof(Regex)] == null && regexAttribute?.Pattern != null)
            {
                output.Attributes.SetAttribute(nameof(Regex), regexAttribute?.Pattern);

            }
            if (output.Attributes[nameof(ErrorMessage)] == null && regexAttribute?.ErrorMessage != null)
            {
                output.Attributes.SetAttribute(nameof(ErrorMessage), regexAttribute?.ErrorMessage);
            }

            // -- MaxLength 
            var stringLengthAttribute = BindingProperty?.GetCustomAttribute<StringLengthAttribute>();
            if (output.Attributes[nameof(MaxLength)] == null && stringLengthAttribute != null)
            {
                output.Attributes.SetAttribute(nameof(MaxLength), stringLengthAttribute?.MaximumLength);
            }

            // ----  style
            var maxLengthAttribute = BindingProperty?.GetCustomAttribute<MaxLengthAttribute>();
            if (output.Attributes[nameof(MaxLength)] == null && maxLengthAttribute != null)
            {
                output.Attributes.SetAttribute(nameof(MaxLength), maxLengthAttribute?.Length);
            }

            var phoneAttribute = BindingProperty?.GetCustomAttribute<PhoneAttribute>();
            if (output.Attributes[nameof(Style)] == null && phoneAttribute != null)
            {
                output.Attributes.SetAttribute(nameof(Style), AdaptiveTextInputStyle.Tel);
            }

            var emailAttribute = BindingProperty?.GetCustomAttribute<EmailAddressAttribute>();
            if (output.Attributes[nameof(Style)] == null && emailAttribute != null)
            {
                output.Attributes.SetAttribute(nameof(Style), AdaptiveTextInputStyle.Email);
            }

            var passwordAttribute = BindingProperty?.GetCustomAttribute<PasswordPropertyTextAttribute>();
            if (output.Attributes[nameof(Style)] == null && passwordAttribute != null)
            {
                output.Attributes.SetAttribute(nameof(Style), AdaptiveTextInputStyle.Password);
            }

            var urlAttribute = BindingProperty?.GetCustomAttribute<UrlAttribute>();
            if (output.Attributes[nameof(Style)] == null && urlAttribute != null)
            {
                output.Attributes.SetAttribute(nameof(Style), AdaptiveTextInputStyle.Url);
            }

            var dtAttribute = BindingProperty?.GetCustomAttribute<DataTypeAttribute>();
            if (output.Attributes[nameof(Style)] == null && dtAttribute != null)
            {
                switch (dtAttribute.DataType)
                {
                    case DataType.Text:
                        output.Attributes.SetAttribute(nameof(Style), AdaptiveTextInputStyle.Text);
                        break;
                    case DataType.EmailAddress:
                        output.Attributes.SetAttribute(nameof(Style), AdaptiveTextInputStyle.Email);
                        break;
                    case DataType.PhoneNumber:
                        output.Attributes.SetAttribute(nameof(Style), AdaptiveTextInputStyle.Tel);
                        break;
                    case DataType.Password:
                        output.Attributes.SetAttribute(nameof(Style), AdaptiveTextInputStyle.Password);
                        break;
                    case DataType.Url:
                        output.Attributes.SetAttribute(nameof(Style), AdaptiveTextInputStyle.Url);
                        break;
                    case DataType.Date:
                    case DataType.DateTime:
                    case DataType.Time:
                    case DataType.CreditCard:
                    case DataType.Currency:
                    case DataType.PostalCode:
                        break;
                }
            }
        }
    }
}
