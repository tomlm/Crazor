// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Crazor.AdaptiveCards
{
    /// <summary>
    /// Helper class used by Newtonsoft.Json to convert the backgroundImage property to/from JSON.
    /// </summary>
    public class AdaptiveBackgroundImageConverter : JsonConverter, ILogWarnings
    {
        /// <summary>
        /// A list of warnings generated by the converter.
        /// </summary>
        public List<AdaptiveWarning> Warnings { get; set; } = new List<AdaptiveWarning>();

        /// <summary>
        /// Writes the object to JSON. If the supplied <paramref name="backgroundImage"/> is all default values and a URL, will write as a simple string. Otherwise, serialize the supplied <paramref name="backgroundImage"/> as a JSON object via the <paramref name="serializer"/>.
        /// </summary>
        /// <param name="writer">JsonWriter to write to.</param>
        /// <param name="backgroundImage">The AdaptiveBackgroundImage object to write.</param>
        /// <param name="serializer">JsonSerializer to use for serialization.</param>
        public override void WriteJson(JsonWriter writer, object backgroundImage, JsonSerializer serializer)
        {
            AdaptiveBackgroundImage bi = (AdaptiveBackgroundImage) backgroundImage;
            if (!string.IsNullOrEmpty(bi.Url))
            {
                if (bi.HasDefaultValues())
                {
                    writer.WriteValue(bi.Url);
                }
                else
                {
                    serializer.Serialize(writer, backgroundImage);
                }
            }
        }

        /// <summary>
        /// Lets Newtonsoft.Json know that this class supports writing.
        /// </summary>
        public override bool CanWrite => true;

        /// <summary>
        /// Generates a new <see cref="AdaptiveBackgroundImage"/> instance from JSON.
        /// </summary>
        /// <param name="reader">JsonReader from which to read.</param>
        /// <param name="objectType">Not used.</param>
        /// <param name="existingValue">Not used.</param>
        /// <param name="serializer">Not used.</param>
        /// <returns>A new <see cref="AdaptiveBackgroundImage"/> instance.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken backgroundImageJSON = JToken.Load(reader);

            // Handle BackgroundImage as a string (BackCompat)
            if (backgroundImageJSON.Type == JTokenType.String)
            {
                return new AdaptiveBackgroundImage(backgroundImageJSON.Value<string>());
            }
            // backgroundImage is an object (Modern)
            else if (backgroundImageJSON.Type == JTokenType.Object)
            {
                return backgroundImageJSON.ToObject<AdaptiveBackgroundImage>();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Called by Newtonsoft.Json to determine if this converter knows how to convert an object of type <paramref name="objectType"/>.
        /// </summary>
        /// <param name="objectType">The type of object to convert.</param>
        /// <returns></returns>
        public override bool CanConvert(Type objectType)
        {
            // string --> BackCompat
            // AdaptiveBackgroundImage --> Modern
            return objectType == typeof(string) || objectType == typeof(AdaptiveBackgroundImage);
        }
    }
}
