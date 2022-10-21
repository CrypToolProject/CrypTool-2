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
using OnlineDocumentationGenerator.DocInformations.Localization;
using OnlineDocumentationGenerator.DocInformations.Utils;
using System;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace OnlineDocumentationGenerator.DocInformations
{
    public class ComponentDocumentationPage : PluginDocumentationPage
    {
        public PropertyInfoAttribute[] Connectors { get; private set; }

        public ComponentTemplateList RelevantTemplates { get; private set; }

        public ComponentDocumentationPage(Type componentType, XElement xml) : base(componentType, xml)
        {
            Connectors = PluginExtension.GetProperties(componentType);

            RelevantTemplates = new ComponentTemplateList();
            if (DocGenerator.RelevantComponentToTemplatesMap.ContainsKey(componentType.Name))
            {
                System.Collections.Generic.List<TemplateDocumentationPage> templates = DocGenerator.RelevantComponentToTemplatesMap[componentType.Name];
                foreach (TemplateDocumentationPage templateDocumentationPage in templates)
                {
                    RelevantTemplates.Add(templateDocumentationPage);
                }
            }
        }

        protected override LocalizedPluginDocumentationPage CreateLocalizedEntityDocumentationPage(PluginDocumentationPage pluginDocumentationPage, Type componentType, XElement xml, string lang, BitmapFrame componentImage)
        {
            if (pluginDocumentationPage is ComponentDocumentationPage)
            {
                return new LocalizedComponentDocumentationPage((ComponentDocumentationPage)pluginDocumentationPage,
                                                               componentType, xml, lang, componentImage);
            }
            return null;
        }
    }
}
