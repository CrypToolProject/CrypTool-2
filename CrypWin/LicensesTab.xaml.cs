/*
   Copyright 2008 - 2022 CrypTool Team

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
using System.Windows.Controls;

namespace CrypTool.CrypWin
{
    /// <summary>
    /// Interaction logic for SystemInfos.xaml
    /// </summary>
    [TabColor("White")]
    [Localization("CrypTool.CrypWin.Properties.Resources")]
    public partial class LicensesTab : UserControl
    {
        public LicensesTab()
        {
            InitializeComponent();
            Tag = FindResource("Icon");
            LicenseTextbox.Text += (Properties.Resources.CrypToolLicenses + ":\r\n \r\n");
            LicenseTextbox.Text += Properties.Resources.ApacheLicense2;
        }
    }
}
