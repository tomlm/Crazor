// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Newtonsoft.Json;

namespace AdaptiveCards
{
    /// <summary>
    ///     Controls the horizontal size (width) of Column.
    /// </summary>
    [JsonConverter(typeof(IgnoreDefaultStringEnumConverter<AdaptiveColumnWidth>), true)]
    public enum AdaptiveColumnWidth
    {
        /// <summary>
        ///     The width of the Column adjusts to match that of its container
        /// </summary>
        Stretch,

        /// <summary>
        ///     The width of the Column is optimally chosen depending on the space available in the element's container
        /// </summary>
        Auto

    }
}
