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
using System.Windows;
using System.Windows.Controls;

namespace PKCS1.WpfControls.RsaKeyGen
{
    /// <summary>
    /// Interaktionslogik für RsaKeyGenControl.xaml
    /// </summary>
    public partial class RsaKeyGenControl : UserControl, IPkcs1UserControl
    {
        public RsaKeyGenControl()
        {
            InitializeComponent();
        }

        private void TabItem_HelpButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender == tabGenKey)
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(PKCS1.OnlineHelp.OnlineHelpActions.KeyGen_Tab);
            }
            else if (sender == tabInputKey)
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(PKCS1.OnlineHelp.OnlineHelpActions.KeyInput_Tab);
            }
        }

        #region IPkcs1UserControl Member

        void IPkcs1UserControl.Dispose()
        {
            //throw new NotImplementedException();
        }

        void IPkcs1UserControl.Init()
        {
            //throw new NotImplementedException();
        }

        void IPkcs1UserControl.SetTab(int i)
        {
            //throw new NotImplementedException();
        }

        #endregion
    }
}
