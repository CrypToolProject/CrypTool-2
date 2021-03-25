using System;
using System.Globalization;
using System.Threading;
using System.Xml.Linq;
using OnlineDocumentationGenerator.DocInformations.Utils;
using OnlineDocumentationGenerator.Generators.HtmlGenerator;
using OnlineDocumentationGenerator.Utils;

namespace OnlineDocumentationGenerator.DocInformations.Localization
{
    public class LocalizedCommonDocumentationPage : LocalizedEntityDocumentationPage
    {
        private readonly XElement _xml;

        public override string FilePath
        {
            get { return OnlineHelp.GetCommonDocFilename(DocumentationPage.Name, Lang); }
        }

        public XElement Description { get; private set; }

        public LocalizedCommonDocumentationPage(CommonDocumentationPage commonDocumentationPage, XElement xml, string lang)
        {
            CultureInfo cultureInfo = CultureInfoHelper.GetCultureInfo(lang);
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            _xml = xml;
            Lang = lang;
            DocumentationPage = commonDocumentationPage;

            if (_xml != null)
            {
                var nameEl = XMLHelper.FindLocalizedChildElement(_xml, "name");
                if (nameEl == null)
                {
                    throw new NullReferenceException(string.Format("Error in {0}: Common documentation must provide name.", commonDocumentationPage.Name));
                }
                Name = nameEl.Value;
                Description = XMLHelper.FindLocalizedChildElement(_xml, "description");
            }
        }
    }
}