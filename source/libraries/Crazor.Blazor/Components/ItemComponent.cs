// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Microsoft.AspNetCore.Components;
using System.Reflection;

namespace Crazor.Blazor.Components
{
    public class ItemComponent<ModelT> : ComponentBase
    {
        public ModelT Model { get; set; } = Activator.CreateInstance<ModelT>();

        [CascadingParameter(Name = "ParentModel")]
        protected object ParentModel { get; set; }

        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        protected virtual Dictionary<string, object> GetAttributes()
        {
            var attributes = new Dictionary<string, object>();
            foreach (var property in this.GetType().GetProperties().Where(p => p.PropertyType != typeof(RenderFragment) && p.GetCustomAttribute<ParameterAttribute>() != null))
            {
                var val = property.GetValue(this);
                var targetProperty = Model!.GetType().GetProperty(property.Name) ?? property;
                var defValue = (targetProperty.PropertyType.IsValueType) ? Activator.CreateInstance(targetProperty.PropertyType) : null;
                if (val != null && !Object.Equals(val, defValue))
                {
                    if (val is bool b)
                        // HTML serialization used by Blazor will turn raw bool into just attribute name with no value
                        // we are targeting XML which always must have a value, and bool is represented as string "true"|"false"
                        attributes.Add(property.Name, b.ToString().ToLower());
                    else
                        attributes.Add(property.Name, val);
                }
            }
            return attributes;
        }

        //protected override void BuildRenderTree(RenderTreeBuilder builder)
        //{
        //    int i = 0;
        //    base.BuildRenderTree(builder);

        //    // Add the CascadingValue component
        //    builder.OpenComponent<CascadingValue<object>>(i++);
        //    builder.AddAttribute(i++, "Name", "Parent");
        //    builder.AddAttribute(i++, "Value", Item);
        //    builder.AddAttribute(i++, "ChildContent", (RenderFragment)((builder2) => {
        //        builder2.AddContent(i++, ChildContent);
        //    }));
        //    builder.CloseComponent();

        //    // Add JSON serialized
        //    var json = JsonConvert.SerializeObject(Item);
        //    builder.AddContent(i++, (MarkupString)$"{json},");
        //}
    }
}
