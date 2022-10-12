// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace AdaptiveCards
{
    /// <summary>
    /// Represents the fallback property.
    /// </summary>
    [JsonConverter(typeof(AdaptiveFallbackConverter))]
    public partial class AdaptiveFallbackElement : IXmlSerializable
    {

        /// <summary>
        /// Initializes an AdaptiveFallbackElement with the given <paramref name="fallbackType"/>.
        /// </summary>
        /// <param name="fallbackType">The type of fallback this instance represents.</param>
        public AdaptiveFallbackElement(AdaptiveFallbackType fallbackType)
        {
            Type = fallbackType;
            Content = null;
        }

        /// <summary>
        /// Initializes an AdaptiveFallbackElement with the given <paramref name="fallbackContent"/>.
        /// </summary>
        /// <param name="fallbackContent">The content to show in the event of fallback.</param>
        public AdaptiveFallbackElement(AdaptiveTypedElement fallbackContent)
        {
            Type = AdaptiveFallbackType.Content;
            Content = fallbackContent;
        }

        /// <summary>
        /// Initializes an AdaptiveFallbackElement with no fallback type.
        /// </summary>
        public AdaptiveFallbackElement()
        {
            Type = AdaptiveFallbackType.None;
        }

        /// <summary>
        /// Represents the type of fallback to perform.
        /// </summary>
        [JsonIgnore]
#if !NETSTANDARD1_3
        [XmlIgnore]
#endif
        public AdaptiveFallbackType Type { get; set; }

        /// <summary>
        /// The content to show in the event of fallback.
        /// </summary>
        [JsonIgnore]
#if !NETSTANDARD1_3
        [XmlIgnore]
#endif
        public AdaptiveTypedElement Content { get; set; }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            while (reader.MoveToNextAttribute())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Attribute:
                        switch (reader.Name)
                        {
                            case "Type":
                                if (Enum.TryParse<AdaptiveFallbackType>(reader.Value, true, out var type))
                                {
                                    Type = type;
                                }
                                break;
                        }
                        break;

                    default:
                        Debug.WriteLine(reader.NodeType);
                        break;
                }
            }

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.EndElement:
                        reader.ReadEndElement();
                        return;

                    case XmlNodeType.Element:
                        var elementTypes = AdaptiveTypedElementConverter.TypedElementTypes.Value.Values.ToList();
                        if (!AdaptiveTypedElementConverter.TypedElementTypes.Value.TryGetValue(reader.Name, out var typeToCreate))
                        {
                            typeToCreate = typeof(AdaptiveUnknownElement);
                        }
                        XmlSerializer serializer = new XmlSerializer(typeToCreate, elementTypes.ToArray());
                        Type = AdaptiveFallbackType.Content;
                        Content = (AdaptiveTypedElement)serializer.Deserialize(reader.ReadSubtree());
                        break; // we've read our content node, we should be done?
                }
            }

        }

        public void WriteXml(XmlWriter writer)
        {
            if (this.Type == AdaptiveFallbackType.Drop)
            {
                writer.WriteAttributeString("Type", this.Type.ToString().ToLower());
            }
            else
            {
                XmlSerializer serializer = new XmlSerializer(Content.GetType());

                serializer.Serialize(writer, Content);
            }
        }
    }
}
