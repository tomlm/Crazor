namespace Crazor.Attributes
{
    /// <summary>
    /// Defines Authentication connection to use for the view
    /// </summary>
    /// <remarks>
    /// When this is added to the card user will be given option to signin.
    /// </remarks>
    /// <example>
    /// [OAuthConnection("GraphAPI")]
    /// </example>
    [AttributeUsage(AttributeTargets.Class)]
    public class OAuthConnectionAttribute : Attribute
    {
        public const string Default = "Default";

        public OAuthConnectionAttribute(string connectionName = Default)
        {
            Connection = connectionName;
        }

        /// <summary>
        /// Auth Connection Name as configured in bot framework portal.
        /// </summary>
        public string Connection { get; set; }
    }
}
