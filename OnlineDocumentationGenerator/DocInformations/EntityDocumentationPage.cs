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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace OnlineDocumentationGenerator.DocInformations
{
    public abstract class EntityDocumentationPage
    {
        public Dictionary<string, LocalizedEntityDocumentationPage> Localizations { get; protected set; }

        public abstract string Name { get; }
        public abstract string DocDirPath { get; }

        public Reference.ReferenceList References { get; protected set; }

        public List<string> AvailableLanguages => Localizations.Keys.ToList();

        public LocalizedEntityDocumentationPage CurrentLocalization
        {
            get
            {
                string lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                if (Localizations.ContainsKey(lang))
                {
                    return Localizations[lang];
                }
                return Localizations["en"];
            }
        }

        protected EntityDocumentationPage()
        {
            Localizations = new Dictionary<string, LocalizedEntityDocumentationPage>();
        }
    }
}