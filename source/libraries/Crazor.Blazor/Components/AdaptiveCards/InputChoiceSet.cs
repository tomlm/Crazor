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
        public Boolean? IsMultiSelect { get => Item.IsMultiSelect; set => Item.IsMultiSelect = value ?? false; }

        [Parameter]
        public Boolean? IsVisible { get => Item.IsVisible; set => Item.IsVisible = value ?? true; }

        [Parameter]
        public String Placeholder { get => Item.Placeholder; set => Item.Placeholder = value; }

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
        public Boolean? Wrap { get => Item.Wrap; set => Item.Wrap = value ?? false; }

        [Parameter]
        public String Height { get => Item.Height.ToString(); set => Item.Height = value; }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();


            if (BindingProperty != null)
            {
                var bindingType = BindingProperty.PropertyType;
                if (bindingType.Name == "Nullable`1")
                {
                    bindingType = bindingType.GenericTypeArguments[0];
                }
                if (bindingType.IsEnum)
                {
                    // automatically compute choice from enumeration.
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
                            Item.Choices.Add(new AdaptiveChoice() { Title = descriptionAttribute.Description, Value = value?.ToString() });
                        }
                        else if (displayAttribute != null)
                        {
                            Item.Choices.Add(new AdaptiveChoice() { Title = displayAttribute.DisplayName, Value = value?.ToString() });
                        }
                        else
                        {
                            Item.Choices.Add(new AdaptiveChoice() { Title = value.ToString(), Value = value.ToString() });
                        }
                    }
                }
            }
        }
    }
}
