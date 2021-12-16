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

using Primes.Library;
using System.Windows;
using System.Windows.Controls;

namespace Primes.WpfVisualization.Navigation
{
    /// <summary>
    /// Interaction logic for Navigation.xaml
    /// </summary>
    public partial class Navigation : UserControl
    {
        public event Navigate OnNavigate;

        public Navigation()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void expander1_Collapsed(object sender, RoutedEventArgs e)
        {
            if (sender.GetType() == typeof(Expander))
            {
                HandleNavigation((sender as Expander));
            }
        }

        public void HandleNavigation(Expander exp)
        {
            //StackPanel spTarget = null;
            //switch (exp.Name)
            //{
            //  case "expander1":
            //    spTarget = stackPanel2;
            //    break;
            //  case "expander2":
            //    //spTarget = stackPanel3;
            //    break;
            //}
            //if (spTarget != null)
            //{
            //  spTarget.Visibility = (exp.IsExpanded) ? Visibility.Collapsed : Visibility.Visible;
            //}
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //if(OnNavigate!=null)
            //{
            //  NavigationCommandType commandtype = NavigationCommandType.None;
            //  if (sender == btnFaktorisierung) commandtype = NavigationCommandType.Factor;
            //  else if (sender == btnPrimeDistribution) commandtype = NavigationCommandType.Graph;
            //  else if (sender == btnStartPage) commandtype = NavigationCommandType.Start;
            //  else if (sender == btnPrimeTest) commandtype = NavigationCommandType.Primetest;
            //  else if (sender == btnPrimeSpirals) commandtype = NavigationCommandType.Primespirals;
            //  else if (sender == btnGeneratePrimes) commandtype = NavigationCommandType.Primesgeneration;
            //  else if (sender == btnNumerline) commandtype = NavigationCommandType.Numberline;
            //  else if (sender == btnPrimitiveRoot) commandtype = NavigationCommandType.PrimitivRoot;

            //  OnNavigate(commandtype);
            //}
        }

        private void lnk_Click(object sender, RoutedEventArgs e)
        {
            if (OnNavigate != null)
            {
                NavigationCommandType commandtype = NavigationCommandType.None;
                if (sender == lnkFacBruteForce)
                {
                    commandtype = NavigationCommandType.Factor_Bf;
                }
                else if (sender == lnkFacQS)
                {
                    commandtype = NavigationCommandType.Factor_QS;
                }
                else if (sender == lnkTestEratothenes)
                {
                    commandtype = NavigationCommandType.Primetest_Sieve;
                }
                else if (sender == lnkTestMillerRabin)
                {
                    commandtype = NavigationCommandType.Primetest_Miller;
                }
                else if (sender == lnkDistribNumberline)
                {
                    commandtype = NavigationCommandType.PrimeDistrib_Numberline;
                }
                else if (sender == lnkDistribNumberrec)
                {
                    commandtype = NavigationCommandType.PrimeDistrib_Numberrec;
                }
                else if (sender == lnkDistribUlam)
                {
                    commandtype = NavigationCommandType.PrimeDistrib_Ulam;
                }
                else if (sender == lnkPrimitivRoots)
                {
                    commandtype = NavigationCommandType.PrimitivRoot;
                }
                else if (sender == lnkGenPrimes)
                {
                    commandtype = NavigationCommandType.Primesgeneration;
                }
                else if (sender == lnkCountPrimes)
                {
                    commandtype = NavigationCommandType.Graph;
                }
                else if (sender == lnkStartPage)
                {
                    commandtype = NavigationCommandType.Start;
                }
                else if (sender == lnkNumberTheoryFunctions)
                {
                    commandtype = NavigationCommandType.NumberTheoryFunctions;
                }
                else if (sender == lnkSieveOfAtkin)
                {
                    commandtype = NavigationCommandType.SieveOfAtkin;
                }
                else if (sender == lnkDistribGoldbach)
                {
                    commandtype = NavigationCommandType.PrimeDistrib_Goldbach;
                }
                else if (sender == lnkPowMod)
                {
                    commandtype = NavigationCommandType.PowerMod;
                }
                else if (sender == lnkPowBaseMod)
                {
                    commandtype = NavigationCommandType.PowerBaseMod;
                }

                OnNavigate(commandtype);
            }
        }

        private void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            if (sender.GetType() == typeof(TreeViewItem))
            {
                (sender as TreeViewItem).IsExpanded = true;
            }
        }

        private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
        }
    }
}
