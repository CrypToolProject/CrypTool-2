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

using Primes.WpfControls.PrimesDistribution.Numberline;
using Primes.WpfControls.PrimesDistribution.NumberRectangle;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Primes.WpfControls.PrimesDistribution
{
    /// <summary>
    /// Interaction logic for PrimesInNaturalNumbersControl.xaml
    /// </summary>
    public partial class PrimesInNaturalNumbersControl : UserControl, IPrimeMethodDivision
    {
        public PrimesInNaturalNumbersControl()
        {
            InitializeComponent();
            //numberlinectrl.Execute += new VoidDelegate(numberlinectrl_Execute);
            //numberlinectrl.Stop += new VoidDelegate(numberlinectrl_Stop);

            tabItemNumberline.OnTabContentChanged += content =>
            {
                if (tbctrl.SelectedItem == tabItemNumberline)
                {
                    NumberLineCtrl.Init();
                }
            };

            tabItemNumbergrid.OnTabContentChanged += content =>
            {
                if (tbctrl.SelectedItem == tabItemNumbergrid)
                {
                    NumberRectCtrl.Init();
                }
            };
        }

        #region IPrimeUserControl Members

        public void Dispose()
        {
        }

        #endregion

        private NumberlineControl NumberLineCtrl => (NumberlineControl)tabItemNumberline.Content;
        private NumberRectangleControl NumberRectCtrl => (NumberRectangleControl)tabItemNumbergrid.Content;

        private void tbctrl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NumberLineCtrl.Dispose();
            NumberRectCtrl.Dispose();

            if (tbctrl.SelectedItem == tabItemNumberline)
            {
                NumberLineCtrl.Init();
            }
            else if (tbctrl.SelectedItem == tabItemNumbergrid)
            {
                NumberRectCtrl.Init();
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            NumberLineCtrl.Width = sizeInfo.NewSize.Width;
            NumberLineCtrl.Init();
        }

        #region IPrimeUserControl Members

        public void SetTab(int i)
        {
            if (i >= 0 && i < tbctrl.Items.Count)
            {
                tbctrl.SelectedIndex = i;
            }
        }

        #endregion

        #region IPrimeUserControl Members

        public void Init()
        {
            throw new NotImplementedException();
        }

        #endregion

        private void tabitem_HelpButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender == tabItemUlam)
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(Primes.OnlineHelp.OnlineHelpActions.Spiral_Ulam);
            }
            else if (sender == tabItemNumbergrid)
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(Primes.OnlineHelp.OnlineHelpActions.Distribution_Numbergrid);
            }
            else if (sender == tabItemNumberline)
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(Primes.OnlineHelp.OnlineHelpActions.Distribution_Numberline);
            }
            else if (sender == tabItemGraph)
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(Primes.OnlineHelp.OnlineHelpActions.Graph_PrimesCount);
            }
            //else if (sender == tabItemGoldbach)
            //{
            //    OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(Primes.OnlineHelp.OnlineHelpActions.Distribution_Goldbach);
            //}

            e.Handled = true;
        }
    }
}
