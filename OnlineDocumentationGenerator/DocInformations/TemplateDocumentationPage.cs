using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using OnlineDocumentationGenerator.DocInformations.Localization;
using OnlineDocumentationGenerator.DocInformations.Utils;

namespace OnlineDocumentationGenerator.DocInformations
{
    public class TemplateDocumentationPage : EntityDocumentationPage
    {
        private readonly string _relativeTemplateDirectory;

        public string RelativeTemplateDirectory
        {
            get { return _relativeTemplateDirectory; }
        }

        public string TemplateFile { get; private set; }
        public TemplateDirectory TemplateDir { get; private set; }
        public XElement TemplateXML { get; private set; }
        public List<string> RelevantPlugins { get; private set; }
        public string Icon { get; private set; }

        public override string Name
        {
            get
            {
                var flattenedPath = RelativeTemplateDirectory.Replace(Path.DirectorySeparatorChar, '.');
                return string.Format("{0}.{1}", flattenedPath, Path.GetFileNameWithoutExtension(TemplateFile));
            }
        }

        public string AuthorName { get; protected set; }

        public override string DocDirPath
        {
            get { return DocGenerator.TemplateDirectory; }
        }

        public new LocalizedTemplateDocumentationPage CurrentLocalization
        {
            get { return (LocalizedTemplateDocumentationPage) base.CurrentLocalization; }
        }

        public TemplateDocumentationPage(
            string templateFilePath, 
            string relativeTemplateFilePath, 
            string relativeTemplateDirectory, 
            TemplateDirectory templateDir)
        {
            _relativeTemplateDirectory = relativeTemplateDirectory;
            TemplateFile = relativeTemplateFilePath;
            TemplateDir = templateDir;
            
            string templateXMLFile = Path.Combine(Path.GetDirectoryName(templateFilePath), Path.GetFileNameWithoutExtension(templateFilePath) + ".xml");
            if (!File.Exists(templateXMLFile))
                throw new Exception(string.Format("Missing meta infos for template {0}!", templateFilePath));

            TemplateXML = XElement.Load(templateXMLFile);

            BitmapFrame icon = null;
            if (TemplateXML.Element("icon") != null && TemplateXML.Element("icon").Attribute("file") != null)
            {
                var iconFile = Path.Combine(Path.GetDirectoryName(templateFilePath), TemplateXML.Element("icon").Attribute("file").Value);
                if (iconFile == null || !File.Exists(iconFile))
                {
                    iconFile = Path.Combine(Path.GetDirectoryName(templateFilePath), Path.GetFileNameWithoutExtension(templateFilePath) + ".png");
                }
                if (File.Exists(iconFile))
                {
                    try
                    {
                        icon = BitmapFrame.Create(new BitmapImage(new Uri(iconFile)));
                        Icon = iconFile;
                    }
                    catch (Exception)
                    {
                        icon = null;
                        Icon = "";
                    }
                }
            }

            var authorElement = XMLHelper.FindLocalizedChildElement(TemplateXML, "author");
            if (authorElement != null)
            {
                AuthorName = authorElement.Value;
            }

            var relevantPlugins = TemplateXML.Element("relevantPlugins");
            if (relevantPlugins != null)
            {
                RelevantPlugins = new List<string>();
                foreach (var plugin in relevantPlugins.Elements("plugin"))
                {
                    var name = plugin.Attribute("name");
                    if (name != null)
                    {
                        RelevantPlugins.Add(name.Value);
                    }
                }
            }
                
            foreach (var title in TemplateXML.Elements("title"))
            {
                var langAtt = title.Attribute("lang");
                if (langAtt != null && !AvailableLanguages.Contains(langAtt.Value))
                {
                    Localizations.Add(langAtt.Value, new LocalizedTemplateDocumentationPage(this, langAtt.Value, icon));
                }
            }
            if (!Localizations.ContainsKey("en"))
            {
                throw new Exception("Documentation should at least support english language!");
            }
        }
    }
}