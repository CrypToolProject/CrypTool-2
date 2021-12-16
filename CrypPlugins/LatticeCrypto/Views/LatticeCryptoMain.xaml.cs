using LatticeCrypto.Properties;
using LatticeCrypto.Utilities;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LatticeCrypto.Views
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class LatticeCryptoMain
    {
        private ILatticeCryptoUserControl m_ActualControl;
        private ILatticeCryptoUserControl m_StartControl;
        private ILatticeCryptoUserControl m_GaussControl;
        private ILatticeCryptoUserControl m_LLLControl;
        private ILatticeCryptoUserControl m_CVPControl;
        private ILatticeCryptoUserControl m_MerkleHellmanControl;
        private ILatticeCryptoUserControl m_RSAControl;
        private ILatticeCryptoUserControl m_GGHControl;
        private ILatticeCryptoUserControl m_LWEControl;

        public LatticeCryptoMain()
        {
            InitializeComponent();
            Initialize();

            //Screenshot Function
            //KeyDown += delegate (object sender, KeyEventArgs args)
            //               {
            //                   if (args.Key != Key.F5)
            //                       return;
            //                   Util.CreateSaveBitmap(mainGrid);
            //               };
        }

        private void Initialize()
        {
            navigator.OnNavigate += Navigate;

            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                Navigate(NavigationCommandType.Start);
            }, null);
        }

        private void Navigate(NavigationCommandType type)
        {
            if (m_ActualControl != null)
            {
                m_ActualControl.Dispose();
            }
            SetTitle(type);

            switch (type)
            {
                case NavigationCommandType.Start:
                    if (m_StartControl == null)
                    {
                        m_StartControl = new StartControl();
                    }

                    SetUserControl(m_StartControl);
                    break;
                case NavigationCommandType.Gauss:
                    if (m_GaussControl == null)
                    {
                        m_GaussControl = new SvpGaussView();
                    }

                    SetUserControl(m_GaussControl);
                    break;
                case NavigationCommandType.LLL:
                    if (m_LLLControl == null)
                    {
                        m_LLLControl = new SvpLLLView();
                    }

                    SetUserControl(m_LLLControl);
                    break;
                case NavigationCommandType.CVP:
                    if (m_CVPControl == null)
                    {
                        m_CVPControl = new CvpView();
                    }

                    SetUserControl(m_CVPControl);
                    break;
                case NavigationCommandType.MerkleHellman:
                    if (m_MerkleHellmanControl == null)
                    {
                        m_MerkleHellmanControl = new MerkleHellmanView();
                    }

                    SetUserControl(m_MerkleHellmanControl);
                    break;
                case NavigationCommandType.RSA:
                    if (m_RSAControl == null)
                    {
                        m_RSAControl = new RSAView();
                    }

                    SetUserControl(m_RSAControl);
                    break;
                case NavigationCommandType.GGH:
                    if (m_GGHControl == null)
                    {
                        m_GGHControl = new GGHView();
                    }

                    SetUserControl(m_GGHControl);
                    break;
                case NavigationCommandType.LWE:
                    if (m_LWEControl == null)
                    {
                        m_LWEControl = new LWEView();
                    }

                    SetUserControl(m_LWEControl);
                    break;
            }

        }

        private void SetTitle(NavigationCommandType type)
        {
            switch (type)
            {
                case NavigationCommandType.Start:
                    lblTitel.Text = Languages.LatticeSettings;
                    break;
                case NavigationCommandType.Gauss:
                    lblTitel.Text = Languages.tabGaussAlgorithm;
                    break;
                case NavigationCommandType.LLL:
                    lblTitel.Text = Languages.tabLLLAlgorithm;
                    break;
                case NavigationCommandType.CVP:
                    lblTitel.Text = Languages.tabFindCVP;
                    break;
                case NavigationCommandType.MerkleHellman:
                    lblTitel.Text = Languages.tabAttackMerkleHellman;
                    break;
                case NavigationCommandType.RSA:
                    lblTitel.Text = Languages.tabAttackRSA;
                    break;
                case NavigationCommandType.GGH:
                    lblTitel.Text = Languages.tabGGH;
                    break;
                case NavigationCommandType.LWE:
                    lblTitel.Text = Languages.tabLWE;
                    break;
            }
        }

        private void SetUserControl(ILatticeCryptoUserControl control)
        {
            ((UserControl)control).HorizontalAlignment = HorizontalAlignment.Stretch;
            ((UserControl)control).VerticalAlignment = VerticalAlignment.Stretch;
            ContentArea.Content = control as UserControl;
            m_ActualControl = control;
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OnlineHelp.OnlineHelpActions action = OnlineHelp.OnlineHelpActions.StartControl;

            if (m_ActualControl.GetType() == typeof(SvpGaussView))
            {
                action = OnlineHelp.OnlineHelpActions.Gauss;
            }
            else if (m_ActualControl.GetType() == typeof(SvpLLLView))
            {
                action = OnlineHelp.OnlineHelpActions.LLL;
            }
            else if (m_ActualControl.GetType() == typeof(CvpView))
            {
                action = OnlineHelp.OnlineHelpActions.CVP;
            }
            else if (m_ActualControl.GetType() == typeof(MerkleHellmanView))
            {
                action = OnlineHelp.OnlineHelpActions.MerkleHellman;
            }
            else if (m_ActualControl.GetType() == typeof(RSAView))
            {
                action = OnlineHelp.OnlineHelpActions.RSA;
            }
            else if (m_ActualControl.GetType() == typeof(GGHView))
            {
                action = OnlineHelp.OnlineHelpActions.GGH;
            }
            else if (m_ActualControl.GetType() == typeof(LWEView))
            {
                action = OnlineHelp.OnlineHelpActions.LWE;
            }

            e.Handled = true;
            OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(action);
        }
    }
}
