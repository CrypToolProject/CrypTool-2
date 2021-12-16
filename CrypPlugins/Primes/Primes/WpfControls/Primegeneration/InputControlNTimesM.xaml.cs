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
using Primes.WpfControls.Validation;
using Primes.WpfControls.Validation.ControlValidator;
using Primes.WpfControls.Validation.ControlValidator.Exceptions;
using Primes.WpfControls.Validation.Validator;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Primes.WpfControls.Primegeneration
{
    /// <summary>
    /// Interaction logic for InputControlNTimesM.xaml
    /// </summary>
    internal delegate void Execute_N_Digits_Delegate(PrimesBigInteger n, PrimesBigInteger digits);

    public partial class InputControlNTimesM : UserControl
    {
        #region Events

        internal event Execute_N_Digits_Delegate Execute;
        internal event VoidDelegate Cancel;

        #endregion

        #region Properties

        private PrimesBigInteger m_MaxDigits;

        public PrimesBigInteger MaxDigits
        {
            get => m_MaxDigits;
            set => m_MaxDigits = value;
        }

        #endregion

        public InputControlNTimesM()
        {
            InitializeComponent();
            m_MaxDigits = PrimesBigInteger.ValueOf(500);
        }

        private void btnExec_Click(object sender, RoutedEventArgs e)
        {
            PrimesBigInteger n = GetPrimesCount();
            PrimesBigInteger digits = GetDigits();
            if (digits != null && n != null)
            {
                if (digits.CompareTo(m_MaxDigits) > 0)
                {
                    Info(string.Format(Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.ntimesm_errordigits, m_MaxDigits.ToString()), tbM);
                }
                else
                {
                    if (Execute != null)
                    {
                        Execute(n, digits);
                    }
                    //if (Execute != null) Execute(n, digits.Subtract(PrimesBigInteger.One));
                }
            }
        }

        private PrimesBigInteger GetPrimesCount()
        {
            PrimesBigInteger result = PrimesBigInteger.One;

            try
            {
                IValidator<PrimesBigInteger> validator = new BigIntegerMinValueValidator(tbN.Text, PrimesBigInteger.ValueOf(1));
                TextBoxValidator<PrimesBigInteger> tbValidator = new TextBoxValidator<PrimesBigInteger>(validator, tbN, "1");
                tbValidator.Validate(ref result);
            }
            catch (ControlValidationException cvex)
            {
                switch (cvex.ValidationResult)
                {
                    case Primes.WpfControls.Validation.ValidationResult.WARNING:
                        Info(cvex.Message, cvex.Control as TextBox);
                        break;
                    case Primes.WpfControls.Validation.ValidationResult.ERROR:
                        Error(cvex.Message, cvex.Control as TextBox);
                        break;
                }
            }

            return result;
        }

        private PrimesBigInteger GetDigits()
        {
            PrimesBigInteger result = PrimesBigInteger.One;

            try
            {
                IValidator<PrimesBigInteger> validator = new BigIntegerMinValueValidator(tbM.Text, PrimesBigInteger.Two);
                TextBoxValidator<PrimesBigInteger> tbValidator = new TextBoxValidator<PrimesBigInteger>(validator, tbM, "2");
                tbValidator.Validate(ref result);
            }
            catch (ControlValidationException cvex)
            {
                switch (cvex.ValidationResult)
                {
                    case Primes.WpfControls.Validation.ValidationResult.WARNING:
                        Info(cvex.Message, cvex.Control as TextBox);
                        break;
                    case Primes.WpfControls.Validation.ValidationResult.ERROR:
                        Error(cvex.Message, cvex.Control as TextBox);
                        break;
                }
            }

            return result;
        }

        private void Info(string message, TextBox tb)
        {
            if (!string.IsNullOrEmpty(message) && tb != null)
            {
                pnlInfo.Visibility = Visibility.Visible;
                tbInfo.Foreground = Brushes.Blue;
                tb.Background = Brushes.LightBlue;
                tbInfo.Text = message;
            }
            else
            {
                pnlInfo.Visibility = Visibility.Collapsed;
                tbInfo.Text = string.Empty;
                tbInfo.Foreground = Brushes.Black;
                tbInfo.Background = Brushes.White;
            }
        }

        private void Error(string message, TextBox tb)
        {
            if (!string.IsNullOrEmpty(message) && tb != null)
            {
                pnlInfo.Visibility = Visibility.Visible;
                tbInfo.Foreground = Brushes.WhiteSmoke;
                tb.Background = Brushes.Red;
                tbInfo.Text = message;
            }
            else
            {
                pnlInfo.Visibility = Visibility.Collapsed;
                tbInfo.Text = string.Empty;
                tbInfo.Foreground = Brushes.Black;
                tbInfo.Background = Brushes.White;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (Cancel != null)
            {
                Cancel();
            }
        }

        public void SetButtonExecuteEnable(bool enabled)
        {
            ControlHandler.SetButtonEnabled(btnExec, enabled);
        }

        public void SetButtonCancelEnable(bool enabled)
        {
            ControlHandler.SetButtonEnabled(btnCancel, enabled);
        }
    }
}
