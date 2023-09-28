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
using OnlineDocumentationGenerator.DocInformations.Utils;
using System;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace OnlineDocumentationGenerator.DocInformations.Localization
{
    public class LocalizedComponentDocumentationPage : LocalizedPluginDocumentationPage
    {
        public PropertyInfoAttribute[] Connectors => DocumentationPage.Connectors;

        public ComponentTemplateList Templates => DocumentationPage.RelevantTemplates;

        public new ComponentDocumentationPage DocumentationPage => (ComponentDocumentationPage)base.DocumentationPage;

        public XElement Introduction { get; private set; }
        public XElement Manual { get; private set; }
        public XElement Presentation { get; private set; }

        public LocalizedComponentDocumentationPage(ComponentDocumentationPage componentDocumentationPage, Type pluginType, XElement xml, string lang, BitmapFrame icon)
            : base(componentDocumentationPage, pluginType, xml, lang, icon)
        {
            if (_xml != null)
            {
                ReadInformationsFromXML();
            }
        }

        private void ReadInformationsFromXML()
        {
            Introduction = XMLHelper.FindLocalizedChildElement(_xml, "introduction");
            Manual = XMLHelper.FindLocalizedChildElement(_xml, "usage");
            Presentation = XMLHelper.FindLocalizedChildElement(_xml, "presentation");
        }
    }
}
