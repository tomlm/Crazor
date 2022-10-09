// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace AdaptiveCards
{
    /// <summary>
    /// Represents an element in Adaptive Cards.
    /// </summary>
    public abstract class AdaptiveElement : AdaptiveTypedElement
    {
        /// <summary>
        /// The amount of space the element should be separated from the previous element. Default value is <see cref="AdaptiveSpacing.Default"/>.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
#if !NETSTANDARD1_3
        [XmlAttribute]
#endif
        [DefaultValue(typeof(AdaptiveSpacing), "default")]
        public AdaptiveSpacing Spacing { get; set; }

        /// <summary>
        /// Indicates whether there should be a visible separator (e.g. a line) between this element and the one before it.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
#if !NETSTANDARD1_3
        [XmlAttribute]
#endif
        [DefaultValue(false)]
        public bool Separator { get; set; }

        /// <summary>
        /// SSML fragment for spoken interaction.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [Obsolete("CardElement.Speak has been deprecated.  Use AdaptiveCard.Speak", false)]
        public string Speak { get; set; }

        /// <summary>
        /// The amount of space the element should be separated from the previous element. Default value is <see cref="AdaptiveHeight.Auto"/>.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
#if !NETSTANDARD1_3
        [XmlIgnore]
#endif
        [DefaultValue(null)]
        public AdaptiveDimension Height { get; set; }

        /// <summary>
        /// XmlProperty for serialization of height
        /// </summary>
        [JsonIgnore]
#if !NETSTANDARD1_3
        [XmlAttribute(nameof(Height))]
#endif
        [DefaultValue(null)]
        public string HeightXml { get => Height?.ToString(); set => this.Height = (value != null) ? new AdaptiveDimension(value) : null; }
        
        /// <summary>
        /// Control serialization of empty height values
        /// </summary>
        /// <returns></returns>
        public bool ShouldSerializeHeightXml() => Height != null;

        /// <summary>
        /// Indicates whether the element should be visible when the card has been rendered.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
#if !NETSTANDARD1_3
        [XmlAttribute]
#endif
        [DefaultValue(true)]
        public bool IsVisible { get; set; } = true;
    }
}
