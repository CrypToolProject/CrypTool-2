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
using OnlineDocumentationGenerator.DocInformations.Localization;
using OnlineDocumentationGenerator.DocInformations.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace OnlineDocumentationGenerator.DocInformations
{
    public class TemplateDocumentationPage : EntityDocumentationPage
    {
        private readonly string _relativeTemplateDirectory;

        public string RelativeTemplateDirectory => _relativeTemplateDirectory;

        public string TemplateFile { get; private set; }
        public TemplateDirectory TemplateDir { get; private set; }
        public XElement TemplateXML { get; private set; }
        public List<string> RelevantPlugins { get; private set; }
        public string Icon { get; private set; }

        public override string Name
        {
            get
            {
                string flattenedPath = RelativeTemplateDirectory.Replace(Path.DirectorySeparatorChar, '.');
                return string.Format("{0}.{1}", flattenedPath, Path.GetFileNameWithoutExtension(TemplateFile));
            }
        }

        public string AuthorName { get; protected set; }

        public override string DocDirPath => DocGenerator.TemplateDirectory;

        public new LocalizedTemplateDocumentationPage CurrentLocalization => (LocalizedTemplateDocumentationPage)base.CurrentLocalization;

        public TemplateDocumentationPage(
            string templateFilePath,
            string relativeTemplateFilePath,
            string relativeTemplateDirectory,
            TemplateDirectory templateDir)
        {
            _relativeTemplateDirectory = relativeTemplateDirectory;
            TemplateFile = relativeTemplateFilePath;
            TemplateDir = templateDir;

            string templateXMLFile = Path.Combine(Path.GetDirectoryName(templateFilePath), Path.GetFileNameWithoutExtension(templateFilePath) + ".xml");
            if (!File.Exists(templateXMLFile))
            {
                throw new Exception(string.Format("Missing meta infos for template {0}!", templateFilePath));
            }

            TemplateXML = XElement.Load(templateXMLFile);

            BitmapFrame icon = null;
            if (TemplateXML.Element("icon") != null && TemplateXML.Element("icon").Attribute("file") != null)
            {
                string iconFile = Path.Combine(Path.GetDirectoryName(templateFilePath), TemplateXML.Element("icon").Attribute("file").Value);
                if (iconFile == null || !File.Exists(iconFile))
                {
                    iconFile = Path.Combine(Path.GetDirectoryName(templateFilePath), Path.GetFileNameWithoutExtension(templateFilePath) + ".png");
                }
                if (File.Exists(iconFile))
                {
                    try
                    {
                        icon = BitmapFrame.Create(new BitmapImage(new Uri(iconFile)));
                        Icon = iconFile;
                    }
                    catch (Exception)
                    {
                        icon = null;
                        Icon = "";
                    }
                }
            }

            XElement authorElement = XMLHelper.FindLocalizedChildElement(TemplateXML, "author");
            if (authorElement != null)
            {
                AuthorName = authorElement.Value;
            }

            XElement relevantPlugins = TemplateXML.Element("relevantPlugins");
            if (relevantPlugins != null)
            {
                RelevantPlugins = new List<string>();
                foreach (XElement plugin in relevantPlugins.Elements("plugin"))
                {
                    XAttribute name = plugin.Attribute("name");
                    if (name != null)
                    {
                        RelevantPlugins.Add(name.Value);
                    }
                }
            }

            foreach (XElement title in TemplateXML.Elements("title"))
            {
                XAttribute langAtt = title.Attribute("lang");
                if (langAtt != null && !AvailableLanguages.Contains(langAtt.Value))
                {
                    Localizations.Add(langAtt.Value, new LocalizedTemplateDocumentationPage(this, langAtt.Value, icon));
                }
            }
            if (!Localizations.ContainsKey("en"))
            {
                throw new Exception("Documentation should at least support english language!");
            }
        }
    }
}