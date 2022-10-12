using AdaptiveCards;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
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
        private static XmlSerializer serializer = new XmlSerializer(typeof(AdaptiveCard));
        private static XmlWriterSettings settings = new XmlWriterSettings()
        {
            Encoding = new UnicodeEncoding(false, false), // no BOM in a .NET string
            Indent = true,
        };


        public static IEnumerable<object> GetTestFiles()
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
                    serializer.Serialize(xmlWriter, card);
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
            try
            {
                if (!File.Exists(xmlFile))
                {
                    var card = JsonConvert.DeserializeObject<AdaptiveCard>(json, jsonSettings);
                    json = JsonConvert.SerializeObject(card, jsonSettings);
                    File.WriteAllText(jsonFile, json);
                    File.WriteAllText(xmlFile, ToXml(card));
                }
            }
            catch (Exception err)
            {
                Debug.WriteLine($"{jsonFile} {err.Message}", err);
                throw;
            }

            Debug.WriteLine($"---- {name} -----");
            var xml = File.ReadAllText(xmlFile);
            var jsonCard = JsonConvert.DeserializeObject<AdaptiveCard>(json, jsonSettings);
            var reader = XmlReader.Create(new StringReader(xml));
            var xmlCard = serializer.Deserialize(reader);
            var json2 = JsonConvert.SerializeObject(xmlCard, jsonSettings);
            if (json != json2)
            {
                Debug.WriteLine(json);
                Debug.WriteLine(json2);
            }
            Assert.Equal(json, json2, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }

    }
}