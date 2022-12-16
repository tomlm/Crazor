// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using AdaptiveCards;
using Crazor.Attributes;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace Crazor.Blazor.Components.AdaptiveCards
{

    /// <summary>
    /// Component for Input.ChoiceSet
    /// </summary>

    public class InputChoiceSet : Input<AdaptiveChoiceSetInput>
    {

        [Parameter]
        public Boolean? IsMultiSelect { get=> Item.IsMultiSelect; set => Item.IsMultiSelect = value ?? false; }

        [Parameter]
        public Boolean? IsVisible { get => Item.IsVisible; set => Item.IsVisible = value ?? true; }

        [Parameter]
        public String Placeholder { get => Item.Placeholder ; set=> Item.Placeholder  = value; }

        [Parameter]
        public Boolean? Separator { get => Item.Separator; set => Item.Separator = value ?? false; }

        [Parameter]
        [DefaultValue(typeof(AdaptiveSpacing), "Default")]
        public AdaptiveSpacing Spacing { get => Item.Spacing; set => Item.Spacing = value; }

        [Parameter]
        [DefaultValue(typeof(AdaptiveChoiceInputStyle), "Compact")]
        public AdaptiveChoiceInputStyle Style { get; set; }

        [Parameter]
        [Binding(BindingType.Value)]
        public String Value { get => Item.Value; set => Item.Value = value; }

        [Parameter]
        public Boolean? Wrap { get=>Item.Wrap; set => Item.Wrap = value ?? false; }

        [Parameter]
        public String Height { get => Item.Height.ToString(); set => Item.Height = value; }

        public override async Task ProcessAsync(ComponentContext context, ComponentOutput output)
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
