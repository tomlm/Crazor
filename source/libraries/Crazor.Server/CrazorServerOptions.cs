﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Crazor.Teams;

namespace Crazor
{
    public class CrazorServerOptions : ServiceOptions
    {
        /// <summary>
        /// Teams manifest
        /// </summary>
        public Manifest Manifest { get; set; }
    }
}
