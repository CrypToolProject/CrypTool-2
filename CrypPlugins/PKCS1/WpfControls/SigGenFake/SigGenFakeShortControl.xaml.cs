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
using PKCS1.WpfControls.Components;
using System.Windows;
using System.Windows.Controls;

namespace PKCS1.WpfControls.SigGenFake
{
    /// <summary>
    /// Interaktionslogik für SigGenFakeShort.xaml
    /// </summary>
    public partial class SigGenFakeShortControl : UserControl, IPkcs1UserControl
    {
        private bool isKeyGen = false;
        private bool isDatablockGen = false;

        public bool GenSigAvailable
        {
            get => (bool)GetValue(GenSigAvailableProperty);
            set => SetValue(GenSigAvailableProperty, value);
        }

        public static readonly DependencyProperty GenSigAvailableProperty = DependencyProperty.Register(
          nameof(GenSigAvailable), typeof(bool), typeof(SigGenFakeShortControl), new PropertyMetadata(false));

        public SigGenFakeShortControl()
        {
            InitializeComponent();
            RsaKey.Instance.RaiseKeyGeneratedEvent += handleKeyGenerated;

            tabGenDatablock.OnTabContentChanged += content =>
            {
                DatablockControl datablockcontrol3 = ((DatablockControl)((ScrollViewer)content).Content);
                datablockcontrol3.RaiseDataBlockGenerated += handleKeyGenerated;
            };
        }

        private void handleKeyGenerated(ParameterChangeType type)
        {
            if (type == ParameterChangeType.RsaKey)
            {
                isKeyGen = true;
            }
            else if (type == ParameterChangeType.DataBlock)
            {
                isDatablockGen = true;
            }

            if (isKeyGen == true && isDatablockGen == true)
            {
                GenSigAvailable = true;
            }
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

        private void TabItem_HelpButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender == tabGenDatablock)
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(PKCS1.OnlineHelp.OnlineHelpActions.Gen_Datablock_Tab);
            }
            else if (sender == tabGenSignature)
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(PKCS1.OnlineHelp.OnlineHelpActions.Gen_Kuehn_Sig_Tab);
            }
        }
    }
}
