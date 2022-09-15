// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Xml.Serialization;

namespace AdaptiveCards
{
    /// <summary>
    ///     Base of Action types
    /// </summary>
    public abstract class AdaptiveAction : AdaptiveTypedElement
    {
        /// <summary>
        ///     Title of the action
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
#if !NETSTANDARD1_3
        [XmlAttribute]
#endif
        public string Title { get; set; }

        // TODO: Title should be required is NOT a selectAction? Or can we use it as a tooltip?

        /// <summary>
        ///     Speak phrase for this action
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [Obsolete("ActionBase.Speak has been deprecated.  Use AdaptiveCard.Speak", false)]
        public string Speak { get; set; }

        /// <summary>
        ///     IconUrl that can be specified for actions
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
#if !NETSTANDARD1_3
        [XmlAttribute]
#endif
        public string IconUrl { get; set; }

        /// <summary>
        ///     Style that can be specified for actions
        /// </summary>
        /// <remarks>
        /// Values:
        ///  * "default": Action is displayed as normal
        ///  * "positive": Action is displayed with a positive style(typically the button becomes accent color)
        ///  * "destructive": Action is displayed with a destructive style(typically the button becomes red)
        ///  This is a string for backwards compatibility, but the enum is AdaptiveActionStyle
        /// </remarks>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
#if !NETSTANDARD1_3
        [XmlAttribute]
#endif
        [DefaultValue("default")]
        public string Style { get; set; } = "default";

        /// <summary>
        /// Defines text that should be displayed to the end user as they hover the mouse over the action, and read when using narration software.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
#if !NETSTANDARD1_3
        [XmlAttribute]
#endif
        public string Tooltip { get; set; }

        /// <summary>
        ///     Determines whether the action should be displayed as a button or in the overflow menu.
        /// </summary>
        /// <remarks>
        /// Values are: "primary" or "secondary"
        /// </remarks>
        [JsonConverter(typeof(IgnoreNullEnumConverter<AdaptiveActionMode>), true)]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
#if !NETSTANDARD1_3
        [XmlIgnore]
#endif
        [DefaultValue(null)]
        public AdaptiveActionMode? Mode { get; set; } = null;

#if !NETSTANDARD1_3
        /// <summary>
        /// The XML serializer doesn't handle nullable value types. This allows serialization if non-null.
        /// </summary>
        [JsonIgnore]
        [XmlAttribute("Mode")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public AdaptiveActionMode ModeXml { get { return (Mode.HasValue) ? Mode.Value : AdaptiveActionMode.Primary; } set { Mode = value; } }

        /// <summary>
        /// Determines whether to serialize the style for XML.
        /// </summary>
        public bool ShouldSerializeStyleXml() => this.Mode.HasValue;
#endif


        /// <summary>
        ///     Is this action enabled or disabled?
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [XmlAttribute]
        public bool IsEnabled{ get; set; } = true;
    }
}
