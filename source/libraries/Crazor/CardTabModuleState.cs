// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using AdaptiveCards;

namespace Crazor
{
    internal class CardTabModuleState
    {
        public Dictionary<string, AdaptiveExecuteAction> RefreshMap { get; set; } = new Dictionary<string, AdaptiveExecuteAction>();
    }
}
