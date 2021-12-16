using OnlineDocumentationGenerator.DocInformations.Localization;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace OnlineDocumentationGenerator.DocInformations
{
    public abstract class EntityDocumentationPage
    {
        public Dictionary<string, LocalizedEntityDocumentationPage> Localizations { get; protected set; }

        public abstract string Name { get; }
        public abstract string DocDirPath { get; }

        public Reference.ReferenceList References { get; protected set; }

        public List<string> AvailableLanguages => Localizations.Keys.ToList();

        public LocalizedEntityDocumentationPage CurrentLocalization
        {
            get
            {
                string lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                if (Localizations.ContainsKey(lang))
                {
                    return Localizations[lang];
                }
                return Localizations["en"];
            }
        }

        protected EntityDocumentationPage()
        {
            Localizations = new Dictionary<string, LocalizedEntityDocumentationPage>();
        }
    }
}