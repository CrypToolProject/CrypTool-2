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
using OnlineDocumentationGenerator.DocInformations.Utils;
using OnlineDocumentationGenerator.Generators.HtmlGenerator;
using OnlineDocumentationGenerator.Utils;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace OnlineDocumentationGenerator.DocInformations.Localization
{
    public class LocalizedTemplateDocumentationPage : LocalizedEntityDocumentationPage
    {
        private readonly XElement _xml;
        private readonly string _filePath;

        public new TemplateDocumentationPage DocumentationPage => base.DocumentationPage as TemplateDocumentationPage;

        public override string FilePath => _filePath;

        public string TemplateFile => DocumentationPage.TemplateFile;
        public XElement Summary { get; private set; }
        public XElement Description { get; private set; }

        public string AuthorName => DocumentationPage.AuthorName;

        public XElement SummaryOrDescription
        {
            get
            {
                if (Summary != null)
                {
                    return Summary;
                }

                if (Description != null)
                {
                    return Description;
                }

                return null;
            }
        }

        public List<string> CategoryPathList()
        {
            List<string> categories = new List<string>();
            TemplateDirectory tdir = DocumentationPage.TemplateDir;

            while (tdir != null)
            {
                string name;
                try
                {
                    name = tdir.LocalizedInfos[Lang].Name;
                    name = name.Trim();
                }
                catch (System.Exception)
                {
                    name = "???";
                }
                categories.Insert(0, name);
                tdir = tdir.Parent;
            }

            return categories;
        }

        public string CategoryPath()
        {
            return string.Join("\\ ", CategoryPathList().Skip(1));
        }

        public LocalizedTemplateDocumentationPage(TemplateDocumentationPage templateDocumentationPage, string lang, BitmapFrame icon)
        {
            base.DocumentationPage = templateDocumentationPage;
            Lang = lang;
            Icon = icon;
            _xml = templateDocumentationPage.TemplateXML;
            _filePath = OnlineHelp.GetTemplateDocFilename(Path.Combine(templateDocumentationPage.RelativeTemplateDirectory, Path.GetFileName(templateDocumentationPage.TemplateFile)), lang);

            CultureInfo cultureInfo = CultureInfoHelper.GetCultureInfo(lang);
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            XElement titleElement = XMLHelper.FindLocalizedChildElement(_xml, "title");
            if (titleElement != null)
            {
                Name = titleElement.Value;
            }

            Summary = XMLHelper.FindLocalizedChildElement(_xml, "summary");
            Description = XMLHelper.FindLocalizedChildElement(_xml, "description");
        }
    }
}