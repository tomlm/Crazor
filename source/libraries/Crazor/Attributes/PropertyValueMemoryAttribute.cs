using Crazor.Validation;

namespace Crazor.Attributes
{

    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyValueMemoryAttribute : MemoryAttribute
    {
        public PropertyValueMemoryAttribute(string propertyKey)
        {
            this.PropertyKeyName = propertyKey;
        }

        public string PropertyKeyName { get; set; }

        public override string? GetKey(object obj)
        {
            return obj.GetPropertyValue(PropertyKeyName)?.ToString();
        }
    }
}
