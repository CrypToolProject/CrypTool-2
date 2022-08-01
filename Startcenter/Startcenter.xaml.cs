using CrypTool.PluginBase;
using StartCenter;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Startcenter
{
    /// <summary>
    /// Interaction logic for Startcenter.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("Startcenter.Properties.Resources")]
    public partial class Startcenter : UserControl
    {
        private readonly Panels _panelsObj;

        public string TemplatesDir
        {
            set => ((Panels)panels.Children[0]).TemplatesDir = value;
        }

        public void ReloadTemplates(object sender, ExecutedRoutedEventArgs e)
        {
            Cursor _previousCursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                ((Templates)(((Panels)panels.Children[0]).templates.Child)).ReloadTemplates();
            }
            catch (Exception)
            {
            }
            Mouse.OverrideCursor = _previousCursor;
        }

        public event OpenEditorHandler OnOpenEditor;
        public event OpenTabHandler OnOpenTab;
        public event EventHandler<TemplateOpenEventArgs> TemplateLoaded;

        public Startcenter()
        {
            InitializeComponent();
            ((MainFunctionButtons)mainFunctionButtonsBorder.Child).OnOpenEditor += (content, info) => OnOpenEditor(content, info);
            _panelsObj = (Panels)panels.Children[0];
            _panelsObj.OnOpenEditor += (content, info) => OnOpenEditor(content, info);
            _panelsObj.OnOpenTab += (content, info, parent) => OnOpenTab(content, info, parent);
            _panelsObj.TemplateLoaded += new EventHandler<TemplateOpenEventArgs>(templateLoaded);
        }

        private void templateLoaded(object sender, TemplateOpenEventArgs e)
        {
            if (TemplateLoaded != null)
            {
                TemplateLoaded.Invoke(sender, e);
            }
        }

        public void ShowHelp()
        {
            _panelsObj.ShowHelp();
        }

        /// <summary>
        /// This handles the drop of files onto the Startcenter
        /// If a cwm-file is dropped, it opens a new WorkspaceManager
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PreviewDropHandler(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] filePaths = (string[])(e.Data.GetData(DataFormats.FileDrop));

                foreach (string path in filePaths)
                {
                    // we only open existing files that names end with cwm
                    if (System.IO.File.Exists(path) && path.ToLower().EndsWith("cwm"))
                    {
                        TabInfo info = new TabInfo()
                        {
                            Filename = new FileInfo(path)
                        };
                        TemplateLoaded.Invoke(this, new TemplateOpenEventArgs() { Type = typeof(WorkspaceManager.WorkspaceManagerClass), Info = info });
                    }
                }
            }
        }
    }
}
