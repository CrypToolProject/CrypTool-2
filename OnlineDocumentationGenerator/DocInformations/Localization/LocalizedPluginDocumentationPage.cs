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
using OnlineDocumentationGenerator.Generators.HtmlGenerator;
using OnlineDocumentationGenerator.Utils;
using System;
using System.Globalization;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace OnlineDocumentationGenerator.DocInformations.Localization
{
    public abstract class LocalizedPluginDocumentationPage : LocalizedEntityDocumentationPage
    {
        protected XElement _xml;

        public new PluginDocumentationPage DocumentationPage => base.DocumentationPage as PluginDocumentationPage;

        public override string FilePath => OnlineHelp.GetPluginDocFilename(PluginType, Lang);

        public Type PluginType { get; private set; }
        public string ToolTip { get; private set; }

        public TaskPaneAttribute[] Settings => DocumentationPage.Settings;

        public string AuthorURL => DocumentationPage.AuthorURL;

        public string AuthorInstitute => DocumentationPage.AuthorInstitute;

        public string AuthorEmail => DocumentationPage.AuthorEmail;

        public string AuthorName => DocumentationPage.AuthorName;

        protected LocalizedPluginDocumentationPage(PluginDocumentationPage pluginDocumentationPage, Type pluginType, XElement xml, string lang, BitmapFrame icon)
        {
            base.DocumentationPage = pluginDocumentationPage;
            PluginType = pluginType;
            _xml = xml;
            Lang = lang;
            Icon = icon;

            CultureInfo cultureInfo = CultureInfoHelper.GetCultureInfo(lang);
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            Name = pluginType.GetPluginInfoAttribute().Caption;
            ToolTip = pluginType.GetPluginInfoAttribute().ToolTip;
        }
    }
}