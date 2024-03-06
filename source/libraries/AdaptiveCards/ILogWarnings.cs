// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Collections.Generic;

namespace Crazor.AdaptiveCards
{
    internal interface ILogWarnings
    {
        List<AdaptiveWarning> Warnings { get; set; }
    }
}
