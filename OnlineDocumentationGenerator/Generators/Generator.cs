/*
   Copyright 2008 - 2022 CrypTool Team

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using OnlineDocumentationGenerator.DocInformations;
using System.Collections.Generic;

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

            foreach (string lang in entityDocumentationPage.AvailableLanguages)
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
            if (OnGuiLogNotificationOccured != null)
            {
                EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, null, message, logLevel);
            }
        }
    }
}
