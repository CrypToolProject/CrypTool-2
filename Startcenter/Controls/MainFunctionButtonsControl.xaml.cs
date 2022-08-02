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
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Startcenter.Controls
{
    /// <summary>
    /// Interaction logic for MainFunctionButtons.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("Startcenter.Properties.Resources")]
    public partial class MainFunctionButtonsControl : UserControl
    {
        public event OpenEditorHandler OnOpenEditor;

        public MainFunctionButtonsControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Helper method to open a new editor tab with appropriate title, tooltip, and icon
        /// </summary>
        /// <param name="editorType"></param>
        private void DoOpenEditor(Type editorType)
        {
            if (OnOpenEditor == null)
            {
                return;
            }
            Span tooltip = new Span();
            tooltip.Inlines.Add(editorType.GetPluginInfoAttribute().ToolTip);
            TabInfo tabInfo = new TabInfo { Title = editorType.GetPluginInfoAttribute().Caption, Icon = editorType.GetImage(0).Source, Tooltip = tooltip };
            OnOpenEditor(editorType, tabInfo);
        }

        private void WizardButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DoOpenEditor(typeof(Wizard.Wizard));
            }
            catch (Exception)
            {
                //do nothing
            }
        }


        private void WorkspaceButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DoOpenEditor(typeof(WorkspaceManager.WorkspaceManagerClass));
            }
            catch (Exception)
            {
                //do nothing
            }
        }
    }
}
