// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using AdaptiveCards;
using Crazor.Attributes;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace Crazor.TagHelpers
{

    /// <summary>
    /// TagHelper for Input.ChoiceSet
    /// </summary>

    [HtmlTargetElement("Input.ChoiceSet")]
    [RestrictChildren("Choice", "Data.Query")]
    public class InputChoiceSetTagHelper : InputTagHelper
    {

        [HtmlAttributeName(nameof(IsMultiSelect))]
        public Boolean? IsMultiSelect { get; set; }

        [HtmlAttributeName(nameof(IsVisible))]
        public Boolean? IsVisible { get; set; }

        [HtmlAttributeName(nameof(Placeholder))]
        public String Placeholder { get; set; }

        [HtmlAttributeName(nameof(Separator))]
        public Boolean? Separator { get; set; }

        [HtmlAttributeName(nameof(Spacing))]
        [DefaultValue(typeof(AdaptiveSpacing), "Default")]
        public AdaptiveSpacing Spacing { get; set; }

        [HtmlAttributeName(nameof(Speak))]
        public String Speak { get; set; }

        [HtmlAttributeName(nameof(Style))]
        [DefaultValue(typeof(AdaptiveChoiceInputStyle), "Compact")]
        public AdaptiveChoiceInputStyle Style { get; set; }

        [HtmlAttributeName(nameof(Value))]
        [Binding(BindingType.Value)]
        public String Value { get; set; }

        [HtmlAttributeName(nameof(Wrap))]
        public Boolean? Wrap { get; set; }

        [HtmlAttributeName(nameof(Height))]
        public String Height { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            await base.ProcessAsync(context, output);
            
            if (BindingProperty != null)
            {
                var bindingType = BindingProperty.PropertyType;
                if (bindingType.Name == "Nullable`1")
                {
                    bindingType = bindingType.GenericTypeArguments[0];
                }
                if (bindingType.IsEnum)
                {
                    var childContent = await output.GetChildContentAsync();
                    if (!childContent.GetContent().TrimStart().StartsWith("<Choice"))
                    {
                        // automatically compute choice from enumeration.
                        StringBuilder sb = new StringBuilder();
                        output.TagMode = TagMode.StartTagAndEndTag;
                        foreach (var value in bindingType.GetEnumValues())
                        {
                            MemberInfo memberInfo = bindingType.GetMember(value.ToString()!).First();

                            // we can then attempt to retrieve the    
                            // description attribute from the member info    
                            var descriptionAttribute = memberInfo.GetCustomAttribute<DescriptionAttribute>();
                            var displayAttribute = memberInfo.GetCustomAttribute<DisplayNameAttribute>();
                            // if we find the attribute we can access its values    
                            if (descriptionAttribute != null)
                            {
                                sb.AppendLine($"<Choice Title=\"{descriptionAttribute.Description}\" Value=\"{value}\"/>");
                            }
                            else if (displayAttribute != null)
                            {
                                sb.AppendLine($"<Choice Title=\"{displayAttribute.DisplayName}\" Value=\"{value}\"/>");
                            }
                            else
                            {
                                sb.AppendLine($"<Choice Title=\"{value}\" Value=\"{value}\"/>");
                            }
                        }

                        output.Content.SetHtmlContent(sb.ToString());
                    }
                }
            }
        }

    }
}
