using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crazor.Attributes
{
    public enum BindingType
    {
        Value,
        PropertyName,
        DisplayName
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class BindValueAttribute : Attribute
    {
        public BindValueAttribute()
        {
        }

        public BindValueAttribute(BindingType policy)
        {
            Policy = policy;
        }

        /// <summary>
        /// if true instead of binding the the value of the property it will bind to the Name of the property.
        /// </summary>
        public BindingType Policy { get; set; } = BindingType.Value;
    }
}
