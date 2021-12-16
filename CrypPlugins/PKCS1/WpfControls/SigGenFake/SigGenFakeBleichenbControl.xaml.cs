using PKCS1.Library;
using PKCS1.WpfControls.Components;
using System.Windows;
using System.Windows.Controls;

namespace PKCS1.WpfControls.SigGenFake
{
    /// <summary>
    /// Interaktionslogik für SigGenFakeBleichenbacher.xaml
    /// </summary>
    public partial class SigGenFakeBleichenbControl : UserControl, IPkcs1UserControl
    {
        private bool isKeyGen = false;
        private bool isDatablockGen = false;

        public bool GenSigAvailable
        {
            get => (bool)GetValue(GenSigAvailableProperty);
            set => SetValue(GenSigAvailableProperty, value);
        }

        public static readonly DependencyProperty GenSigAvailableProperty = DependencyProperty.Register(
          nameof(GenSigAvailable), typeof(bool), typeof(SigGenFakeBleichenbControl), new PropertyMetadata(false));

        public SigGenFakeBleichenbControl()
        {
            InitializeComponent();
            RsaKey.Instance.RaiseKeyGeneratedEvent += handleKeyGenerated;
            tabGenDatablock.OnTabContentChanged += content =>
            {
                DatablockControl datablockcontrol2 = ((DatablockControl)((ScrollViewer)content).Content);
                datablockcontrol2.RaiseDataBlockGenerated += handleKeyGenerated;
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
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(PKCS1.OnlineHelp.OnlineHelpActions.Gen_Bleichenb_Sig_Tab);
            }
        }
    }
}
