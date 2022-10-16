
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Microsoft.AspNetCore.Razor.TagHelpers;
using AdaptiveCards;
using System;
using System.ComponentModel;
using Crazor.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Crazor.TagHelpers
{

    /// <summary>
    /// TagHelper for Input.Text
    /// </summary>

    [HtmlTargetElement("Input.Text")]
    public class InputTextTagHelper : InputTagHelper
    {

        [HtmlAttributeName(nameof(IsMultiline))]
        public Boolean? IsMultiline { get; set; }

        [HtmlAttributeName(nameof(IsVisible))]
        public Boolean? IsVisible { get; set; } 

        [HtmlAttributeName(nameof(MaxLength))]
        public Int32? MaxLength { get; set; }

        [HtmlAttributeName(nameof(Placeholder))]
        [DefaultValue(null)]
        public String Placeholder { get; set; }

        [HtmlAttributeName(nameof(Regex))]
        [DefaultValue(null)]
        public String Regex { get; set; }

        [HtmlAttributeName(nameof(Separator))]
        public Boolean? Separator { get; set; }

        [HtmlAttributeName(nameof(Spacing))]
        [DefaultValue(typeof(AdaptiveSpacing), "Default")]
        public AdaptiveSpacing Spacing { get; set; }

        [HtmlAttributeName(nameof(Speak))]
        [DefaultValue(null)]
        public String Speak { get; set; }

        [HtmlAttributeName(nameof(Style))]
        [DefaultValue(typeof(AdaptiveTextInputStyle), "Text")]
        public AdaptiveTextInputStyle Style { get; set; }

        [HtmlAttributeName(nameof(Value))]
        [DefaultValue(null)]
        [Binding(BindingType.Value)]
        public String Value { get; set; }

        [HtmlAttributeName(nameof(Height))]
        [DefaultValue(null)]
        public String Height { get; set; }


        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            await base.ProcessAsync(context, output);

            // --- Client side validation....
            var regexAttribute = BindingProperty.GetCustomAttribute<RegularExpressionAttribute>();
            if (IfValidation() && output.Attributes[nameof(Regex)] == null && regexAttribute?.Pattern != null)
            {
                output.Attributes.SetAttribute(nameof(Regex), regexAttribute?.Pattern);

                if (output.Attributes[nameof(ErrorMessage)] == null && regexAttribute?.ErrorMessage != null)
                {
                    output.Attributes.SetAttribute(nameof(ErrorMessage), regexAttribute?.ErrorMessage);
                }
            }

            // -- MaxLength 
            var stringLengthAttribute = BindingProperty.GetCustomAttribute<StringLengthAttribute>();
            if (IfValidation() && output.Attributes[nameof(MaxLength)] == null && stringLengthAttribute != null)
            {
                output.Attributes.SetAttribute(nameof(MaxLength), stringLengthAttribute?.MaximumLength);
            }

            // ----  style
            var maxLengthAttribute = BindingProperty.GetCustomAttribute<MaxLengthAttribute>();
            if (output.Attributes[nameof(MaxLength)] == null && maxLengthAttribute != null)
            {
                output.Attributes.SetAttribute(nameof(MaxLength), maxLengthAttribute?.Length);
            }

            var phoneAttribute = BindingProperty.GetCustomAttribute<PhoneAttribute>();
            if (output.Attributes[nameof(Style)] == null && phoneAttribute != null)
            {
                output.Attributes.SetAttribute(nameof(Style), AdaptiveTextInputStyle.Tel);
            }

            var emailAttribute = BindingProperty.GetCustomAttribute<EmailAddressAttribute>();
            if (output.Attributes[nameof(Style)] == null && emailAttribute != null)
            {
                output.Attributes.SetAttribute(nameof(Style), AdaptiveTextInputStyle.Email);
            }

            var passwordAttribute = BindingProperty.GetCustomAttribute<PasswordPropertyTextAttribute>();
            if (output.Attributes[nameof(Style)] == null && passwordAttribute != null)
            {
                output.Attributes.SetAttribute(nameof(Style), AdaptiveTextInputStyle.Password);
            }

            var urlAttribute = BindingProperty.GetCustomAttribute<UrlAttribute>();
            if (output.Attributes[nameof(Style)] == null && urlAttribute != null)
            {
                output.Attributes.SetAttribute(nameof(Style), AdaptiveTextInputStyle.Url);
            }

            var dtAttribute = BindingProperty.GetCustomAttribute<DataTypeAttribute>();
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
