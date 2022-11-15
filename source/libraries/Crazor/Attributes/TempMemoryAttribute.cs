using Crazor.Validation;

namespace Crazor.Attributes
{

    /// <summary>
    /// Don't persist property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class TempMemoryAttribute : MemoryAttribute
    {
        public override string? GetKey(object obj)
        {
            return null;
        }
    }
}
