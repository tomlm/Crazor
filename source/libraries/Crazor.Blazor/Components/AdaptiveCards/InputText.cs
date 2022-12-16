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
        public String Placeholder { get => Item.Placeholder; set => Item.Placeholder = value; }

        [Parameter]
        public String Regex { get => Item.Regex; set => Item.Regex = value; }

        [Parameter]
        public Boolean? Separator { get => Item.Separator; set => Item.Separator = value ?? false; }

        [Parameter]
        [DefaultValue(typeof(AdaptiveSpacing), "Default")]
        public AdaptiveSpacing Spacing { get => Item.Spacing; set => Item.Spacing = value; }

        [Parameter]
        [DefaultValue(typeof(AdaptiveTextInputStyle), "Text")]
        public AdaptiveTextInputStyle? Style { get => Item.Style == AdaptiveTextInputStyle.Text ? null : Item.Style; set => Item.Style = value ?? AdaptiveTextInputStyle.Text; }

        [Parameter]
        [Binding(BindingType.Value)]
        public String Value { get => Item.Value; set => Item.Value = value; }

        [Parameter]
        public String Height { get => Item.Height.ToString(); set => Item.Height = value; }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            // --- Client side validation....
            var regexAttribute = BindingProperty?.GetCustomAttribute<RegularExpressionAttribute>();
            if (this.Regex == null && regexAttribute?.Pattern != null)
            {
                this.Regex = regexAttribute?.Pattern;

            }

            if (this.ErrorMessage == null && regexAttribute?.ErrorMessage != null)
            {
                this.ErrorMessage = regexAttribute?.ErrorMessage;
            }

            // -- MaxLength 
            var stringLengthAttribute = BindingProperty?.GetCustomAttribute<StringLengthAttribute>();
            if (this.MaxLength == null && stringLengthAttribute != null)
            {
                this.MaxLength = stringLengthAttribute?.MaximumLength;
            }

            // ----  style
            var maxLengthAttribute = BindingProperty?.GetCustomAttribute<MaxLengthAttribute>();
            if (this.MaxLength == null && maxLengthAttribute != null)
            {
                this.MaxLength = maxLengthAttribute?.Length;
            }

            var phoneAttribute = BindingProperty?.GetCustomAttribute<PhoneAttribute>();
            if (this.Style == null && phoneAttribute != null)
            {
                this.Style = AdaptiveTextInputStyle.Tel;
            }

            var emailAttribute = BindingProperty?.GetCustomAttribute<EmailAddressAttribute>();
            if (this.Style == null && emailAttribute != null)
            {
                this.Style = AdaptiveTextInputStyle.Email;
            }

            var passwordAttribute = BindingProperty?.GetCustomAttribute<PasswordPropertyTextAttribute>();
            if (this.Style == null && passwordAttribute != null)
            {
                this.Style = AdaptiveTextInputStyle.Password;
            }

            var urlAttribute = BindingProperty?.GetCustomAttribute<UrlAttribute>();
            if (this.Style == null && urlAttribute != null)
            {
                this.Style = AdaptiveTextInputStyle.Url;
            }

            var dtAttribute = BindingProperty?.GetCustomAttribute<DataTypeAttribute>();
            if (this.Style == null && dtAttribute != null)
            {
                switch (dtAttribute.DataType)
                {
                    case DataType.Text:
                        this.Style = AdaptiveTextInputStyle.Text;
                        break;
                    case DataType.EmailAddress:
                        this.Style = AdaptiveTextInputStyle.Email;
                        break;
                    case DataType.PhoneNumber:
                        this.Style = AdaptiveTextInputStyle.Tel;
                        break;
                    case DataType.Password:
                        this.Style = AdaptiveTextInputStyle.Password;
                        break;
                    case DataType.Url:
                        this.Style = AdaptiveTextInputStyle.Url;
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
