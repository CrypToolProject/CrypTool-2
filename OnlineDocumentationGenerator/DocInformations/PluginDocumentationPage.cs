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
using CrypTool.PluginBase;
using OnlineDocumentationGenerator.DocInformations.Localization;
using OnlineDocumentationGenerator.DocInformations.Utils;
using OnlineDocumentationGenerator.Generators.HtmlGenerator;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace OnlineDocumentationGenerator.DocInformations
{
    public abstract class PluginDocumentationPage : EntityDocumentationPage
    {
        protected XElement _xml;
        public Type PluginType { get; protected set; }
        public string AuthorURL { get; protected set; }
        public string AuthorInstitute { get; protected set; }
        public string AuthorEmail { get; protected set; }
        public TaskPaneAttribute[] Settings { get; protected set; }
        public ComponentCategory Category { get; protected set; }

        public override string Name => PluginType.FullName;

        public override string DocDirPath => Path.GetDirectoryName(OnlineHelp.GetPluginDocFilename(PluginType, "en"));

        public PluginDocumentationPage(Type pluginType, XElement xml)
        {
            PluginType = pluginType;
            System.Windows.Media.ImageSource image = pluginType.GetImage(0).Source;
            if (xml == null)
            {
                _xml = GetXML(pluginType);
            }
            else
            {
                _xml = xml;
            }

            AuthorAttribute authorAttribut = pluginType.GetPluginAuthorAttribute();
            if (authorAttribut != null)
            {
                AuthorName = authorAttribut.Author;
                AuthorEmail = authorAttribut.Email;
                AuthorInstitute = authorAttribut.Institute;
                AuthorURL = authorAttribut.URL;
            }

            if (pluginType.GetComponentCategoryAttributes().Count() > 0)
            {
                Category = pluginType.GetComponentCategoryAttributes().First().Category;
            }
            else
            {
                Category = ComponentCategory.Undefined;
            }
            Settings = GetSettings(pluginType);

            if (_xml == null || _xml.Name != "documentation")
            {
                //entity doesn't have a proper _xml file
                _xml = null;
                Localizations.Add("en", CreateLocalizedEntityDocumentationPage(this, pluginType, null, "en", image as BitmapFrame));
            }
            else
            {
                foreach (string lang in XMLHelper.GetAvailableLanguagesFromXML(_xml.Elements("language").Select(langElement => langElement.Attribute("culture").Value)))
                {
                    Localizations.Add(lang, CreateLocalizedEntityDocumentationPage(this, pluginType, _xml, lang, image as BitmapFrame));
                }
                if (!Localizations.ContainsKey("en"))
                {
                    throw new Exception("Documentation should at least support english language!");
                }

                References = XMLHelper.ReadReferences(_xml);
            }
        }

        //Factory Method:
        protected abstract LocalizedPluginDocumentationPage CreateLocalizedEntityDocumentationPage(PluginDocumentationPage pluginDocumentationPage, Type entityType, XElement xml, string lang, BitmapFrame entityImage);

        private TaskPaneAttribute[] GetSettings(Type entityType)
        {
            //Try to find out the settings class type of this entity type:
            MemberInfo[] members = entityType.GetMembers(BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (MemberInfo memberInfo in members)
            {
                if (memberInfo.MemberType == MemberTypes.Field)
                {
                    Type t = ((FieldInfo)memberInfo).FieldType;
                    if (t.GetInterfaces().Contains(typeof(ISettings)))
                    {
                        return t.GetSettingsProperties(entityType);
                    }
                }
            }
            return null;
        }

        public string AuthorName { get; protected set; }

        protected static XElement GetXML(Type type)
        {
            string descriptionUrl = type.GetPluginInfoAttribute().DescriptionUrl;
            if (descriptionUrl == null || Path.GetExtension(descriptionUrl).ToLower() != ".xml")
            {
                return null;
            }

            if (descriptionUrl != string.Empty)
            {
                int sIndex = descriptionUrl.IndexOf('/');
                Uri xmlUri = new Uri(string.Format("pack://application:,,,/{0};component/{1}",
                                                    descriptionUrl.Substring(0, sIndex), descriptionUrl.Substring(sIndex + 1)));
                Stream stream = Application.GetResourceStream(xmlUri).Stream;
                return XElement.Load(stream);
            }
            return null;
        }
    }
}