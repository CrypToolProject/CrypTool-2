using OnlineDocumentationGenerator.DocInformations.Localization;
using OnlineDocumentationGenerator.DocInformations.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace OnlineDocumentationGenerator.DocInformations
{
    public class CommonDocumentationPage : EntityDocumentationPage
    {
        private readonly XElement _xml;

        public int Id
        {
            get;
            set;
        }

        public override string Name => Localizations["en"].Name;

        public override string DocDirPath => DocGenerator.CommonDirectory;

        public CommonDocumentationPage(int id, XElement xml)
        {
            _xml = xml;
            Id = id;

            Localizations = new Dictionary<string, LocalizedEntityDocumentationPage>();

            if (_xml != null && _xml.Name == "documentation")
            {
                foreach (string lang in XMLHelper.GetAvailableLanguagesFromXML(_xml.Elements("language").Select(langElement => langElement.Attribute("culture").Value)))
                {
                    Localizations.Add(lang, new LocalizedCommonDocumentationPage(this, _xml, lang));
                }

                if (!Localizations.ContainsKey("en"))
                {
                    throw new Exception("Documentation should at least support english language!");
                }

                References = XMLHelper.ReadReferences(_xml);
            }
            else
            {
                throw new Exception("Error while trying to read common documentation page.");
            }
        }
    }
}