//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crazor
{
    public class SessionData
    {
        private const char seperator = '|';
        
        public static SessionData FromString(string data)
        {
            var parts = data.Split(seperator);
            var resourceId = parts.Skip(1).Take(1).Single();
            var sessionId = parts.Skip(2).Take(1).Single();
            return new SessionData()
            {
                App = parts.First(),
                ResourceId = !String.IsNullOrWhiteSpace(resourceId) ? resourceId : null,
                SessionId = !String.IsNullOrWhiteSpace(sessionId) ? sessionId : null,
            };
        }

        public override string ToString()
        {
            return $"{App}{seperator}{ResourceId ?? String.Empty}{seperator}{SessionId ?? String.Empty}";
        }

        /// <summary>
        /// Unique id for the card application
        /// </summary>
        public string App { get; set; } = String.Empty;

        /// <summary>
        /// Unique id for an instance of the application
        /// </summary>
        public string? ResourceId { get; set; } = null;

        /// <summary>
        /// Unique Id for the session
        /// </summary>
        public string? SessionId { get; set; } = null;
    }
}
