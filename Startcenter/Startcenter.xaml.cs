using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CrypTool.PluginBase;
using StartCenter;
using System.IO;

namespace Startcenter
{
    /// <summary>
    /// Interaction logic for Startcenter.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("Startcenter.Properties.Resources")]
    public partial class Startcenter : UserControl
    {
        private Panels _panelsObj;

        public string TemplatesDir
        {
            set 
            {
                ((Panels)panels.Children[0]).TemplatesDir = value;
            }
        }
        
        public void ReloadTemplates(Object sender, ExecutedRoutedEventArgs e)
        {
            Cursor _previousCursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                ((Templates)(((Panels)panels.Children[0]).templates.Child)).ReloadTemplates();
            }
            catch(Exception ex)
            {
            }
            Mouse.OverrideCursor = _previousCursor;
        }

        public event OpenEditorHandler OnOpenEditor;
        public event OpenTabHandler OnOpenTab;
        public event StartcenterEditor.StartupBehaviourChangedHandler StartupBehaviourChanged;
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

        void templateLoaded(object sender, TemplateOpenEventArgs e)
        {
            if (TemplateLoaded != null)
            {
                TemplateLoaded.Invoke(sender, e);
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (StartupBehaviourChanged != null && StartupCheckbox.IsChecked.HasValue)
                StartupBehaviourChanged(StartupCheckbox.IsChecked.Value);
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

                foreach (var path in filePaths)
                {
                    // we only open existing files that names end with cwm
                    if (System.IO.File.Exists(path) && path.ToLower().EndsWith("cwm"))
                    {
                        var info = new TabInfo()
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
