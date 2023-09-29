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
using System.Linq;
using System.Xml.Linq;

namespace OnlineDocumentationGenerator.DocInformations
{
    public class CommonDocumentationPage : EntityDocumentationPage
    {
        private readonly XElement _xml;

        public int Id
        {
            get;
            set;
        }

        public override string Name => Localizations["en"].Name;

        public override string DocDirPath => DocGenerator.CommonDirectory;

        public CommonDocumentationPage(int id, XElement xml)
        {
            _xml = xml;
            Id = id;

            Localizations = new Dictionary<string, LocalizedEntityDocumentationPage>();

            if (_xml != null && _xml.Name == "documentation")
            {
                foreach (string lang in XMLHelper.GetAvailableLanguagesFromXML(_xml.Elements("language").Select(langElement => langElement.Attribute("culture").Value)))
                {
                    Localizations.Add(lang, new LocalizedCommonDocumentationPage(this, _xml, lang));
                }

                if (!Localizations.ContainsKey("en"))
                {
                    throw new Exception("Documentation should at least support english language!");
                }

                References = XMLHelper.ReadReferences(_xml);
            }
            else
            {
                throw new Exception("Error while trying to read common documentation page.");
            }
        }
    }
}