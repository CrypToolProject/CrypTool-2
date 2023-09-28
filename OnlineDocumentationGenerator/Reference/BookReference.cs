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
using System.Globalization;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml.Linq;

namespace OnlineDocumentationGenerator.Reference
{
    public class BookReference : Reference
    {
        public string Name => GetLocalizedProperty("Name", Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName);

        public string Author => GetLocalizedProperty("Author", Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName);

        public string Publisher => GetLocalizedProperty("Publisher", Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName);

        public string Year => GetLocalizedProperty("Year", Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName);

        public BookReference(XElement linkReferenceElement) : base(linkReferenceElement)
        {
            foreach (XElement e in linkReferenceElement.Elements())
            {
                string lang = "en";
                if (e.Attribute("lang") != null)
                {
                    CultureInfo cult = new CultureInfo(e.Attribute("lang").Value);
                    lang = cult.TwoLetterISOLanguageName;
                }

                if (e.Name == "author")
                {
                    SetLocalizedProperty("Author", lang, e.Value);
                }
                else if (e.Name == "publisher")
                {
                    SetLocalizedProperty("Publisher", lang, e.Value);
                }
                else if (e.Name == "name")
                {
                    SetLocalizedProperty("Name", lang, e.Value);
                }
                else if (e.Name == "year")
                {
                    SetLocalizedProperty("Year", lang, e.Value);
                }
            }
        }

        public override string ToHTML(string lang)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(HttpUtility.HtmlEncode(Author));
            builder.Append(". ");
            builder.Append(HttpUtility.HtmlEncode(Name));
            builder.Append(". ");
            builder.Append("<i>" + HttpUtility.HtmlEncode(Publisher) + "</i>");
            builder.Append(" (" + HttpUtility.HtmlEncode(Year) + ")");
            return builder.ToString();
        }
    }
}
