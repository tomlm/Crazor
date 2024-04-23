// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveCards;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using System.Reflection;

namespace Crazor
{

    /// <summary>
    /// Class representing options for Crazor service.
    /// </summary>
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
        /// GetChannelOptions for channelId if it exists, otherwise return "Default" options
        /// </summary>
        /// <param name="channelId"></param>
        /// <returns>channelOptions for controlling behavior for channelId</returns>
        public ChannelOptions GetChannelOptions(string channelId)
        {
            if (!this.ChannelOptions.TryGetValue(channelId, out var channelOptions))
            {
                channelOptions = this.ChannelOptions["Default"];
            }
            return channelOptions;
        }

        /// <summary>
        /// Gets or sets the loggers to trace input and output Card JSON.
        /// </summary>
        public LoggerOptions Logger { get; } = new();

        /// <summary>
        /// Map of ChannelId => ChannelOptions
        /// </summary>
        public Dictionary<string, ChannelOptions> ChannelOptions { get; } = new Dictionary<string, ChannelOptions>(StringComparer.OrdinalIgnoreCase);

        public class LoggerOptions
        {
            public Action<Activity>? Request { get; set; }
            public Action<Activity, AdaptiveCard>? Response { get; set; }
        }
    }
}
