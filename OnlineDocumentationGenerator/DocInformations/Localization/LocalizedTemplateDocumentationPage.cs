using OnlineDocumentationGenerator.DocInformations.Utils;
using OnlineDocumentationGenerator.Generators.HtmlGenerator;
using OnlineDocumentationGenerator.Utils;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace OnlineDocumentationGenerator.DocInformations.Localization
{
    public class LocalizedTemplateDocumentationPage : LocalizedEntityDocumentationPage
    {
        private readonly XElement _xml;
        private readonly string _filePath;

        public new TemplateDocumentationPage DocumentationPage => base.DocumentationPage as TemplateDocumentationPage;

        public override string FilePath => _filePath;

        public string TemplateFile => DocumentationPage.TemplateFile;
        public XElement Summary { get; private set; }
        public XElement Description { get; private set; }

        public string AuthorName => DocumentationPage.AuthorName;

        public XElement SummaryOrDescription
        {
            get
            {
                if (Summary != null)
                {
                    return Summary;
                }

                if (Description != null)
                {
                    return Description;
                }

                return null;
            }
        }

        public List<string> CategoryPathList()
        {
            List<string> categories = new List<string>();
            TemplateDirectory tdir = DocumentationPage.TemplateDir;

            while (tdir != null)
            {
                string name;
                try
                {
                    name = tdir.LocalizedInfos[Lang].Name;
                    name = name.Trim();
                }
                catch (System.Exception)
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

            XElement titleElement = XMLHelper.FindLocalizedChildElement(_xml, "title");
            if (titleElement != null)
            {
                Name = titleElement.Value;
            }

            Summary = XMLHelper.FindLocalizedChildElement(_xml, "summary");
            Description = XMLHelper.FindLocalizedChildElement(_xml, "description");
        }
    }
}