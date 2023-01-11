﻿// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Crazor.Server.Teams;
using Newtonsoft.Json;

namespace Crazor.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class TabInfoAttribute : Attribute
    {
        public TabInfoAttribute()
        {
        }

        public TabInfoAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// The display name of the tab.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Specifies whether the tab offers an experience in the context of a channel in a team, or an experience scoped to an individual user alone. These options are non-exclusive. Currently static tabs are only supported in the 'personal' scope.
        /// </summary>
        public List<StaticTabScope> Scopes { get; set; } = new List<StaticTabScope>() { StaticTabScope.Personal  };

        /// <summary>
        /// The set of contextItem scopes that a tab belong to
        /// </summary>
        public List<TabContext> Context { get; set; } = new List<TabContext>() { };
    }
}
