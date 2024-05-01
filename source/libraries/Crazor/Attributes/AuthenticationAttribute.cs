
namespace Crazor.Attributes
{
    /// <summary>
    /// Defines Authentication connection to use for the view
    /// </summary>
    /// <example>
    /// [Authentication("GraphAPI")]
    /// </example>
    [AttributeUsage(AttributeTargets.Class)]
    public class AuthenticationAttribute : Attribute
    {
        public AuthenticationAttribute(string connectionName = "Default")
        {
            Name = connectionName;
        }

        /// <summary>
        /// Auth Connection Name as configured in bot framework portal.
        /// </summary>
        public string Name { get; set; }

    }
}
