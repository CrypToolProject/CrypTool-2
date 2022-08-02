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
using System.Windows.Controls;

namespace Startcenter.Controls
{
    /// <summary>
    /// Interaction logic for Panels.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("Startcenter.Properties.Resources")]
    public partial class PanelsControl : UserControl
    {
        private readonly TemplatesControl _templatesObj;

        public string TemplatesDir
        {
            set => ((TemplatesControl)templates.Child).TemplatesDir = value;
        }

        public event OpenEditorHandler OnOpenEditor;
        public event OpenTabHandler OnOpenTab;
        public event EventHandler<TemplateOpenEventArgs> TemplateLoaded;

        public PanelsControl()
        {
            InitializeComponent();
            ((LastOpenedFilesListControl)lastOpenedFilesList.Child).OnOpenEditor += (content, info) => OnOpenEditor(content, info);
            ((LastOpenedFilesListControl)lastOpenedFilesList.Child).OnOpenTab += (content, info, parent) => OnOpenTab(content, info, parent);
            ((LastOpenedFilesListControl)lastOpenedFilesList.Child).TemplateLoaded += new EventHandler<TemplateOpenEventArgs>(templateLoaded);
            _templatesObj = ((TemplatesControl)templates.Child);
            _templatesObj.TemplateLoaded += new EventHandler<TemplateOpenEventArgs>(templateLoaded);
            _templatesObj.OnOpenEditor += (content, info) => OnOpenEditor(content, info);
            _templatesObj.OnOpenTab += (content, title, parent) => OnOpenTab(content, title, parent);
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
            _templatesObj.ShowHelp();
        }
    }
}
