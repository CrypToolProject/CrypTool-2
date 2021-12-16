using CrypTool.PluginBase.Attributes;
using PKCS1.Library;
using PKCS1.Resources.lang.Gui;
using PKCS1.WpfControls;
using PKCS1.WpfControls.RsaKeyGen;
using PKCS1.WpfControls.SigGen;
using PKCS1.WpfControls.SigGenFake;
using PKCS1.WpfControls.SigVal;
using PKCS1.WpfControls.Start;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace PKCS1.WpfVisualization
{
    /// <summary>
    /// Interaktionslogik für pkcs1control.xaml
    /// </summary>
    [TabColor("black")]
    public partial class Pkcs1Control : UserControl
    {
        private IPkcs1UserControl m_ActualControl = null;
        private IPkcs1UserControl m_RsaKeyGenControl = null;
        private IPkcs1UserControl m_StartControl = null;
        private IPkcs1UserControl m_SigGenControl = null;
        private IPkcs1UserControl m_SigGenFakeBleichenbControl = null;
        private IPkcs1UserControl m_SigGenFakeShortControl = null;
        private IPkcs1UserControl m_SigValControl = null;

        public Pkcs1Control()
        {
            InitializeComponent();
            Initialize();
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
                case NavigationCommandType.RsaKeyGen:
                    if (m_RsaKeyGenControl == null)
                    {
                        m_RsaKeyGenControl = new RsaKeyGenControl();
                    }

                    SetUserControl(m_RsaKeyGenControl);
                    break;
                case NavigationCommandType.SigGen:
                    if (m_SigGenControl == null)
                    {
                        m_SigGenControl = new SigGenPkcs1Control();
                    }

                    SetUserControl(m_SigGenControl);
                    break;
                case NavigationCommandType.SigGenFakeBleichenb:
                    if (m_SigGenFakeBleichenbControl == null)
                    {
                        m_SigGenFakeBleichenbControl = new SigGenFakeBleichenbControl();
                    }

                    SetUserControl(m_SigGenFakeBleichenbControl);
                    break;
                case NavigationCommandType.SigGenFakeShort:
                    if (m_SigGenFakeShortControl == null)
                    {
                        m_SigGenFakeShortControl = new SigGenFakeShortControl();
                    }

                    SetUserControl(m_SigGenFakeShortControl);
                    break;
                case NavigationCommandType.SigVal:
                    if (m_SigValControl == null)
                    {
                        m_SigValControl = new SigValControl();
                    }

                    SetUserControl(m_SigValControl);
                    break;
                case NavigationCommandType.Start:
                    if (m_StartControl == null)
                    {
                        m_StartControl = new StartControl();
                    }

                    SetUserControl(m_StartControl);
                    break;
            }

        }

        private void SetTitle(NavigationCommandType type)
        {
            switch (type)
            {
                case NavigationCommandType.RsaKeyGen:
                    lblTitel.Text = RsaKeyGenCtrl.title;
                    break;
                case NavigationCommandType.Start:
                    lblTitel.Text = Common.startTitle;
                    break;
                case NavigationCommandType.SigGen:
                    lblTitel.Text = SigGenRsaCtrl.title;
                    break;
                case NavigationCommandType.SigGenFakeBleichenb:
                    lblTitel.Text = SigGenBleichenbCtrl.title;
                    break;
                case NavigationCommandType.SigGenFakeShort:
                    lblTitel.Text = SigGenKuehnCtrl.title;
                    break;
                case NavigationCommandType.SigVal:
                    lblTitel.Text = SigValCtrl.title;
                    break;
            }
        }

        private void SetUserControl(IPkcs1UserControl control)
        {
            SetUserControl(control, 0);
        }

        private void SetUserControl(IPkcs1UserControl control, int tab)
        {
            (control as UserControl).HorizontalAlignment = HorizontalAlignment.Stretch;
            (control as UserControl).VerticalAlignment = VerticalAlignment.Stretch;
            ContentArea.Content = control as UserControl;

            m_ActualControl = control;
        }


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //kein code
        }

        internal void Dispose()
        {
            if (m_RsaKeyGenControl != null)
            {
                m_RsaKeyGenControl.Dispose();
            }

            if (m_StartControl != null)
            {
                m_StartControl.Dispose();
            }

            if (m_SigGenControl != null)
            {
                m_SigGenControl.Dispose();
            }

            if (m_SigGenFakeBleichenbControl != null)
            {
                m_SigGenFakeBleichenbControl.Dispose();
            }

            if (m_SigGenFakeShortControl != null)
            {
                m_SigGenFakeShortControl.Dispose();
            }

            if (m_SigValControl != null)
            {
                m_SigValControl.Dispose();
            }

            m_RsaKeyGenControl = null;
            m_StartControl = null;
            m_SigGenControl = null;
            m_SigGenFakeBleichenbControl = null;
            m_SigGenFakeShortControl = null;
            m_SigValControl = null;
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PKCS1.OnlineHelp.OnlineHelpActions action = PKCS1.OnlineHelp.OnlineHelpActions.StartControl;

            if (m_ActualControl.GetType() == typeof(RsaKeyGenControl))
            {
                action = PKCS1.OnlineHelp.OnlineHelpActions.KeyGen;
            }
            else if (m_ActualControl.GetType() == typeof(SigGenPkcs1Control))
            {
                action = PKCS1.OnlineHelp.OnlineHelpActions.SigGen;
            }
            else if (m_ActualControl.GetType() == typeof(SigGenFakeBleichenbControl))
            {
                action = PKCS1.OnlineHelp.OnlineHelpActions.SigGenFakeBleichenbacher;
            }
            else if (m_ActualControl.GetType() == typeof(SigGenFakeShortControl))
            {
                action = PKCS1.OnlineHelp.OnlineHelpActions.SigGenFakeKuehn;
            }
            else if (m_ActualControl.GetType() == typeof(SigValControl))
            {
                action = PKCS1.OnlineHelp.OnlineHelpActions.SigVal;
            }

            e.Handled = true;
            OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(action);
        }
    }
}
