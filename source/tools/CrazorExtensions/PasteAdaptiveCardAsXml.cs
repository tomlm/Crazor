using System;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using AdaptiveCards;
using EnvDTE;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Settings.Internal;
using Microsoft.VisualStudio.Shell;
using Newtonsoft.Json;
using static Microsoft.VisualStudio.Threading.AsyncReaderWriterLock;
using Task = System.Threading.Tasks.Task;

namespace CrazorExtensions
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class PasteAdaptiveCardAsXml
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("9a18b675-be1d-4e5a-87c3-28c84b89225d");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="PasteAdaptiveCardAsXml"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private PasteAdaptiveCardAsXml(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static PasteAdaptiveCardAsXml Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in PasteAdaptiveCardAsXml's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new PasteAdaptiveCardAsXml(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var json = Clipboard.GetText(TextDataFormat.UnicodeText);
            var card = JsonConvert.DeserializeObject<AdaptiveCard>(json);
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, AdaptiveCard.ContentType);
            var serializer = new XmlSerializer(typeof(AdaptiveCard));

            using (StringWriter textWriter = new StringWriter())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(textWriter, new XmlWriterSettings() { Indent = true }))
                {
                    serializer.Serialize(xmlWriter, card, namespaces);
                }

                var xml = MassageXml(textWriter.ToString());
                DTE dte = Package.GetGlobalService(typeof(DTE)) as DTE;
                TextDocument activeDoc = dte.ActiveDocument.Object() as TextDocument;
                activeDoc.CreateEditPoint(activeDoc.Selection.ActivePoint).Insert(xml);
            }
        }

        public string MassageXml(string xml)
        {
            foreach (var enumType in typeof(AdaptiveCard).Assembly.GetTypes().Where(t => t.Name.StartsWith("Adaptive") && t.IsEnum))
            {
                foreach (var value in enumType.GetEnumValues())
                {
                    MemberInfo memberInfo = enumType.GetMember(value.ToString()).First();
                    var xmlEnumAtt = memberInfo.GetCustomAttribute<XmlEnumAttribute>();
                    if (xmlEnumAtt != null)
                    {
                        xml = xml.Replace($"\"{xmlEnumAtt.Name}\"", $"\"{memberInfo.Name}\"");
                    }
                }
            }
            xml = xml
                .Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", String.Empty)
                .Replace("xmlns=\"application/vnd.microsoft.card.adaptive\"", String.Empty)
                .Replace("<Input.", "<Input")
                .Replace("</Input.", "</Input")
                .Replace("<Action.", "<Action")
                .Replace("</Action.", "</Action")
                .Replace("<Data.", "<Data")
                .Replace("</Data.", "</Data");
            return xml;
        }
    }
}