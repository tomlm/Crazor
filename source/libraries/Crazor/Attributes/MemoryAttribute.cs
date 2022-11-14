using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Crazor.Attributes
{

    [AttributeUsage(AttributeTargets.Property)]
    public abstract class MemoryAttribute : Attribute
    {
        public string Name => GetType().Name.Replace("MemoryAttribute", String.Empty);

        public abstract string? GetKey(object obj);
    }
}
