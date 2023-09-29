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
using OnlineDocumentationGenerator.Reference;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

namespace OnlineDocumentationGenerator.DocInformations.Utils
{
    internal static class XMLHelper
    {
        public static XElement FindLocalizedChildElement(XElement element, string xname)
        {
            const string defaultLang = "en";
            CultureInfo currentLang = Thread.CurrentThread.CurrentUICulture;

            IEnumerable<XElement> allElements = element.Elements(xname);
            IEnumerable<XElement> foundElements = null;

            if (allElements.Any())
            {
                foundElements = from descln in allElements
                                where (new CultureInfo(descln.Attribute("lang").Value)).TwoLetterISOLanguageName.Equals(currentLang.TwoLetterISOLanguageName)
                                select descln;
                if (!foundElements.Any())
                {
                    foundElements = from descln in allElements
                                    where (new CultureInfo(descln.Attribute("lang").Value)).TwoLetterISOLanguageName == defaultLang
                                    select descln;
                }
            }

            if (foundElements == null || !foundElements.Any() || !allElements.Any())
            {
                if (!allElements.Any())
                {
                    return null;
                }
                else
                {
                    return allElements.First();
                }
            }

            return foundElements.First();
        }

        public static ReferenceList ReadReferences(XElement xml)
        {
            if (xml.Element("references") != null)
            {
                ReferenceList references = new ReferenceList();

                foreach (XElement refs in xml.Element("references").Elements())
                {
                    switch (refs.Name.ToString())
                    {
                        case "linkReference":
                            references.Add(new LinkReference(refs));
                            break;
                        case "bookReference":
                            references.Add(new BookReference(refs));
                            break;
                    }
                }

                return references;
            }
            return null;
        }

        public static IEnumerable<string> GetAvailableLanguagesFromXML(IEnumerable<string> xml)
        {
            IEnumerable<string> langs = xml;
            return langs.Select(lang => new CultureInfo(lang)).Select(cult => cult.TwoLetterISOLanguageName);
        }
    }
}
