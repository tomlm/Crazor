// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Crazor.AdaptiveCards;
using Crazor.Attributes;
using Crazor.Interfaces;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace Crazor
{
    [CardRoute("/Cards/Empty")]
    public class EmptyCardView : CustomCardView
    {
        public override Task<AdaptiveCard?> RenderCardAsync(bool isPreview, CancellationToken cancellationToken)
        {
            return Task.FromResult(new AdaptiveCard("1.0"))!;
        }
    }
}
