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

using CrypTool.PluginBase.Miscellaneous;
using Primes.Bignum;
using Primes.Library;
using System;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Primes.WpfControls.Components
{
    /// <summary>
    /// Interaction logic for GenerateRandomControl.xaml
    /// </summary>

    public partial class GenerateRandomControl : UserControl
    {
        public event GmpBigIntegerParameterDelegate OnRandomNumberGenerated;

        public BigInteger MaxValue = 10000;

        public GenerateRandomControl()
        {
            InitializeComponent();
        }

        private void FireOnRandomNumberGenerated(PrimesBigInteger value)
        {
            if (OnRandomNumberGenerated != null)
            {
                OnRandomNumberGenerated(value);
            }
        }

        private Primes.OnlineHelp.OnlineHelpActions m_HelpAction;

        public Primes.OnlineHelp.OnlineHelpActions HelpAction
        {
            get => m_HelpAction;
            set => m_HelpAction = value;
        }

        private void ImageHelpClick(object sender, MouseButtonEventArgs e)
        {
            OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(m_HelpAction);
            e.Handled = true;
        }

        public string Title
        {
            get
            {
                if (miHeader != null && miHeader.Header != null)
                {
                    return miHeader.Header.ToString();
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (miHeader != null)
                {
                    miHeader.Header = value;
                }
            }
        }

        public bool ShowMultipleFactors
        {
            get => miIntegerManyFactors.Visibility == Visibility.Visible;
            set
            {
                if (value)
                {
                    miIntegerManyFactors.Visibility = Visibility.Visible;
                }
                else
                {
                    miIntegerManyFactors.Visibility = Visibility.Collapsed;
                }
            }
        }

        public bool ShowTwoBigFactors
        {
            get => miTowBigFactors.Visibility == Visibility.Visible;
            set
            {
                if (value)
                {
                    miTowBigFactors.Visibility = Visibility.Visible;
                }
                else
                {
                    miTowBigFactors.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void miIntegerManyFactors_Click(object sender, RoutedEventArgs e)
        {
            PrimesBigInteger value = null;

            try
            {
                if (sender == miPrime)
                {
                    value = new PrimesBigInteger(MaxValue.RandomPrimeLimit());
                }
                else if (sender == miBigInteger)
                {
                    value = new PrimesBigInteger(MaxValue.RandomIntLimit());
                }
                else if (sender == miIntegerManyFactors)
                {
                    int bits = Math.Max(MaxValue.BitCount() / 16, 8);
                    BigInteger p = MaxValue.Sqrt().RandomIntLimit();
                    for (int i = 0; i < 16; i++)
                    {
                        BigInteger pp = p * BigIntegerHelper.RandomPrimeBits(bits);
                        if (pp < MaxValue)
                        {
                            p = pp;
                        }
                    }
                    value = new PrimesBigInteger(p);
                }
                else if (sender == miTowBigFactors)
                {
                    BigInteger a = MaxValue.Sqrt().RandomPrimeLimit();
                    BigInteger b = (MaxValue / a).RandomPrimeLimit();
                    value = new PrimesBigInteger(a * b);
                }
            }
            catch
            {
                return;
            }

            if (value != null)
            {
                FireOnRandomNumberGenerated(value);
            }
        }
    }
}
