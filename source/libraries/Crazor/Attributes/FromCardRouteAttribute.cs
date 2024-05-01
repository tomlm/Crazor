
namespace Crazor.Attributes
{
    /// <summary>
    /// Specifies that a parameter or property should be bound from the CardRoute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class FromCardRouteAttribute : Attribute
    {
        /// <summary>
        /// Name of CardRoute pattern
        /// </summary>
        public string? Name { get; set; }
    }
}
