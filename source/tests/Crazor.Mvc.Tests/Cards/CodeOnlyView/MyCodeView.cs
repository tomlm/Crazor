// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveCards;
using Crazor.Attributes;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Crazor.Mvc.Tests.Cards.CodeOnlyView
{
    /// <summary>
    /// You don't have to use Razor for your view.  Simply derive from CardView and override BindCard() to return the AdaptiveCard
    /// </summary>
    [CardRoute("MyCode")]
    public class MyCodeView : CardView
    {

        [SessionMemory]
        public int Counter { get; set; }

        public override async Task<AdaptiveCard?> RenderCardAsync(bool isPreview, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            var card = new AdaptiveCard("1.5")
            {
                Body = new List<AdaptiveElement>()
                {
                    new AdaptiveTextBlock($"CodeOnly"),
                    new AdaptiveTextBlock($"Counter: {this.Counter}")
                },
                Actions = new List<AdaptiveAction>()
                {
                    new AdaptiveExecuteAction(){ Verb = nameof(OnIncrement), Title = "Increment"}
                }
            };
            System.Diagnostics.Debug.WriteLine(ToXml(card));
            return card;
        }

        public void OnIncrement()
            => this.Counter++;

        public static string ToXml(AdaptiveCard card)
        {
            XmlWriterSettings settings = new XmlWriterSettings()
            {
                Encoding = new UnicodeEncoding(false, false), // no BOM in a .NET string
                Indent = true,
            };

            using (StringWriter textWriter = new StringWriter())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(textWriter, settings))
                {
                    AdaptiveCard.XmlSerializer.Serialize(xmlWriter, card, new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty }));
                }
                return textWriter.ToString(); //This is the output as a string
            }
        }


    }
}
