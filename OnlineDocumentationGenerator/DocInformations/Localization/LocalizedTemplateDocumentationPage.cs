using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using OnlineDocumentationGenerator.DocInformations.Utils;
using OnlineDocumentationGenerator.Generators.HtmlGenerator;
using OnlineDocumentationGenerator.Utils;

namespace OnlineDocumentationGenerator.DocInformations.Localization
{
    public class LocalizedTemplateDocumentationPage : LocalizedEntityDocumentationPage
    {
        private XElement _xml;
        private string _filePath;

        public new TemplateDocumentationPage DocumentationPage { get { return base.DocumentationPage as TemplateDocumentationPage; } }

        public override string FilePath
        {
            get { return _filePath; }
        }

        public string TemplateFile
        {
            get
            { 
                return DocumentationPage.TemplateFile;
            }
        }
        public XElement Summary { get; private set; }
        public XElement Description { get; private set; }

        public string AuthorName
        {
            get { return DocumentationPage.AuthorName; }
        }

        public XElement SummaryOrDescription
        {
            get
            {
                if (Summary != null)
                    return Summary;
                if (Description != null)
                    return Description;
                return null;
            }
        }

        public List<string> CategoryPathList()
        {
            List<string> categories = new List<string>();
            TemplateDirectory tdir = ((TemplateDocumentationPage)DocumentationPage).TemplateDir;

            while (tdir != null)
            {
                string name;
                try
                {
                    name = tdir.LocalizedInfos[Lang].Name;
                    name = name.Trim();
                }
                catch (System.Exception ex)
                {
                    name = "???";
                }
                categories.Insert(0, name);
                tdir = tdir.Parent;
            }

            return categories;
        }

        public string CategoryPath()
        {
            return string.Join("\\ ", CategoryPathList().Skip(1));
        }

        public LocalizedTemplateDocumentationPage(TemplateDocumentationPage templateDocumentationPage, string lang, BitmapFrame icon)
        {
            base.DocumentationPage = templateDocumentationPage;
            Lang = lang;
            Icon = icon;
            _xml = templateDocumentationPage.TemplateXML;
            _filePath = OnlineHelp.GetTemplateDocFilename(Path.Combine(templateDocumentationPage.RelativeTemplateDirectory, Path.GetFileName(templateDocumentationPage.TemplateFile)), lang);

            CultureInfo cultureInfo = CultureInfoHelper.GetCultureInfo(lang);
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            var titleElement = XMLHelper.FindLocalizedChildElement(_xml, "title");
            if (titleElement != null)
            {
                Name = titleElement.Value;
            }
            
            Summary = XMLHelper.FindLocalizedChildElement(_xml, "summary");
            Description = XMLHelper.FindLocalizedChildElement(_xml, "description");
        }
    }
}