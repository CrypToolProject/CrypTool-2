/*
   Copyright 2008 Timo Eckhardt, University of Siegen

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

using CrypTool.PluginBase;
using CrypTool.PluginBase.Attributes;
using Primes.Library;
using Primes.Options;
using Primes.WpfControls;
using Primes.WpfControls.Factorization;
using Primes.WpfControls.NumberTheory;
using Primes.WpfControls.NumberTheory.PrimitivRoots;
using Primes.WpfControls.Primegeneration;
using Primes.WpfControls.Primegeneration.SieveOfAtkin;
using Primes.WpfControls.PrimesDistribution;
using Primes.WpfControls.PrimesDistribution.Graph;
using Primes.WpfControls.PrimesDistribution.Numberline;
using Primes.WpfControls.PrimesDistribution.Spirals;
using Primes.WpfControls.Primetest;
using Primes.WpfControls.Start;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Primes.WpfVisualization
{
    /// <summary>
    /// Interaction logic for PrimesControl.xaml
    /// </summary>
    [TabColor("green")]
    public partial class PrimesControl : UserControl
    {
        private IPrimeMethodDivision m_ActualControl = null;
        private IPrimeMethodDivision m_FactorizationControl = null;
        private IPrimeMethodDivision m_GraphControl = null;
        private IPrimeMethodDivision m_StartControl = null;
        private IPrimeMethodDivision m_PrimetestControl = null;
        private IPrimeMethodDivision m_PrimespiralControl = null;
        private IPrimeMethodDivision m_PrimesgenerationControl = null;
        private IPrimeMethodDivision m_PrimesInNaturalNumbersControl = null;
        private IPrimeMethodDivision m_NumberTheoryControl = null;
        private IPrimeMethodDivision m_SieveOfAtkinControl = null;
        private bool m_Standalone = false;

        public bool Standalone
        {
            get => m_Standalone;
            set
            {
                if (m_HistoryPointer > 0)
                {
                    m_HistoryPointer--;
                }

                m_Standalone = value;

                if (Standalone)
                {
                    btnClose.Visibility = Visibility.Visible;
                    miClose.Visibility = Visibility.Visible;
                }
                else
                {
                    btnClose.Visibility = Visibility.Hidden;
                    miClose.Visibility = Visibility.Hidden;
                }
            }
        }

        private List<NavigationCommandType> m_History;
        private int m_HistoryPointer = -1;
        public event VoidDelegate OnClose;

        public PrimesControl()
        {
            InitializeComponent();
            Initialize();
        }

        private void Navigate(NavigationCommandType type)
        {
            Navigate(type, true);
        }

        private void Navigate(NavigationCommandType type, bool incHistory)
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
                case NavigationCommandType.Factor_Bf:
                    if (m_FactorizationControl == null)
                    {
                        m_FactorizationControl = new FactorizationControl();
                    }

                    SetUserControl(m_FactorizationControl, 0);
                    break;
                case NavigationCommandType.Factor_QS:
                    if (m_FactorizationControl == null)
                    {
                        m_FactorizationControl = new FactorizationControl();
                    }

                    SetUserControl(m_FactorizationControl, 2);
                    break;
                case NavigationCommandType.Primetest_Miller:
                    if (m_PrimetestControl == null)
                    {
                        m_PrimetestControl = new PrimetestControl(NavigateHistory);
                    }

                    SetUserControl(m_PrimetestControl, 2);
                    break;
                case NavigationCommandType.Primetest_Sieve:
                    if (m_PrimetestControl == null)
                    {
                        m_PrimetestControl = new PrimetestControl(NavigateHistory);
                    }

                    SetUserControl(m_PrimetestControl, 0);
                    break;
                case NavigationCommandType.SieveOfAtkin:
                    if (m_PrimetestControl == null)
                    {
                        m_PrimetestControl = new PrimetestControl(NavigateHistory);
                    }

                    SetUserControl(m_PrimetestControl, 3);
                    break;
                case NavigationCommandType.PrimeDistrib_Numberline:
                    if (m_PrimesInNaturalNumbersControl == null)
                    {
                        m_PrimesInNaturalNumbersControl = new PrimesInNaturalNumbersControl();
                    }

                    SetUserControl(m_PrimesInNaturalNumbersControl, 0);
                    break;
                case NavigationCommandType.PrimeDistrib_Numberrec:
                    if (m_PrimesInNaturalNumbersControl == null)
                    {
                        m_PrimesInNaturalNumbersControl = new PrimesInNaturalNumbersControl();
                    }

                    SetUserControl(m_PrimesInNaturalNumbersControl, 1);
                    break;
                case NavigationCommandType.Graph:
                    if (m_GraphControl == null)
                    {
                        m_GraphControl = new GraphControl();
                    }

                    if (m_PrimesInNaturalNumbersControl == null)
                    {
                        m_PrimesInNaturalNumbersControl = new PrimesInNaturalNumbersControl();
                    }

                    SetUserControl(m_PrimesInNaturalNumbersControl, 2);
                    break;
                case NavigationCommandType.PrimeDistrib_Ulam:
                    if (m_PrimesInNaturalNumbersControl == null)
                    {
                        m_PrimesInNaturalNumbersControl = new PrimesInNaturalNumbersControl();
                    }

                    SetUserControl(m_PrimesInNaturalNumbersControl, 3);
                    break;
                case NavigationCommandType.Numberline:
                    if (m_PrimesInNaturalNumbersControl == null)
                    {
                        m_PrimesInNaturalNumbersControl = new PrimesInNaturalNumbersControl();
                    }

                    SetUserControl(m_PrimesInNaturalNumbersControl);
                    break;
                case NavigationCommandType.Primespirals:
                    if (m_PrimespiralControl == null)
                    {
                        m_PrimespiralControl = new PrimesprialControl();
                    }

                    SetUserControl(m_PrimespiralControl);
                    break;
                case NavigationCommandType.Primesgeneration:
                    if (m_PrimesgenerationControl == null)
                    {
                        m_PrimesgenerationControl = new PrimesgenerationControl();
                    }

                    SetUserControl(m_PrimesgenerationControl);
                    break;
                case NavigationCommandType.PowerMod:
                    if (m_NumberTheoryControl == null)
                    {
                        m_NumberTheoryControl = new NumberTheoryControl();
                    }

                    SetUserControl(m_NumberTheoryControl, 0);
                    break;
                case NavigationCommandType.PowerBaseMod:
                    if (m_NumberTheoryControl == null)
                    {
                        m_NumberTheoryControl = new NumberTheoryControl();
                    }

                    SetUserControl(m_NumberTheoryControl, 1);
                    break;
                case NavigationCommandType.NumberTheoryFunctions:
                    if (m_NumberTheoryControl == null)
                    {
                        m_NumberTheoryControl = new NumberTheoryControl();
                    }

                    SetUserControl(m_NumberTheoryControl, 2);
                    break;
                case NavigationCommandType.PrimitivRoot:
                    if (m_NumberTheoryControl == null)
                    {
                        m_NumberTheoryControl = new NumberTheoryControl();
                    }

                    SetUserControl(m_NumberTheoryControl, 3);
                    break;
                case NavigationCommandType.PrimeDistrib_Goldbach:
                    if (m_NumberTheoryControl == null)
                    {
                        m_NumberTheoryControl = new NumberTheoryControl();
                    }

                    SetUserControl(m_NumberTheoryControl, 4);
                    break;
            }

            if (incHistory)
            {
                NavigateHistory(type);
            }

            SetHistoryButtons();
        }

        private void NavigateHistory(NavigationCommandType type)
        {
            m_HistoryPointer++;
            m_History.RemoveRange(m_HistoryPointer, m_History.Count - m_HistoryPointer);
            m_History.Add(type);
        }

        private void SetTitle(NavigationCommandType type)
        {
            imghelp.Visibility = Visibility.Visible;

            switch (type)
            {
                case NavigationCommandType.Factor_QS:
                case NavigationCommandType.Factor_Bf:
                    lblTitel.Text = Primes.Resources.lang.PrimesControl.PrimesControl.title_factorize;
                    break;
                case NavigationCommandType.Start:
                    lblTitel.Text = Primes.Resources.lang.PrimesControl.PrimesControl.title_start;
                    //imghelp.Visibility = Visibility.Hidden;
                    break;
                case NavigationCommandType.Primetest_Miller:
                case NavigationCommandType.Primetest_Sieve:
                case NavigationCommandType.SieveOfAtkin:
                    lblTitel.Text = Primes.Resources.lang.PrimesControl.PrimesControl.title_primetest;
                    break;
                case NavigationCommandType.Primespirals:
                    lblTitel.Text = Primes.Resources.lang.PrimesControl.PrimesControl.title_primespiral;
                    break;
                case NavigationCommandType.Primesgeneration:
                    lblTitel.Text = Primes.Resources.lang.PrimesControl.PrimesControl.title_primesgeneration;
                    break;
                case NavigationCommandType.PrimeDistrib_Numberline:
                case NavigationCommandType.PrimeDistrib_Numberrec:
                case NavigationCommandType.PrimeDistrib_Ulam:
                case NavigationCommandType.Graph:
                    lblTitel.Text = Primes.Resources.lang.PrimesControl.PrimesControl.title_distribution;
                    break;
                case NavigationCommandType.NumberTheoryFunctions:
                case NavigationCommandType.PrimitivRoot:
                case NavigationCommandType.PowerMod:
                case NavigationCommandType.PowerBaseMod:
                case NavigationCommandType.PrimeDistrib_Goldbach:
                    lblTitel.Text = Primes.Resources.lang.PrimesControl.PrimesControl.title_Numbertheory;
                    break;
            }
        }

        private void SetUserControl(IPrimeMethodDivision control)
        {
            SetUserControl(control, -1);
        }

        private void SetUserControl(IPrimeMethodDivision control, int tab)
        {
            //(control as UserControl).Width = ContentArea.ActualWidth;
            //(control as UserControl).Height = ContentArea.ActualHeight;
            if (tab >= 0)
            {
                try
                {
                    control.SetTab(tab);
                }
                catch { }
            }
            (control as UserControl).HorizontalAlignment = HorizontalAlignment.Stretch;
            (control as UserControl).VerticalAlignment = VerticalAlignment.Stretch;
            ContentArea.Content = control as UserControl;
            //ContentArea.Children.Add(control as UserControl);
            m_ActualControl = control;
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            if (m_ActualControl != null)
            {
                //(m_ActualControl as UserControl).Height = ContentArea.ActualHeight;
                //(m_ActualControl as UserControl).Width = ContentArea.ActualWidth;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //StartControl.Width = ContentArea.ActualWidth;
            //StartControl.Height = ContentArea.ActualHeight;
        }

        public void CleanUp()
        {
            if (m_FactorizationControl != null)
            {
                m_FactorizationControl.Dispose();
            }

            if (m_GraphControl != null)
            {
                m_GraphControl.Dispose();
            }

            if (m_PrimetestControl != null)
            {
                m_PrimetestControl.Dispose();
            }

            if (m_PrimespiralControl != null)
            {
                m_PrimespiralControl.Dispose();
            }

            if (m_PrimesgenerationControl != null)
            {
                m_PrimesgenerationControl.Dispose();
            }

            if (m_PrimesInNaturalNumbersControl != null)
            {
                m_PrimesInNaturalNumbersControl.Dispose();
            }

            if (m_NumberTheoryControl != null)
            {
                m_NumberTheoryControl.Dispose();
            }

            if (m_SieveOfAtkinControl != null)
            {
                m_SieveOfAtkinControl.Dispose();
            }

            m_FactorizationControl = null;
            m_GraphControl = null;
            m_StartControl = null;
            m_PrimetestControl = null;
            m_PrimespiralControl = null;
            m_PrimesgenerationControl = null;
            m_PrimesInNaturalNumbersControl = null;
            m_NumberTheoryControl = null;
            m_SieveOfAtkinControl = null;

            OnlineHelp.OnlineHelpAccess.HelpWindowClosed();
        }

        private void ImageHelpClick(object sender, MouseButtonEventArgs e)
        {
            //Primes.OnlineHelp.OnlineHelpActions action = Primes.OnlineHelp.OnlineHelpActions.Graph_PrimesCount;
            Primes.OnlineHelp.OnlineHelpActions action = Primes.OnlineHelp.OnlineHelpActions.StartControl;

            if (m_ActualControl.GetType() == typeof(GraphControl))
            {
                action = Primes.OnlineHelp.OnlineHelpActions.Graph_PrimesCount;
            }
            else if (m_ActualControl.GetType() == typeof(FactorizationControl))
            {
                action = Primes.OnlineHelp.OnlineHelpActions.Factorization_Factorization;
            }
            else if (m_ActualControl.GetType() == typeof(PrimetestControl))
            {
                action = Primes.OnlineHelp.OnlineHelpActions.Primetest_Primetest;
            }
            else if (m_ActualControl.GetType() == typeof(PrimesgenerationControl))
            {
                action = Primes.OnlineHelp.OnlineHelpActions.Generation_Generation;
            }
            else if (m_ActualControl.GetType() == typeof(NumberlineControl))
            {
                action = Primes.OnlineHelp.OnlineHelpActions.Distribution_Numberline;
            }
            else if (m_ActualControl.GetType() == typeof(PrimesprialControl))
            {
                action = Primes.OnlineHelp.OnlineHelpActions.Spiral_Ulam;
            }
            else if (m_ActualControl.GetType() == typeof(PrimitivRootControl))
            {
                action = Primes.OnlineHelp.OnlineHelpActions.PrimitivRoot_PrimitivRoot;
            }
            else if (m_ActualControl.GetType() == typeof(PrimesInNaturalNumbersControl))
            {
                action = Primes.OnlineHelp.OnlineHelpActions.Distribution_Distribution;
            }
            else if (m_ActualControl.GetType() == typeof(SieveOfAtkinControl))
            {
                action = Primes.OnlineHelp.OnlineHelpActions.Generation_SieveOfAtkin;
            }
            else if (m_ActualControl.GetType() == typeof(NumberTheoryControl))
            {
                action = Primes.OnlineHelp.OnlineHelpActions.Numbertheory_Numbertheory;
            }

            e.Handled = true;
            OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(action);
        }

        private void miOptions_Click(object sender, RoutedEventArgs e)
        {
            OptionsAccess.ShowOptionsDialog();
        }

        private void miClose_Click(object sender, RoutedEventArgs e)
        {
            if (OnClose != null)
            {
                OptionsAccess.ForceClose();
                OnClose();
            }
        }

        /*
        private void miSwitch_Click(object sender, RoutedEventArgs e)
        {
          if (pnlLeft.Visibility == Visibility.Visible)
          {
            miSwitch.Header = Primes.Resources.lang.PrimesControl.PrimesControl.miswitch_show;
            pnlLeft.Visibility = Visibility.Collapsed;
            cdleft.MinWidth = 0;
          }
          else
          {
            miSwitch.Header = Primes.Resources.lang.PrimesControl.PrimesControl.miswitch_hide;
            pnlLeft.Visibility = Visibility.Visible;
            cdleft.MinWidth = 250;
          }
          //if (!m_Collapsed)
          //{
          //  cdleft.MinWidth = 0;
          //  pnlLeft.Width = 0;
          //}
          //else
          //{
          //  cdleft.MinWidth = 250;
          //  pnlLeft.Width = 250;
          //}
          //m_Collapsed = !m_Collapsed;
        }
         */

        #region IPlugin Members

        public void Dispose()
        {
            CleanUp();
        }

        public void Initialize()
        {
            ControlHandler.Dispatcher = Dispatcher;
            navigator.OnNavigate += Navigate;

            PrimesCountList.LoadPrimes();
            m_History = new List<NavigationCommandType>();

            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                Navigate(NavigationCommandType.Start);
            }, null);
            (m_StartControl as StartControl).OnStartpageLinkClick += new Navigate(Navigate);
            MouseRightButtonDown += new MouseButtonEventHandler(PrimesControl_MouseRightButtonDown);
        }

        private void PrimesControl_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Parent != null)
            {
                if ((Parent as Control).ContextMenu != null)
                {
                    (Parent as Control).ContextMenu.Visibility = Visibility.Hidden;
                }
            }
        }

        // public event StatusBarProgressbarValueChangedHandler OnStatusBarProgressbarValueChanged;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private void FireOnStatusBarProgressbarValueChanged(double val)
        {
            //if (OnPluginProgressChanged != null) OnPluginProgressChanged(this, new PluginProgressEventArgs(val, 17));
        }

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        private void FireOnGuiLogNotificationOccured()
        {
            //if (OnGuiLogNotificationOccured != null) OnGuiLogNotificationOccured(this,new GuiLogEventArgs("",this,NotificationLevel.Info));
        }

        public UserControl Presentation => this;

        #endregion

        #region IPlugin Members

        public void Stop()
        {
        }

        #endregion

        private void miHistory_Click(object sender, RoutedEventArgs e)
        {
            if (sender == miBack)
            {
                if (m_HistoryPointer > 0)
                {
                    m_HistoryPointer--;
                }
            }
            else
            {
                if (m_HistoryPointer < m_History.Count - 1)
                {
                    m_HistoryPointer++;
                }
            }

            Navigate(m_History[m_HistoryPointer], false);
        }

        private void SetHistoryButtons()
        {
            miBack.IsEnabled = (m_HistoryPointer > 0);
            miForward.IsEnabled = (m_HistoryPointer < m_History.Count - 1);
        }
    }
}
