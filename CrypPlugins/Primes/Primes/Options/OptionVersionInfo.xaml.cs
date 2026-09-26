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
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Primes.Options
{
    /// <summary>
    /// Interaction logic for OptionVersionInfo.xaml
    /// </summary>
    public partial class OptionVersionInfo : UserControl
    {
        public OptionVersionInfo()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Version version = AssemblyHelper.GetVersion(Assembly.GetAssembly(GetType()));
            string strVersion = string.Format("{0}.{1}.{2}", new object[] { version.Major - 1, version.Minor, version.Revision });
            tbVersionInfo.Text = strVersion;
            tbBuildInfo.Text = version.Build.ToString();
        }
    }
}