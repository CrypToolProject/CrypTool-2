using System;
using System.Windows.Controls;
using CrypTool.PluginBase;

namespace Startcenter
{
    /// <summary>
    /// Interaction logic for Panels.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("Startcenter.Properties.Resources")]
    public partial class Panels : UserControl
    {
        private Templates _templatesObj;

        public string TemplatesDir
        {
            set
            {
                ((Templates)templates.Child).TemplatesDir = value;
            }
        }

        public event OpenEditorHandler OnOpenEditor;
        public event OpenTabHandler OnOpenTab;
        public event EventHandler<TemplateOpenEventArgs> TemplateLoaded;

        public Panels()
        {
            InitializeComponent();
            ((LastOpenedFilesList)lastOpenedFilesList.Child).OnOpenEditor += (content, info) => OnOpenEditor(content, info);
            ((LastOpenedFilesList)lastOpenedFilesList.Child).OnOpenTab += (content, info, parent) => OnOpenTab(content, info, parent);
            ((LastOpenedFilesList)lastOpenedFilesList.Child).TemplateLoaded += new EventHandler<TemplateOpenEventArgs>(templateLoaded);
            _templatesObj = ((Templates) templates.Child);
            _templatesObj.TemplateLoaded += new EventHandler<TemplateOpenEventArgs>(templateLoaded);
            _templatesObj.OnOpenEditor += (content, info) => OnOpenEditor(content, info);
            _templatesObj.OnOpenTab += (content, title, parent) => OnOpenTab(content, title, parent);
        }

        void templateLoaded(object sender, TemplateOpenEventArgs e)
        {
            if (TemplateLoaded != null)
            {
                TemplateLoaded.Invoke(sender, e);
            }
        }

        public void ShowHelp()
        {
            _templatesObj.ShowHelp();
        }
    }
}
