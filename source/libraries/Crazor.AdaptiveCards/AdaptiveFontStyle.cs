// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Newtonsoft.Json;
using System;

namespace Crazor
{
    /// <summary>
    ///     Controls the font type of the TextBlock Elements
    /// </summary>
    [JsonConverter(typeof(IgnoreDefaultStringEnumConverter<AdaptiveFontType>), true)]
    public enum AdaptiveFontType
    {
        /// <summary>
        ///     The default font type for general use
        /// </summary>
        Default,

        /// <summary>
        ///     The monospace font type
        /// </summary>
        Monospace
    }
}
