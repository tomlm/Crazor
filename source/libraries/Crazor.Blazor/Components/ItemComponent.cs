// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Newtonsoft.Json;
using System.Reflection;

namespace Crazor.Blazor.Components
{
    public class ItemComponent<ItemT> : ComponentBase
    {
        public ItemT Item { get; set; } = Activator.CreateInstance<ItemT>();

        [CascadingParameter(Name = "Parent")]
        protected object Parent { get; set; }

        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        protected Dictionary<string, object> GetAttributes()
        {
            var attributes = new Dictionary<string, object>();
            foreach (var property in this.GetType().GetProperties().Where(p => p.PropertyType != typeof(RenderFragment) && p.GetCustomAttribute<ParameterAttribute>() != null))
            {
                var val = property.GetValue(this);
                if (val != null)
                {
                    attributes.Add(property.Name, val);
                }
            }
            return attributes;
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            int i = 0;
            base.BuildRenderTree(builder);

            // Add the CascadingValue component
            builder.OpenComponent<CascadingValue<object>>(i++);
            builder.AddAttribute(i++, "Name", "Parent");
            builder.AddAttribute(i++, "Value", Item);
            builder.AddAttribute(i++, "ChildContent", (RenderFragment)((builder2) => {
                builder2.AddContent(i++, ChildContent);
            }));
            builder.CloseComponent();

            // Add JSON serialized
            var json = JsonConvert.SerializeObject(Item);
            builder.AddContent(i++, (MarkupString)$"{json},");
        }
    }
}
