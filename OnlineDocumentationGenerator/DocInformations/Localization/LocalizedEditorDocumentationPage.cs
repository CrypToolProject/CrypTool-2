using OnlineDocumentationGenerator.DocInformations.Utils;
using System;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace OnlineDocumentationGenerator.DocInformations.Localization
{
    public class LocalizedEditorDocumentationPage : LocalizedPluginDocumentationPage
    {
        public XElement Manual { get; private set; }

        public LocalizedEditorDocumentationPage(EditorDocumentationPage editorDocumentationPage, Type pluginType, XElement xml, string lang, BitmapFrame icon)
            : base(editorDocumentationPage, pluginType, xml, lang, icon)
        {
            if (_xml != null)
            {
                ReadInformationsFromXML();
            }
        }

        private void ReadInformationsFromXML()
        {
            Manual = XMLHelper.FindLocalizedChildElement(_xml, "usage");
        }
    }
}
