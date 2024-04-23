// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using AdaptiveCards;

namespace Crazor
{
    public class BannerMessage
    {
        public string Text { get; set; } = String.Empty;

        public AdaptiveContainerStyle Style { get; set; } = AdaptiveContainerStyle.Accent;
    }
}
