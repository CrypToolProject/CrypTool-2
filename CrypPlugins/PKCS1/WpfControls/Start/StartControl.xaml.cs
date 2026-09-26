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
using PKCS1.OnlineHelp;
using System.Windows.Controls;
//using PKCS1.WpfControls;

namespace PKCS1.WpfControls.Start
{
    /// <summary>
    /// Interaktionslogik für StartControl.xaml
    /// </summary>
    public partial class StartControl : UserControl, IPkcs1UserControl
    {
        private readonly System.Windows.Forms.WebBrowser b;

        public StartControl()
        {
            InitializeComponent();
            b = new System.Windows.Forms.WebBrowser
            {
                Dock = System.Windows.Forms.DockStyle.Fill
            };
            windowsFormsHost1.Child = b;
            b.DocumentText = OnlineHelpAccess.HelpResourceManager.GetString("Start");
        }

        #region IPkcs1UserControl Member

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public void Init()
        {
            //throw new NotImplementedException();
        }

        public void SetTab(int i)
        {
            //throw new NotImplementedException();
        }

        #endregion
    }
}
