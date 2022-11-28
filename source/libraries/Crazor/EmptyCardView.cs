// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using AdaptiveCards;

namespace Crazor
{
    internal class EmptyCardView : CardView
    {
        public override Task<AdaptiveCard?> RenderCardAsync(bool isPreview, CancellationToken cancellationToken)
        {
            return Task.FromResult(new AdaptiveCard("1.0"))!;
        }
    }
}
