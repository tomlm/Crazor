// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using AdaptiveCards;
using Crazor.Attributes;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;
using System.Reflection;
using System.Text;


namespace Crazor.Blazor.Components
{
    /// <summary>
    /// Shows errors when present for a given input id as TextBlock Attention
    /// </summary>
    public class ElementComponent<ElementT> : TypedElementComponent<ElementT>
        where ElementT : AdaptiveElement
    {
        //[Parameter]
        //public string Class { get => Element.Class; set => Element.Class = value; }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            var properties = this.GetType().GetProperties().Where(p => p.GetCustomAttribute<ParameterAttribute>() != null);
            StringBuilder sb = new StringBuilder();
            foreach (var property in properties)
            {
                var bindValueAttribute = property.GetCustomAttribute<BindingAttribute>();
                var value = property.GetValue(this);

                if (value != null)
                {
                    var dva = property.GetCustomAttribute<DefaultValueAttribute>();

                    if (dva != null && Object.Equals(value, dva.Value))
                    {
                        continue;
                    }

                    //if (property.PropertyType == typeof(bool) || property.PropertyType == typeof(bool?))
                    //{
                    //    output.Attributes.SetAttribute(attributeName, value.ToString()!.ToLower());
                    //}
                    //else
                    //{
                    //    output.Attributes.SetAttribute(attributeName, value.ToString());
                    //}
                }
            }
        }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);

            if (this.ParentModel is AdaptiveCard card)
            {
                card.Body.Add(this.Model);
            }
            else if (this.ParentModel is AdaptiveContainer container)
            {
                container.Items.Add(this.Model);
            }
            else if (Model is AdaptiveTableCell cell && this.ParentModel is AdaptiveTableRow row)
            {
                row.Cells.Add(cell);
            }
            else if (Model is AdaptiveColumn col && this.ParentModel is AdaptiveColumnSet colSet )
            {
                colSet.Columns.Add(col);
            }
            else
            {
                throw new Exception($"{ParentModel.GetType().Name} is not a known element container type");
            }
        }
    }
}
