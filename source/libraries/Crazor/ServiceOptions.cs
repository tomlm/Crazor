// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveCards;
using Microsoft.Bot.Schema;
using System.Reflection;

namespace Crazor
{
    public class ServiceOptions
    {
        /// <summary>
        /// Load assemblies with cards, using long or short form of the assembly name.
        /// </summary>
        /// <param name="assemblyString"></param>
        public void LoadCardAssembly(string assemblyString)
        {
            Assembly.Load(assemblyString);
        }

        /// <summary>
        /// Gets or sets the loggers to trace input and output Card JSON.
        /// </summary>
        public LoggerOptions Logger { get; } = new();

        public class LoggerOptions
        {
            public Action<Activity>? Request{ get; set; }
            public Action<Activity, AdaptiveCard>? Response { get; set; }
        }
    }
}
