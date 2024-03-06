// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;

namespace Crazor.AdaptiveCards
{
    public delegate void TypedEventHandler<in TSender, in TEventArgs>(
        TSender sender,
        TEventArgs e
    ) where TEventArgs : EventArgs;
}
