using AdaptiveCards;

namespace Crazor
{

    [Flags]
    public enum AuthenticationFlags
    {
        None = 0,
        
        /// <summary>
        /// Supports OAuth cards
        /// </summary>
        OAuth = 0x1,

        /// <summary>
        /// Supports SSO
        /// </summary>
        SSO = 0x2,
    }

    /// <summary>
    /// ChannelOptions class represents options for 
    /// </summary>
    public class ChannelOptions
    {
        /// <summary>
        /// Does this channel support card header 
        /// </summary>
        public bool SupportsCardHeader { get; set; } = false;

        /// <summary>
        /// Does this support a task module
        /// </summary>
        public bool SupportsTaskModule { get; set; } = false;

        /// <summary>
        /// Version to assume client supports for poly fills
        /// </summary>
        public AdaptiveSchemaVersion SchemaVersion { get; set; } = new AdaptiveSchemaVersion(1, 5);

        /// <summary>
        /// Authentication suppport for the channel
        /// </summary>
        public AuthenticationFlags Authentication { get; set; } = AuthenticationFlags.None;
    }
}
