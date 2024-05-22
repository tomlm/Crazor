namespace Crazor.Attributes
{

    [AttributeUsage(AttributeTargets.Property)]
    public abstract class MemoryAttribute : Attribute
    {
        public string Name => GetType().Name.Replace("MemoryAttribute", String.Empty);

        public abstract string? GetKey(object obj);
    }
}
