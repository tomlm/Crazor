// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Newtonsoft.Json;
using System.ComponentModel;
using System.Xml.Serialization;

namespace AdaptiveCards
{
    /// <summary>
    ///     Execute action gathers up input fields, merges with optional data field and generates event to client
    ///     asking for data to be submitted. 
    /// </summary>
#if !NETSTANDARD1_3
    [XmlType(TypeName = AdaptiveExecuteAction.TypeName)]
#endif
    public class AdaptiveExecuteAction : AdaptiveAction
    {
        /// <inheritdoc />
        public const string TypeName = "Action.Execute";

        /// <inheritdoc />
#if !NETSTANDARD1_3
        [XmlIgnore]
#endif
        public override string Type { get; set; } = TypeName;

        /// <summary>
        ///     initial data that input fields will be combined with. This is essentially 'hidden' properties, Example:
        ///     {"id":"123123123"}
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
#if !NETSTANDARD1_3
        [XmlIgnore]
#endif
        [DefaultValue(null)]
        public object Data { get; set; }

#if !NETSTANDARD1_3
        /// <summary>
        /// Get or set the data as a JSON string.
        /// </summary>
        [JsonIgnore]
        [XmlText]
        [DefaultValue(null)]
        public string DataXml
        {
            get => (Data != null) ? JsonConvert.SerializeObject(Data, Formatting.Indented) : null;
            set => Data = (value != null) ? JsonConvert.DeserializeObject(value, new JsonSerializerSettings { Converters = { new StrictIntConverter() } }) : null;
        }
#endif

        /// <summary>
        ///     Controls which inputs are associated with the execute action
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
#if !NETSTANDARD1_3
        [XmlAttribute]
#endif
        [DefaultValue(typeof(AdaptiveAssociatedInputs), "auto")]
        public AdaptiveAssociatedInputs AssociatedInputs { get; set; }

        /// <summary>
        ///     The card author-defined verb associated with this action.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
#if !NETSTANDARD1_3
        [XmlAttribute]
#endif
        [DefaultValue(null)]
        public string Verb { get; set; }
    }
}
