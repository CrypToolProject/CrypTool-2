/*
   Copyright (C) CrypTool 2 Team

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
using CrypTool.PluginBase.Attributes;
using System.Windows;
using System.Windows.Controls;

namespace Wizard
{
    /// <summary>
    /// Interaction logic for WizardSettingsTab.xaml
    /// </summary>
    [Localization("Wizard.Resources.settingsRes")]
    [SettingsTab("WizardSettings", "/MainSettings/")]
    public partial class WizardSettingsTab : UserControl
    {
        public WizardSettingsTab(Style settingsStyle)
        {
            Resources.Add("settingsStyle", settingsStyle);
            InitializeComponent();

            CrypTool.PluginBase.Properties.Settings.Default.PropertyChanged += delegate { CrypTool.PluginBase.Miscellaneous.ApplicationSettingsHelper.SaveApplicationsSettings(); };
        }
    }
}
