// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
        /// Insert adaptive card markup to simulate card header and secondary menu
        /// </summary>
        public bool AddCardHeader { get; set; } = false;

        /// <summary>
        /// Automatically add secondary actions like OpenUrl, Refresh, About, Settings etc.
        /// </summary>
        public bool AddSecondaryActions { get; set; } = true;

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
