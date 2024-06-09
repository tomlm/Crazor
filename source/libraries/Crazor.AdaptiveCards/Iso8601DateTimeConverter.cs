// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Crazor
{
    /// <summary>
    /// Format datetime as Iso8601 instant format "yyyy-MM-ddTHH:mm:ssZ";
    /// </summary>
    public class Iso8601DateTimeConverter : IsoDateTimeConverter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Iso8601DateTimeConverter() : base()
        {
            DateTimeFormat = "yyyy-MM-ddTHH:mm:ssZ";
        }
    }
}
