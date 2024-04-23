// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using AdaptiveCards;
using Crazor.Attributes;
using Crazor.Blazor.Components.Adaptive;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;
using System.Reflection;
using System.Text;


namespace Crazor.Blazor.Components
{
    /// <summary>
    /// Shows errors when present for a given input id as TextBlock Attention
    /// </summary>
    public class ElementComponent<ElementT> : TypedElementComponent<ElementT>, IChildItem
        where ElementT : AdaptiveElement
    {
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

        public virtual void AddToParent()
        {
            ArgumentNullException.ThrowIfNull(ParentItem);

            if (this.ParentItem is AdaptiveCard card)
            {
                card.Body.Add(this.Item);
            }
            else if (ParentItem is AdaptiveShowCardAction showCard && this.Item is AdaptiveCard ac)
            {
                showCard.Card = ac;
            }
            else if (Item is AdaptiveTableCell cell && this.ParentItem is AdaptiveTableRow row)
            {
                row.Cells.Add(cell);
            }
            else if (Item is AdaptiveColumn col && this.ParentItem is AdaptiveColumnSet colSet)
            {
                colSet.Columns.Add(col);
            }
            else if (Item is AdaptiveImage image && this.ParentItem is AdaptiveImageSet imageSet)
            {
                imageSet.Images.Add(image);
            }
            else if (this.ParentItem is AdaptiveContainer container)
            {
                container.Items.Add(this.Item);
            }
            else if (this.ParentItem is AdaptiveColumn column)
            {
                column.Items.Add(this.Item);
            }
            else if (this.ParentItem is AdaptiveTableCell cellParent)
            {
                cellParent.Items.Add(this.Item);
            }
            else
            {
                throw new Exception($"{ParentItem.GetType().Name} is not a known element container type");
            }

        }
    }
}
