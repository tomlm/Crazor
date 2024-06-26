using Microsoft.AspNetCore.Components;
using System.Reflection;

namespace Crazor.Blazor.Components
{
    public class ItemComponent<ItemT> : ComponentBase
    {
        public ItemT Item { get; set; } = Activator.CreateInstance<ItemT>();

        [CascadingParameter(Name = "ParentItem")]
        protected object ParentItem { get; set; }

        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        protected virtual Dictionary<string, object> GetAttributes()
        {
            var attributes = new Dictionary<string, object>();

            foreach (var property in this.GetType().GetProperties().Where(p => p.PropertyType != typeof(RenderFragment) && p.GetCustomAttribute<ParameterAttribute>() != null))
            {
                var val = property.GetValue(this);
                var targetProperty = Item!.GetType().GetProperty(property.Name) ?? property;
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
    }
}
