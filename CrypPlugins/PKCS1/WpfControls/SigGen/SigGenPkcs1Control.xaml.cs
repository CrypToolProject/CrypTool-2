using System.Windows;
using System.Windows.Controls;
using PKCS1.Library;
using PKCS1.WpfControls.Components;

namespace PKCS1.WpfControls.SigGen
{
    /// <summary>
    /// Interaktionslogik für SigGenPkcs1.xaml
    /// </summary>
    public partial class SigGenPkcs1Control : UserControl, IPkcs1UserControl
    {
        private bool isKeyGen = false;
        private bool isDatablockGen = false;

        public SigGenPkcs1Control()
        {
            InitializeComponent();
            RsaKey.Instance.RaiseKeyGeneratedEvent += handleKeyGenerated;

            tabGenDatablock.OnTabContentChanged += content =>
            {
                var datablockcontrol = ((DatablockControl)((ScrollViewer)content).Content);
                datablockcontrol.RaiseDataBlockGenerated += handleKeyGenerated;
            };

            if (RsaKey.Instance.isKeyGenerated())
            {
                this.tabGenSignature.IsEnabled = true;
            }
            else
            {
                this.tabGenSignature.IsEnabled = false;
            }
        }

        private void handleKeyGenerated(ParameterChangeType type)
        {
            if (type == ParameterChangeType.RsaKey)
            {
                this.isKeyGen = true;
            }
            else if (type == ParameterChangeType.DataBlock)
            {
                this.isDatablockGen = true;
            }

            if (this.isKeyGen == true && this.isDatablockGen == true)
            {
                this.tabGenSignature.IsEnabled = true;
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
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(PKCS1.OnlineHelp.OnlineHelpActions.Gen_PKCS1_Sig_Tab);
            }
        }
    }
}
