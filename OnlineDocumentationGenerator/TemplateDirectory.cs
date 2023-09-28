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
using OnlineDocumentationGenerator.DocInformations;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace OnlineDocumentationGenerator
{
    public class TemplateDirectory
    {
        public class LocalizedTemplateInfos
        {
            public string Lang { get; internal set; }
            public string Name { get; internal set; }
            public XElement Summary { get; internal set; }
        }

        public List<TemplateDocumentationPage> ContainingTemplateDocPages { get; private set; }
        public List<TemplateDirectory> SubDirectories { get; private set; }
        public TemplateDirectory Parent { get; private set; }
        public Dictionary<string, LocalizedTemplateInfos> LocalizedInfos { get; private set; }
        public BitmapFrame DirIcon { get; private set; }
        public int Order { get; private set; }

        public string GetName(string lang)
        {
            if (LocalizedInfos.ContainsKey(lang) && LocalizedInfos[lang].Name != null)
            {
                return LocalizedInfos[lang].Name;
            }
            else
            {
                return LocalizedInfos["en"].Name;
            }
        }

        public string GetName()
        {
            return GetName(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
        }

        public XElement GetSummary(string lang)
        {
            if (LocalizedInfos.ContainsKey(lang) && LocalizedInfos[lang].Summary != null)
            {
                return LocalizedInfos[lang].Summary;
            }
            else
            {
                return LocalizedInfos["en"].Summary;
            }
        }

        public XElement GetSummary()
        {
            return GetSummary(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
        }

        public override string ToString()
        {
            return string.Format("{0} - {1}", GetName(), GetSummary());
        }

        public TemplateDirectory(DirectoryInfo directory, TemplateDirectory parent = null)
        {
            ContainingTemplateDocPages = new List<TemplateDocumentationPage>();
            SubDirectories = new List<TemplateDirectory>();
            LocalizedInfos = new Dictionary<string, LocalizedTemplateInfos>
            {
                { "en", new LocalizedTemplateInfos() { Lang = "en", Name = directory.Name } }
            };

            Parent = parent;

            //Read metainfos:
            FileInfo[] metainfo = directory.GetFiles("dir.xml");
            if (metainfo.Length > 0)
            {
                XElement metaXML = XElement.Load(metainfo[0].FullName);

                foreach (XElement nameElements in metaXML.Elements("name"))
                {
                    XAttribute langAtt = nameElements.Attribute("lang");
                    if (langAtt != null)
                    {
                        if (LocalizedInfos.ContainsKey(langAtt.Value))
                        {
                            LocalizedInfos[langAtt.Value].Name = nameElements.Value;
                        }
                        else
                        {
                            LocalizedInfos.Add(langAtt.Value, new LocalizedTemplateInfos() { Lang = langAtt.Value, Name = nameElements.Value });
                        }
                    }
                }

                foreach (XElement summaryElements in metaXML.Elements("summary"))
                {
                    XAttribute langAtt = summaryElements.Attribute("lang");
                    if (langAtt != null)
                    {
                        if (LocalizedInfos.ContainsKey(langAtt.Value))
                        {
                            LocalizedInfos[langAtt.Value].Summary = summaryElements;
                        }
                        else
                        {
                            LocalizedInfos.Add(langAtt.Value, new LocalizedTemplateInfos() { Lang = langAtt.Value, Summary = summaryElements });
                        }
                    }
                }

                if (metaXML.Element("icon") != null && metaXML.Element("icon").Attribute("file") != null)
                {
                    string iconFile = Path.Combine(directory.FullName, metaXML.Element("icon").Attribute("file").Value);
                    if (File.Exists(iconFile))
                    {
                        DirIcon = BitmapFrame.Create(new BitmapImage(new Uri(iconFile)));
                    }
                }

                if (metaXML.Attribute("order") != null)
                {
                    try
                    {
                        Order = int.Parse(metaXML.Attribute("order").Value);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
    }
}
