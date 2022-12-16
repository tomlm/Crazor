// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.


using AdaptiveCards;
using Microsoft.AspNetCore.Components;

namespace Crazor.Blazor.Components.AdaptiveCards
{

    /// <summary>
    /// Component for MediaSource
    /// </summary>

    public class MediaSource : ItemComponent<IList<AdaptiveMediaSource>, AdaptiveMediaSource>
    {

        [Parameter]
        public String MimeType { get => Item.MimeType; set => Item.MimeType = value; }

        [Parameter]
        public String Url { get => Item.Url; set => Item.Url = value; }
    }
}
