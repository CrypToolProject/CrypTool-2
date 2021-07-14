using System.Collections.Generic;
using OnlineDocumentationGenerator.DocInformations;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;

namespace OnlineDocumentationGenerator.Generators
{
    public abstract class Generator
    {
        protected List<EntityDocumentationPage> DocPages = new List<EntityDocumentationPage>();
        protected HashSet<string> AvailableLanguages = new HashSet<string>();

        public string OutputDir
        {
            get; set;
        }

        /// <summary>
        /// Adds a documentation page for the given entity to generate in all available localizations.
        /// </summary>
        /// <param name="entityDocumentationPage">The class with all informations about the entity</param>
        public void AddDocumentationPage(EntityDocumentationPage entityDocumentationPage)
        {
            DocPages.Add(entityDocumentationPage);

            foreach (var lang in entityDocumentationPage.AvailableLanguages)
            {
                AvailableLanguages.Add(lang);
            }
        }

        /// <summary>
        /// Generates all specified pages and an index page.
        /// </summary>
        public abstract void Generate(TemplateDirectory templatesDir);
        
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            if(OnGuiLogNotificationOccured!=null)
                EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, null, message, logLevel);
        }
    }
}
