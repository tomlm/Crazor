namespace Crazor.Attributes
{
    /// <summary>
    /// Specifies that a parameter or property should be bound using the request query string.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class FromCardQueryAttribute : Attribute
    {
        /// <summary>
        /// Name of query param
        /// </summary>
        public string? Name { get; set; }
    }
}
