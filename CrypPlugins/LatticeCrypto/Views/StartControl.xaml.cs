using LatticeCrypto.OnlineHelp;
using LatticeCrypto.Utilities;


namespace LatticeCrypto.Views
{
    /// <summary>
    /// Interaktionslogik für StartControl.xaml
    /// </summary>
    public partial class StartControl : ILatticeCryptoUserControl
    {
        private readonly System.Windows.Forms.WebBrowser b;

        public StartControl()
        {
            InitializeComponent();
            b = new System.Windows.Forms.WebBrowser { Dock = System.Windows.Forms.DockStyle.Fill };
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
