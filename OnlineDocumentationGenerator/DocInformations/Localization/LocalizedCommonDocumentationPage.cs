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
using System;
using System.Globalization;
using System.Threading;
using System.Xml.Linq;

namespace OnlineDocumentationGenerator.DocInformations.Localization
{
    public class LocalizedCommonDocumentationPage : LocalizedEntityDocumentationPage
    {
        private readonly XElement _xml;

        public override string FilePath => OnlineHelp.GetCommonDocFilename(DocumentationPage.Name, Lang);

        public XElement Description { get; private set; }

        public LocalizedCommonDocumentationPage(CommonDocumentationPage commonDocumentationPage, XElement xml, string lang)
        {
            CultureInfo cultureInfo = CultureInfoHelper.GetCultureInfo(lang);
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            _xml = xml;
            Lang = lang;
            DocumentationPage = commonDocumentationPage;

            if (_xml != null)
            {
                XElement nameEl = XMLHelper.FindLocalizedChildElement(_xml, "name");
                if (nameEl == null)
                {
                    throw new NullReferenceException(string.Format("Error in {0}: Common documentation must provide name.", commonDocumentationPage.Name));
                }
                Name = nameEl.Value;
                Description = XMLHelper.FindLocalizedChildElement(_xml, "description");
            }
        }
    }
}