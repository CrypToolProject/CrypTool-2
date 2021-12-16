using CrypTool.PluginBase;
using OnlineDocumentationGenerator.Generators.HtmlGenerator;
using OnlineDocumentationGenerator.Utils;
using System;
using System.Globalization;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace OnlineDocumentationGenerator.DocInformations.Localization
{
    public abstract class LocalizedPluginDocumentationPage : LocalizedEntityDocumentationPage
    {
        protected XElement _xml;

        public new PluginDocumentationPage DocumentationPage => base.DocumentationPage as PluginDocumentationPage;

        public override string FilePath => OnlineHelp.GetPluginDocFilename(PluginType, Lang);

        public Type PluginType { get; private set; }
        public string ToolTip { get; private set; }

        public TaskPaneAttribute[] Settings => DocumentationPage.Settings;

        public string AuthorURL => DocumentationPage.AuthorURL;

        public string AuthorInstitute => DocumentationPage.AuthorInstitute;

        public string AuthorEmail => DocumentationPage.AuthorEmail;

        public string AuthorName => DocumentationPage.AuthorName;

        protected LocalizedPluginDocumentationPage(PluginDocumentationPage pluginDocumentationPage, Type pluginType, XElement xml, string lang, BitmapFrame icon)
        {
            base.DocumentationPage = pluginDocumentationPage;
            PluginType = pluginType;
            _xml = xml;
            Lang = lang;
            Icon = icon;

            CultureInfo cultureInfo = CultureInfoHelper.GetCultureInfo(lang);
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            Name = pluginType.GetPluginInfoAttribute().Caption;
            ToolTip = pluginType.GetPluginInfoAttribute().ToolTip;
        }
    }
}