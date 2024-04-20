// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Crazor.AdaptiveCards
{
    /// <summary>
    /// Interface encapsulating the properties of an AdaptiveCards element that displays text.
    /// </summary>
    public interface IAdaptiveTextElement
    {
        /// <summary>
        /// The size to use while displaying the text.
        /// </summary>
        AdaptiveTextSize Size { get; set; }

        /// <summary>
        /// The weight to use while displaying the text.
        /// </summary>
        AdaptiveTextWeight Weight { get; set; }

        /// <summary>
        /// The color to use while displaying the text.
        /// </summary>
        AdaptiveTextColor Color { get; set; }

        /// <summary>
        /// Make the text less prominent when displayed.
        /// </summary>
        bool IsSubtle { get; set; }

        /// <summary>
        /// The text to display.
        /// </summary>
        string Text { get; set; }

        /// <summary>
        /// Controls which <see cref="AdaptiveFontType"/> is used to display the text.
        /// </summary>
        AdaptiveFontType FontType { get; set; }
    }
}
