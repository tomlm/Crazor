namespace Crazor.Attributes
{
    /// <summary>
    /// Global memory scoped to the application class name
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class AppMemoryAttribute : MemoryAttribute
    {
        public override string? GetKey(object obj) => "data";
    }
}
