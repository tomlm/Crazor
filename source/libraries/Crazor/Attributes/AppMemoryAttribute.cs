namespace Crazor.Attributes
{
    /// <summary>
    /// This property will be peristed scoped to the application for all users and all sessions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class AppMemoryAttribute : MemoryAttribute
    {
        public override string? GetKey(object obj) => "data";
    }
}
