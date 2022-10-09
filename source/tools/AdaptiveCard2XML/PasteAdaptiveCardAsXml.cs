using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using AdaptiveCards;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Newtonsoft.Json;
using Task = System.Threading.Tasks.Task;

namespace AdaptiveCard2Xml
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
            using (MemoryStream stream = new MemoryStream())
            {
                using (XmlWriter writer = XmlWriter.Create(stream, new XmlWriterSettings() { Indent = true }))
                {
                    new XmlSerializer(typeof(AdaptiveCard)).Serialize(writer, card);
                }
                stream.Seek(0, SeekOrigin.Begin);

                using (TextReader reader = new StreamReader(stream))
                {
                    var xml = reader.ReadToEnd();
                    DTE dte = Package.GetGlobalService(typeof(DTE)) as DTE;
                    TextDocument activeDoc = dte.ActiveDocument.Object() as TextDocument;
                    activeDoc.Selection.SelectAll();
                    activeDoc.Selection.Delete();
                    activeDoc.CreateEditPoint().Insert(xml);
                }
            }
        }
    }
}
