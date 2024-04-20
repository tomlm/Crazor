// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveCards;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace AdaptiveCardXmlTests
{
    public class SerializationTests
    {
        private static JsonSerializerSettings jsonSettings = new JsonSerializerSettings()
        {
            Formatting = Newtonsoft.Json.Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
        };
        private static XmlWriterSettings settings = new XmlWriterSettings()
        {
            Encoding = new UnicodeEncoding(false, false), // no BOM in a .NET string
            Indent = true,
        };


        public static IEnumerable<object[]> GetTestFiles()
        {
            var dataFolder = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "Data"));
            var files = Directory.EnumerateFiles(dataFolder, "*.json", SearchOption.AllDirectories);
            return files.Select(s => new object[] { Path.GetFileName(s), s });
        }

        public static string ToXml(AdaptiveCard card)
        {
            using (StringWriter textWriter = new StringWriter())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(textWriter, settings))
                {
                    var xmlSerializer = new XmlSerializer(typeof(AdaptiveCard), defaultNamespace: AdaptiveCard.ContentType);
                    xmlSerializer.Serialize(xmlWriter, card, new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty }));
                }
                return textWriter.ToString(); //This is the output as a string
            }
        }

        [Theory]
        [MemberData(nameof(GetTestFiles))]
        public void Test(string name, string jsonFile)
        {
            string xmlFile = jsonFile.Replace(".json", ".xml");
            var json = File.ReadAllText(jsonFile);
            var card = JsonConvert.DeserializeObject<AdaptiveCard>(json, jsonSettings);
            json = JsonConvert.SerializeObject(card, jsonSettings);
            try
            {
                if (!File.Exists(xmlFile))
                {
                    File.WriteAllText(jsonFile, json);
                }
                File.WriteAllText(xmlFile, ToXml(card!));
            }
            catch (Exception err)
            {
                Debug.WriteLine($"{jsonFile} {err.Message}", err);
                throw;
            }

            Debug.WriteLine($"---- {name} -----");
            var xml = File.ReadAllText(xmlFile);
            var jsonCard = JsonConvert.DeserializeObject<AdaptiveCard>(json, jsonSettings);
            xml = ToXml(jsonCard!);
            var reader = XmlReader.Create(new StringReader(xml), new XmlReaderSettings() { IgnoreWhitespace = false });
            var xmlSerializer = new XmlSerializer(typeof(AdaptiveCard), defaultNamespace: AdaptiveCard.ContentType);
            var xmlCard = xmlSerializer.Deserialize(reader);
            var json2 = JsonConvert.SerializeObject(xmlCard, jsonSettings);
            if (json != json2)
            {
                //File.WriteAllText(@"\scratch\foo1.json", json);
                //File.WriteAllText(@"\scratch\foo2.json", json2);
                Debug.WriteLine(json);
                Debug.WriteLine(json2);
            }
            Assert.Equal(json, json2, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }

    }
}