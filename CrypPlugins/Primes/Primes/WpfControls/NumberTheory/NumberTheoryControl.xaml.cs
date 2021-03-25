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

using Primes.WpfControls.NumberTheory.PowerMod;
using System.Windows;
using System.Windows.Controls;

namespace Primes.WpfControls.NumberTheory
{
    /// <summary>
    /// Interaction logic for NumberTheoryControl.xaml
    /// </summary>
    public partial class NumberTheoryControl : UserControl, IPrimeMethodDivision
    {
        public NumberTheoryControl()
        {
            InitializeComponent();
        }

        #region IPrimeMethodDivision Members

        public void Dispose()
        {
            ((PowerModControl)tabItemPower.Content).CleanUp();
            ((PowerModControl)tabItemPowerBase.Content).CleanUp();
        }

        public void Init()
        {
        }

        public void SetTab(int i)
        {
            if (i >= 0 && i < tbctrl.Items.Count)
            {
                tbctrl.SelectedIndex = i;
            }
        }

        #endregion

        private void TabItem_HelpButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender == tabItemNTFunctions)
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(Primes.OnlineHelp.OnlineHelpActions.Numbertheoretic_Functions);
            }
            else if (sender == tabItemPower)
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(Primes.OnlineHelp.OnlineHelpActions.Modular_Exponentiation);
            }
            else if (sender == tabItemPowerBase)
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(Primes.OnlineHelp.OnlineHelpActions.Modular_Exponentiation);
            }
            else if (sender == tabItemPRoots)
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(Primes.OnlineHelp.OnlineHelpActions.PrimitivRoot_PrimitivRoot);
            }
            else if (sender == tabItemGoldbach)
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(Primes.OnlineHelp.OnlineHelpActions.Distribution_Goldbach);
            }
        }
    }
}
