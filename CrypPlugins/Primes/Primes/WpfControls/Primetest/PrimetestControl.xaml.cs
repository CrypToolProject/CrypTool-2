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

using Primes.Bignum;
using Primes.Library;
using Primes.WpfControls.Components;
using Primes.WpfControls.Primegeneration.SieveOfAtkin;
using Primes.WpfControls.Primetest.MillerRabin;
using Primes.WpfControls.Primetest.TestOfFermat;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Primes.WpfControls.Primetest
{
    /// <summary>
    /// Interaction logic for PrimetestControl.xaml
    /// </summary>
    public partial class PrimetestControl : UserControl, IPrimeMethodDivision
    {
        private SieveOfEratosthenes.SieveOfEratosthenes sieveoferatosthenes;
        private TestOfFermatControl fermat;
        private MillerRabinControl millerrabin;
        private SieveOfAtkinControl soa;

        public PrimetestControl(Navigate nav = null)
        {
            InitializeComponent();

            tabItemSieveOfEratosthenes.OnTabContentChanged += content =>
            {
                iscNumber.UnLockControls();
                sieveoferatosthenes = (SieveOfEratosthenes.SieveOfEratosthenes)((ScrollViewer)content).Content;
                sieveoferatosthenes.Start += new VoidDelegate(sieveoferatosthenes_Execute);
                sieveoferatosthenes.Stop += new VoidDelegate(sieveoferatosthenes_Cancel);
                sieveoferatosthenes.ForceGetInteger += new CallbackDelegateGetInteger(sieveoferatosthenes_ForceGetInteger);
            };

            tabItemTestOfFermat.OnTabContentChanged += content =>
            {
                iscNumber.UnLockControls();
                fermat = (TestOfFermat.TestOfFermatControl)content;
                fermat.ForceGetInteger += new CallbackDelegateGetInteger(sieveoferatosthenes_ForceGetInteger);
                fermat.ExecuteTest += new VoidDelegate(fermat_Start);
                fermat.CancelTest += new VoidDelegate(fermat_Stop);
                //fermat.ForceGetValue += new Primes.Library.CallBackDelegate(PrimeTestForceGetValue);
            };

            tabItemMillerRabin.OnTabContentChanged += content =>
            {
                iscNumber.UnLockControls();
                millerrabin = (MillerRabin.MillerRabinControl)content;
                millerrabin.Start += new VoidDelegate(millerrabin_ExecuteTest);
                millerrabin.Stop += new VoidDelegate(millerrabin_CancelTest);
                millerrabin.ForceGetInteger += new CallbackDelegateGetInteger(sieveoferatosthenes_ForceGetInteger);
            };

            tabItemSoa.OnTabContentChanged += content =>
            {
                iscNumber.UnLockControls();
                soa = (Primegeneration.SieveOfAtkin.SieveOfAtkinControl)content;
                soa.Start += new VoidDelegate(millerrabin_ExecuteTest);
                soa.Stop += new VoidDelegate(millerrabin_CancelTest);
            };

            iscNumber.SetText(InputSingleControl.Free, "100");
            iscNumber.SetText(InputSingleControl.CalcFactor, "1");
            iscNumber.SetText(InputSingleControl.CalcBase, ((new Random().Next() % 5) + 1).ToString());
            iscNumber.SetText(InputSingleControl.CalcExp, ((new Random().Next() % 5) + 1).ToString());
            iscNumber.SetText(InputSingleControl.CalcSum, (new Random().Next() % 11).ToString());

            myNavigate = nav;
        }

        private void sieveoferatosthenes_ForceGetInteger(ExecuteIntegerDelegate ExecuteDelegate)
        {
            PrimesBigInteger value = iscNumber.GetValue();
            if (value != null && ExecuteDelegate != null)
            {
                ExecuteDelegate(value);
            }
        }

        private void generateNumberControl_OnRandomNumberGenerated(PrimesBigInteger value)
        {
            iscNumber.FreeText = value.ToString();
            iscNumber_Execute(value);
        }

        private void millerrabin_CancelTest()
        {
            iscNumber.UnLockControls();
        }

        private void millerrabin_ExecuteTest()
        {
            iscNumber.LockControls();
        }

        #region fermat

        private void fermat_Stop()
        {
            iscNumber.UnLockControls();
        }

        private void fermat_Start()
        {
            iscNumber.LockControls();
        }

        #endregion

        #region Erathostenes

        private void sieveoferatosthenes_Cancel()
        {
            iscNumber.UnLockControls();
        }

        private void sieveoferatosthenes_Execute()
        {
            iscNumber.LockControls();
        }

        #endregion

        #region IPrimeUserControl Members

        public void Dispose()
        {
            if (sieveoferatosthenes != null)
            {
                sieveoferatosthenes.CleanUp();
            }
            //if (fermat != null)
            //{
            //  fermat.CleanUp();
            //}
        }

        #endregion

        private void iscNumber_Execute(PrimesBigInteger value)
        {
            CurrentControl.Execute(value);
        }

        private void iscNumber_Cancel()
        {
            CurrentControl.CancelExecute();
            SetLocks();
        }

        private IPrimeTest CurrentControl
        {
            get
            {
                if (tbctrl.SelectedItem == tabItemSieveOfEratosthenes)
                {
                    return sieveoferatosthenes;
                }

                if (tbctrl.SelectedItem == tabItemTestOfFermat)
                {
                    return fermat;
                }

                if (tbctrl.SelectedItem == tabItemMillerRabin)
                {
                    return millerrabin;
                }

                if (tbctrl.SelectedItem == tabItemSoa)
                {
                    return soa;
                }

                return null;
            }
        }

        private NavigationCommandType CurrentNavigationCommand
        {
            get
            {
                if (tbctrl.SelectedItem == tabItemSieveOfEratosthenes)
                {
                    return NavigationCommandType.Primetest_Sieve;
                }

                if (tbctrl.SelectedItem == tabItemMillerRabin)
                {
                    return NavigationCommandType.Primetest_Miller;
                }

                if (tbctrl.SelectedItem == tabItemSoa)
                {
                    return NavigationCommandType.SieveOfAtkin;
                }

                return NavigationCommandType.Primetest_Sieve;
            }
        }

        public Navigate myNavigate = null;

        private void tbctrl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            iscNumber.SetValueValidator(InputSingleControl.Value, CurrentControl.Validator);
            if (myNavigate != null)
            {
                myNavigate(CurrentNavigationCommand);
            }

            iscNumber.GetValue();

            if (CurrentControl == sieveoferatosthenes)
            {
                iscNumber.generateNumberControlVertFree.MaxValue = sieveoferatosthenes.MaxValue;
            }
            else if (CurrentControl == fermat)
            {
                iscNumber.generateNumberControlVertFree.MaxValue = 1000;
            }
            else if (CurrentControl == millerrabin)
            {
                iscNumber.generateNumberControlVertFree.MaxValue = ((System.Numerics.BigInteger)1) << 100;
            }
            else if (CurrentControl == soa)
            {
                iscNumber.generateNumberControlVertFree.MaxValue = 1000;
            }

            SetLocks();
        }

        private void SetLocks()
        {
            PrimesBigInteger value = iscNumber.GetValue();
            if (value != null)
            {
                if (CurrentControl.IsRunning())
                {
                    iscNumber.LockControls();
                }
                else
                {
                    iscNumber.UnLockControls();
                }
            }
        }

        #region IPrimeUserControl Members

        public void SetTab(int i)
        {
            if (i >= 0 && i < tbctrl.Items.Count)
            {
                tbctrl.SelectedIndex = i;
                tbctrl_SelectionChanged(null, null);
                //ResourceDictionary rd = Application.LoadComponent(new Uri("Primes;component/WpfControls/Resources/Brushes.xaml", UriKind.Relative)) as ResourceDictionary;
                //(tbctrl.Items[i] as TabItem).Background = rd["HorizontalLightBrush"] as Brush;
            }
        }

        #endregion

        #region IPrimeUserControl Members

        public event VoidDelegate Execute;

        public event VoidDelegate Stop;

        private void FireExecuteEvent()
        {
            if (Execute != null)
            {
                Execute();
            }
        }

        private void FireStopEvent()
        {
            if (Stop != null)
            {
                Stop();
            }
        }

        #endregion

        #region IPrimeUserControl Members

        public void Init()
        {
            throw new NotImplementedException();
        }

        #endregion

        private void TabItem_HelpButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender == tabItemSieveOfEratosthenes)
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(Primes.OnlineHelp.OnlineHelpActions.Primetest_SieveOfEratosthenes);
            }
            else if (sender == tabItemMillerRabin)
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(Primes.OnlineHelp.OnlineHelpActions.Primetest_MillerRabin);
            }
            else if (sender == tabItemSoa)
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(Primes.OnlineHelp.OnlineHelpActions.Generation_SieveOfAtkin);
            }
        }
    }
}
