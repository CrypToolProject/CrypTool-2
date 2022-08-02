/*
   Copyright 2008-2022 CrypTool Team

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using CrypTool.PluginBase;
using Startcenter.Util;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Startcenter.Controls
{
    /// <summary>
    /// Interaction logic for Startcenter.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("Startcenter.Properties.Resources")]
    public partial class StartcenterControl : UserControl
    {
        private readonly PanelsControl _panelsObj;

        public string TemplatesDir
        {
            set => ((PanelsControl)panels.Children[0]).TemplatesDir = value;
        }

        public void ReloadTemplates(object sender, ExecutedRoutedEventArgs e)
        {
            Cursor _previousCursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                ((TemplatesControl)(((PanelsControl)panels.Children[0]).templates.Child)).ReloadTemplates();
            }
            catch (Exception)
            {
            }
            Mouse.OverrideCursor = _previousCursor;
        }

        public event OpenEditorHandler OnOpenEditor;
        public event OpenTabHandler OnOpenTab;
        public event EventHandler<TemplateOpenEventArgs> TemplateLoaded;

        public StartcenterControl()
        {
            InitializeComponent();
            ((MainFunctionButtonsControl)mainFunctionButtonsBorder.Child).OnOpenEditor += (content, info) => OnOpenEditor(content, info);
            _panelsObj = (PanelsControl)panels.Children[0];
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
