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
        private static XmlSerializer serializer = new XmlSerializer(typeof(AdaptiveCard));
        private static XmlWriterSettings settings = new XmlWriterSettings()
        {
            Encoding = new UnicodeEncoding(false, false), // no BOM in a .NET string
            Indent = true,
        };


        public static IEnumerable<object> GetFiles()
        {
            var dataFolder = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "Data"));
            var files = Directory.EnumerateFiles(dataFolder, "*.json", SearchOption.AllDirectories);
            foreach (var jsonFile in files)
            {
                try
                {
                    string xmlFile = jsonFile.Replace(".json", ".xml");
                    if (!File.Exists(Path.Combine(Environment.CurrentDirectory, xmlFile)))
                    {
                        var json = File.ReadAllText(jsonFile);
                        var card = JsonConvert.DeserializeObject<AdaptiveCard>(json);
                        json = JsonConvert.SerializeObject(card, Newtonsoft.Json.Formatting.Indented);
                        File.WriteAllText(jsonFile, json);

                        File.WriteAllText(xmlFile, ToXml(card));
                    }
                }
                catch (Exception err)
                {
                    Debug.WriteLine($"{jsonFile} {err.Message}", err);
                }
            }
            return files.Select(s => new object[] { Path.GetRelativePath(dataFolder, s), s });
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
        [MemberData(nameof(GetFiles))]
        public void Do(string name, string jsonFile)
        {
            Debug.WriteLine($"---- {name} -----");
            string xmlFile = jsonFile.Replace(".json", ".xml");
            var json = File.ReadAllText(jsonFile);
            var xml = File.ReadAllText(xmlFile);
            var jsonCard = JsonConvert.DeserializeObject<AdaptiveCard>(json);
            var xmlCard = serializer.Deserialize(new StringReader(xml));
            var json2 = JsonConvert.SerializeObject(xmlCard, Newtonsoft.Json.Formatting.Indented);
            if (json != json2)
            {
                Debug.WriteLine(json);
                Debug.WriteLine(json2);
            }
            Assert.Equal(json, json2, ignoreLineEndingDifferences:true, ignoreWhiteSpaceDifferences: true);


        }

    }
}