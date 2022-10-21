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
using System.Collections.Generic;
using System.Xml.Linq;

namespace OnlineDocumentationGenerator.Reference
{
    public abstract class Reference
    {
        private readonly Dictionary<string, Dictionary<string, string>> _localizedPropertyStore = new Dictionary<string, Dictionary<string, string>>();
        public string ID { get; private set; }

        protected Reference(XElement linkReferenceElement)
        {
            XAttribute idAtt = linkReferenceElement.Attribute("id");
            if (idAtt != null)
            {
                ID = idAtt.Value;
            }
        }

        protected string GetLocalizedProperty(string property, string language)
        {
            if (!_localizedPropertyStore.ContainsKey(property))
            {
                return null;
            }

            if (!_localizedPropertyStore[property].ContainsKey(language))
            {
                if (!_localizedPropertyStore[property].ContainsKey("en"))
                {
                    return null;
                }

                return _localizedPropertyStore[property]["en"];
            }
            return _localizedPropertyStore[property][language];
        }

        protected void SetLocalizedProperty(string property, string language, string value)
        {
            if (!_localizedPropertyStore.ContainsKey(property))
            {
                _localizedPropertyStore.Add(property, new Dictionary<string, string>());
            }

            if (!_localizedPropertyStore[property].ContainsKey(language))
            {
                _localizedPropertyStore[property].Add(language, value);
            }
            else
            {
                _localizedPropertyStore[property][language] = value;
            }
        }

        public abstract string ToHTML(string lang);
    }
}
