using System;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using CrypTool.PluginBase;
using OnlineDocumentationGenerator.DocInformations.Localization;
using OnlineDocumentationGenerator.DocInformations.Utils;

namespace OnlineDocumentationGenerator.DocInformations
{
    public class ComponentDocumentationPage : PluginDocumentationPage
    {
        public PropertyInfoAttribute[] Connectors { get; private set; }

        public ComponentTemplateList RelevantTemplates { get; private set; }

        public ComponentDocumentationPage(Type componentType, XElement xml) : base(componentType, xml)
        {
            Connectors = PluginExtension.GetProperties(componentType);
            
            RelevantTemplates = new ComponentTemplateList();
            if (DocGenerator.RelevantComponentToTemplatesMap.ContainsKey(componentType.Name))
            {
                var templates = DocGenerator.RelevantComponentToTemplatesMap[componentType.Name];
                foreach (var templateDocumentationPage in templates)
                {
                    RelevantTemplates.Add(templateDocumentationPage);
                }
            }
        }

        protected override LocalizedPluginDocumentationPage CreateLocalizedEntityDocumentationPage(PluginDocumentationPage pluginDocumentationPage, Type componentType, XElement xml, string lang, BitmapFrame componentImage)
        {
            if (pluginDocumentationPage is ComponentDocumentationPage)
            {
                return new LocalizedComponentDocumentationPage((ComponentDocumentationPage) pluginDocumentationPage,
                                                               componentType, xml, lang, componentImage);
            }
            return null;
        }
    }
}
