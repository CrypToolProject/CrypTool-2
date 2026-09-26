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
using PKCS1.Library;
using System.Windows;
using System.Windows.Controls;

namespace PKCS1.WpfVisualization.Navigation
{
    /// <summary>
    /// Interaktionslogik für Navigation.xaml
    /// </summary>
    public partial class Navigation : UserControl
    {
        public event Navigate OnNavigate;
        public Navigation()
        {
            InitializeComponent();
        }

        private void link_Click(object sender, RoutedEventArgs e)
        {
            if (null != OnNavigate)
            {
                NavigationCommandType commandtype = NavigationCommandType.None;

                if (sender == link_SignatureGenerate)
                {
                    commandtype = NavigationCommandType.SigGen;
                }
                else if (sender == link_RsaKeyGenerate)
                {
                    commandtype = NavigationCommandType.RsaKeyGen;
                }
                else if (sender == link_AttackBleichenbacher)
                {
                    commandtype = NavigationCommandType.SigGenFakeBleichenb;
                }
                else if (sender == link_AttackShortKeysVariant)
                {
                    commandtype = NavigationCommandType.SigGenFakeShort;
                }
                else if (sender == link_SignatureValidate)
                {
                    commandtype = NavigationCommandType.SigVal;
                }
                else if (sender == link_Start)
                {
                    commandtype = NavigationCommandType.Start;
                }

                OnNavigate(commandtype);
            }
        }
    }
}
