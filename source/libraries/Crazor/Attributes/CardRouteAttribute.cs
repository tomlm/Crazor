namespace Crazor.Attributes
{
    /// <summary>
    /// Mark that this class is a CardRoute path
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class CardRouteAttribute : Attribute
    {
        /// <summary>
        /// Creates a new <see cref="RouteAttribute"/> with the given route template.
        /// </summary>
        /// <param name="template">The route template. May not be null.</param>
        public CardRouteAttribute(string template)
        {
            Template = template ?? throw new ArgumentNullException(nameof(template));
        }

        public string Template { get; }

        public int Order { get; set; }

        public string? Name { get; set; }
    }
}
