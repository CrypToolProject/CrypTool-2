using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using OnlineDocumentationGenerator.DocInformations.Localization;

namespace OnlineDocumentationGenerator.DocInformations
{
    public abstract class EntityDocumentationPage
    {
        public Dictionary<string, LocalizedEntityDocumentationPage> Localizations { get; protected set; }

        public abstract string Name { get; }
        public abstract string DocDirPath { get; }

        public Reference.ReferenceList References { get; protected set; }

        public List<string> AvailableLanguages
        {
            get { return Localizations.Keys.ToList(); }
        }

        public LocalizedEntityDocumentationPage CurrentLocalization
        {
            get
            {
                var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
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