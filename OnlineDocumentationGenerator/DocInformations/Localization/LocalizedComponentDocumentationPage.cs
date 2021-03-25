using System;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using CrypTool.PluginBase;
using OnlineDocumentationGenerator.DocInformations.Utils;

namespace OnlineDocumentationGenerator.DocInformations.Localization
{
    public class LocalizedComponentDocumentationPage : LocalizedPluginDocumentationPage
    {
        public PropertyInfoAttribute[] Connectors {
            get { return DocumentationPage.Connectors; }
        }
        
        public ComponentTemplateList Templates { 
            get
            {
                return DocumentationPage.RelevantTemplates;
            }
        }

        public new ComponentDocumentationPage DocumentationPage
        {
            get
            {
                return (ComponentDocumentationPage)base.DocumentationPage;
            }
        }

        public XElement Introduction { get; private set; }
        public XElement Manual { get; private set; }
        public XElement Presentation { get; private set; }

        public LocalizedComponentDocumentationPage(ComponentDocumentationPage componentDocumentationPage, Type pluginType, XElement xml, string lang, BitmapFrame icon)
            : base(componentDocumentationPage, pluginType, xml, lang, icon)
        {
            if (_xml != null)
                ReadInformationsFromXML();
        }

        private void ReadInformationsFromXML()
        {
            Introduction = XMLHelper.FindLocalizedChildElement(_xml, "introduction");
            Manual = XMLHelper.FindLocalizedChildElement(_xml, "usage");
            Presentation = XMLHelper.FindLocalizedChildElement(_xml, "presentation");
        }
    }
}
