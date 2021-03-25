using System.Windows;
using System.Windows.Controls;
using PKCS1.Library;
using PKCS1.WpfControls.Components;

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
            get { return (bool)this.GetValue(GenSigAvailableProperty); }
            set { this.SetValue(GenSigAvailableProperty, value); }
        }

        public static readonly DependencyProperty GenSigAvailableProperty = DependencyProperty.Register(
          nameof(GenSigAvailable), typeof(bool), typeof(SigGenFakeShortControl), new PropertyMetadata(false));

        public SigGenFakeShortControl()
        {
            InitializeComponent();
            RsaKey.Instance.RaiseKeyGeneratedEvent += handleKeyGenerated;

            tabGenDatablock.OnTabContentChanged += content =>
            {
                var datablockcontrol3 = ((DatablockControl)((ScrollViewer)content).Content);
                datablockcontrol3.RaiseDataBlockGenerated += handleKeyGenerated;
            };
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
