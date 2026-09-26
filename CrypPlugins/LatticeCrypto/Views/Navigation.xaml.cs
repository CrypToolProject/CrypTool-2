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
using LatticeCrypto.Utilities;
using System.Windows;

namespace LatticeCrypto.Views
{
    /// <summary>
    /// Interaktionslogik für Navigation.xaml
    /// </summary>
    public partial class Navigation
    {
        public event Navigate OnNavigate;
        public Navigation()
        {
            InitializeComponent();
        }

        private void link_Click(object sender, RoutedEventArgs e)
        {
            if (null == OnNavigate)
            {
                return;
            }

            NavigationCommandType commandtype = NavigationCommandType.None;

            if (Equals(sender, link_Start))
            {
                commandtype = NavigationCommandType.Start;
            }
            else if (Equals(sender, link_Gauss))
            {
                commandtype = NavigationCommandType.Gauss;
            }
            else if (Equals(sender, link_LLL))
            {
                commandtype = NavigationCommandType.LLL;
            }
            else if (Equals(sender, link_CVP))
            {
                commandtype = NavigationCommandType.CVP;
            }
            else if (Equals(sender, link_MerkleHellman))
            {
                commandtype = NavigationCommandType.MerkleHellman;
            }
            else if (Equals(sender, link_RSA))
            {
                commandtype = NavigationCommandType.RSA;
            }
            else if (Equals(sender, link_GGH))
            {
                commandtype = NavigationCommandType.GGH;
            }
            else if (Equals(sender, link_LWE))
            {
                commandtype = NavigationCommandType.LWE;
            }

            OnNavigate(commandtype);
        }
    }
}
