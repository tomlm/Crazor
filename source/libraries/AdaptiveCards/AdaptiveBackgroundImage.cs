// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AdaptiveCards
{
    /// <summary>
    /// Represents the backgroundImage property
    /// </summary>
#if !NETSTANDARD1_3
    [XmlType(TypeName = AdaptiveBackgroundImage.TypeName)]
#endif
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class AdaptiveBackgroundImage
    {
        /// <summary>
        /// Initializes a new AdaptiveBackgroundImage instance.
        /// </summary>
        public AdaptiveBackgroundImage() { }

        /// <summary>
        /// Initializes a new AdaptiveBackgroundImage instance using the specified URL.
        /// </summary>
        /// <param name="url">The background image URL expressed as a string.</param>
        public AdaptiveBackgroundImage(string url)
        {
            Url = url;
        }

        /// <summary>
        /// Initializes a new AdaptiveBackgroundImage instance using the specified parameters.
        /// </summary>
        /// <param name="url">The background image URL expressed as a string.</param>
        /// <param name="fillMode">Controls how the background image should be displayed.</param>
        /// <param name="hAlignment">Controls horizontal alignment.</param>
        /// <param name="vAlignment">Controls vertical alignment.</param>
        public AdaptiveBackgroundImage(string url, AdaptiveImageFillMode fillMode, AdaptiveHorizontalAlignment hAlignment, AdaptiveVerticalAlignment vAlignment)
        {
            Url = url;
            FillMode = fillMode;
            HorizontalAlignment = hAlignment;
            VerticalAlignment = vAlignment;
        }

        /// <summary>
        /// The JSON property name that this class implements.
        /// </summary>
        public const string TypeName = "backgroundImage";

        /// <summary>
        /// The Url of the background image.
        /// </summary>
        [JsonRequired]
#if !NETSTANDARD1_3
        [XmlAttribute]
#endif
        [DefaultValue(null)]
        public string Url { get; set; }

        /// <summary>
        /// Controls how the image is tiled or stretched.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
#if !NETSTANDARD1_3
        [XmlAttribute]
#endif
        [DefaultValue(typeof(AdaptiveImageFillMode), "cover")]
        public AdaptiveImageFillMode FillMode { get; set; }

        /// <summary>
        /// Determines how to align the background image horizontally.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
#if !NETSTANDARD1_3
        [XmlAttribute]
#endif
        [DefaultValue(typeof(AdaptiveHorizontalAlignment), "left")]
        public AdaptiveHorizontalAlignment HorizontalAlignment { get; set; }

        /// <summary>
        /// Determines how to align the background image vertically.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
#if !NETSTANDARD1_3
        [XmlAttribute]
#endif
        [DefaultValue(typeof(AdaptiveVerticalAlignment), "top")]
        public AdaptiveVerticalAlignment VerticalAlignment { get; set; }

        /// <summary>
        /// Determines if this instance only has default property values set (aside from <see cref="Url"/>).
        /// </summary>
        /// <returns>true iff this instance has only default property values.</returns>
        public bool HasDefaultValues()
        {
            if (FillMode == AdaptiveImageFillMode.Cover && HorizontalAlignment == AdaptiveHorizontalAlignment.Left && VerticalAlignment == AdaptiveVerticalAlignment.Top)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Implicitly create an AdaptiveBackgroundImage from a Uri
        /// </summary>
        /// <param name="backgroundImageUrl"></param>
        public static implicit operator AdaptiveBackgroundImage(Uri backgroundImageUrl)
        {
            return new AdaptiveBackgroundImage(backgroundImageUrl.ToString());
        }
    }
}
