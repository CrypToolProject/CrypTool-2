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
using System.Threading;
using System.Web;
using System.Xml.Linq;

namespace OnlineDocumentationGenerator.Reference
{
    public class LinkReference : Reference
    {
        public string Link => GetLocalizedProperty("Link", Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName);

        public string Caption => GetLocalizedProperty("Caption", Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName);

        public LinkReference(XElement linkReferenceElement) : base(linkReferenceElement)
        {
            foreach (XElement e in linkReferenceElement.Elements())
            {
                string lang = "en";
                if (e.Attribute("lang") != null)
                {
                    CultureInfo cult = new CultureInfo(e.Attribute("lang").Value);
                    lang = cult.TwoLetterISOLanguageName;
                }

                if (e.Name == "link")
                {
                    if (e.Attribute("url") != null)
                    {
                        SetLocalizedProperty("Link", lang, e.Attribute("url").Value);
                    }
                }
                else if (e.Name == "caption")
                {
                    SetLocalizedProperty("Caption", lang, e.Value);
                }
            }
        }

        public override string ToHTML(string lang)
        {
            return string.Format("{0} - <a href=\"{1}?external\"><img src=\"../external_link.png\" border=\"0\">{1}</a>", HttpUtility.HtmlEncode(Caption), Link);
        }
    }
}
