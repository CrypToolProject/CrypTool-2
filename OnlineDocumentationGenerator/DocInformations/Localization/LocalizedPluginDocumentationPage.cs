using System;
using System.Globalization;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using CrypTool.PluginBase;
using OnlineDocumentationGenerator.Generators.HtmlGenerator;
using OnlineDocumentationGenerator.Utils;

namespace OnlineDocumentationGenerator.DocInformations.Localization
{
    public abstract class LocalizedPluginDocumentationPage : LocalizedEntityDocumentationPage
    {
        protected XElement _xml;

        public new PluginDocumentationPage DocumentationPage { get { return base.DocumentationPage as PluginDocumentationPage; }}

        public override string FilePath
        {
            get { return OnlineHelp.GetPluginDocFilename(PluginType, Lang); }
        }

        public Type PluginType { get; private set; }
        public string ToolTip { get; private set; }

        public TaskPaneAttribute[] Settings
        {
            get { return DocumentationPage.Settings; }
        }

        public string AuthorURL
        { 
            get { return DocumentationPage.AuthorURL; }
        }

        public string AuthorInstitute
        {
            get { return DocumentationPage.AuthorInstitute; }
        }

        public string AuthorEmail
        {
            get { return DocumentationPage.AuthorEmail; }
        }

        public string AuthorName
        {
            get { return DocumentationPage.AuthorName; }
        }

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