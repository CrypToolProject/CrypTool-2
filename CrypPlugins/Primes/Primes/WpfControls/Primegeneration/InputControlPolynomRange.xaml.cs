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
using Primes.WpfControls.Validation;
using Primes.WpfControls.Validation.Validator;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Primes.WpfControls.Primegeneration
{
    /// <summary>
    /// Interaction logic for InputControlPolynomRange.xaml
    /// </summary>
    public delegate void ExecutePolynomRangeDelegate(
      IPolynom p,
      PrimesBigInteger from,
      PrimesBigInteger to,
      PrimesBigInteger numberOfCalculations,
      PrimesBigInteger numberOfFormulars,
      IList<KeyValuePair<string, Range>> parameters);

    public partial class InputControlPolynomRange : UserControl
    {
        #region Properties

        private IPolynom m_Polynom;

        public IPolynom Polynom
        {
            get => m_Polynom;
            set
            {
                m_Polynom = value;
                gbTitle.Header = m_Polynom.Name;
                pnlImage.Children.Clear();
                pnlImage.Children.Add(m_Polynom.Image);
                pnlFactors.Children.Clear();
                pnlFactors.RowDefinitions.Clear();
                int i = 0;

                foreach (PolynomFactor factor in m_Polynom.Factors)
                {
                    RowDefinition rd = new RowDefinition
                    {
                        Height = new GridLength(1, GridUnitType.Auto)
                    };
                    pnlFactors.RowDefinitions.Add(rd);

                    RowDefinition rdpad = new RowDefinition
                    {
                        Height = new GridLength(10, GridUnitType.Pixel)
                    };
                    pnlFactors.RowDefinitions.Add(rdpad);

                    Label lbl = new Label
                    {
                        Content = factor.Name
                    };
                    Grid.SetColumn(lbl, 0);
                    Grid.SetRow(lbl, i);
                    pnlFactors.Children.Add(lbl);
                    InputRangeControl tbInput = CreateInputControl(factor);
                    Grid.SetColumn(tbInput, 1);
                    Grid.SetRow(tbInput, i);
                    pnlFactors.Children.Add(tbInput);

                    i += 2;
                }
            }
        }

        private InputRangeControl CreateInputControl(PolynomFactor factor)
        {
            InputRangeControl result = new InputRangeControl
            {
                InputRangeControlType = InputRangeControlType.Vertical,
                ShowCalcInput = false,
                ShowButtons = false,
                Width = (Width * 0.9) - 60,
                HorizontalAlignment = HorizontalAlignment.Left,
                Name = factor.Name,
                VerticalAlignment = VerticalAlignment.Top,
                Tag = factor.Name,
                Title = ""
            };
            result.Execute += new ExecuteDelegate(InputRangeControl_Execute);

            InputValidator<PrimesBigInteger> validatorFreeFrom = new InputValidator<PrimesBigInteger>
            {
                DefaultValue = "0",
                Validator = new BigIntegerValidator()
            };
            result.AddInputValidator(InputRangeControl.FreeFrom, validatorFreeFrom);

            InputValidator<PrimesBigInteger> validatorFreeTo = new InputValidator<PrimesBigInteger>
            {
                DefaultValue = "1",
                Validator = new BigIntegerValidator()
            };
            result.AddInputValidator(InputRangeControl.FreeTo, validatorFreeTo);

            return result;
        }

        private PolynomRangeExecuterMode m_PolynomRangeExecuterMode;

        public PolynomRangeExecuterMode PolynomRangeExecuterMode
        {
            get => m_PolynomRangeExecuterMode;
            set
            {
                m_PolynomRangeExecuterMode = value;
                pnlRandom.Visibility = (m_PolynomRangeExecuterMode == PolynomRangeExecuterMode.Random) ? Visibility.Visible : Visibility.Collapsed;
                pnlSystematic.Visibility = (m_PolynomRangeExecuterMode == PolynomRangeExecuterMode.Systematic) ? Visibility.Visible : Visibility.Collapsed;
                switch (m_PolynomRangeExecuterMode)
                {
                    case PolynomRangeExecuterMode.Random:
                        icNumberOfFormulars.SetText("Free", "50");
                        icNumberOfCalculations.SetText("Free", "100");
                        ircRandomChooseXRange.SetText("FreeFrom", "0");
                        ircRandomChooseXRange.SetText("FreeTo", "99");
                        break;
                    case PolynomRangeExecuterMode.Systematic:
                        ircSystematicChooseXRange.SetText("FreeFrom", "0");
                        ircSystematicChooseXRange.SetText("FreeTo", "99");
                        break;
                }
                foreach (Control element in pnlFactors.Children)
                {
                    if (element.GetType() == typeof(InputRangeControl))
                    {
                        InputRangeControl isc = element as InputRangeControl;
                        isc.SetText("FreeFrom", "0");
                        isc.SetText("FreeTo", "5");
                    }
                }
            }
        }

        #endregion

        #region Init

        public InputControlPolynomRange()
        {
            InitializeComponent();
            SetNumberOfCalculationsValidators();
            PolynomRangeExecuterMode = PolynomRangeExecuterMode.Random;
        }

        public InputControlPolynomRange(IPolynom expression)
            : this()
        {
            Polynom = expression;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        public void Stop()
        {
            UnlockAll();
            ControlHandler.SetButtonEnabled(btnExecute, true);
            ControlHandler.SetButtonEnabled(btnCancel, false);
        }

        public void Start()
        {
            LockAll();

            ControlHandler.SetButtonEnabled(btnExecute, false);
            ControlHandler.SetButtonEnabled(btnCancel, true);
        }

        private void SetNumberOfCalculationsValidators()
        {
            InputValidator<PrimesBigInteger> validatorNumberOfCalculations = new InputValidator<PrimesBigInteger>
            {
                DefaultValue = "1",
                Validator = new BigIntegerMinValueValidator(null, PrimesBigInteger.One)
            };
            icNumberOfCalculations.AddInputValidator(InputSingleControl.Free, validatorNumberOfCalculations);
        }

        #endregion

        #region Events

        public event ExecutePolynomRangeDelegate Execute;
        public event VoidDelegate Cancel;

        #endregion

        private void btnExecute_Click(object sender, RoutedEventArgs e)
        {
            DoExecute();
        }

        private void DoExecute()
        {
            bool doExecute = true;

            IList<KeyValuePair<string, Range>> parameters = new List<KeyValuePair<string, Range>>();

            foreach (Control element in pnlFactors.Children)
            {
                if (element.GetType() == typeof(InputRangeControl))
                {
                    InputRangeControl isc = element as InputRangeControl;
                    PrimesBigInteger _from = null;
                    PrimesBigInteger _to = null;

                    isc.GetValue(ref _from, ref _to);
                    if (_from != null && _to != null)
                    {
                        parameters.Add(new KeyValuePair<string, Range>(isc.Tag.ToString(), new Range(_from, _to)));
                    }
                    else
                    {
                        doExecute = false;
                    }
                }
            }

            if (Execute != null)
            {
                if (doExecute)
                {
                    try
                    {
                        if (m_PolynomRangeExecuterMode == PolynomRangeExecuterMode.Random)
                        {
                            ExecuteRandom(parameters);
                        }
                        else
                        {
                            ExecuteSystematic(parameters);
                        }
                    }
                    catch { }
                }
                else
                {
                    UnlockAll();
                }
            }
        }

        private void ExecuteRandom(IList<KeyValuePair<string, Range>> parameters)
        {
            PrimesBigInteger numberOfCalculations = icNumberOfCalculations.GetValue();
            PrimesBigInteger numberOfFormulars = icNumberOfFormulars.GetValue();
            PrimesBigInteger xfrom = null;
            PrimesBigInteger xto = null;
            ircRandomChooseXRange.GetValue(ref xfrom, ref xto);

            if (numberOfFormulars != null)
            {
                if (rbChooseRange.IsChecked.Value)
                {
                    if (xfrom != null && xto != null)
                    {
                        numberOfCalculations = PrimesBigInteger.NaN;
                        Execute(m_Polynom, xfrom, xto, numberOfCalculations, numberOfFormulars, parameters);
                    }
                }
                else if (rbChooseRandom.IsChecked.Value)
                {
                    if (numberOfCalculations != null)
                    {
                        xfrom = PrimesBigInteger.NaN;
                        xto = PrimesBigInteger.NaN;

                        Execute(m_Polynom, xfrom, xto, numberOfCalculations, numberOfFormulars, parameters);
                    }
                }
            }
        }

        private void ExecuteSystematic(IList<KeyValuePair<string, Range>> parameters)
        {
            PrimesBigInteger xfrom = null;
            PrimesBigInteger xto = null;
            ircSystematicChooseXRange.GetValue(ref xfrom, ref xto);
            if (xfrom != null && xto != null)
            {
                Execute(m_Polynom, xfrom, xto, PrimesBigInteger.NaN, PrimesBigInteger.NaN, parameters);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            UnlockAll();

            if (Cancel != null)
            {
                Cancel();
            }
        }

        private void rbChoose_Click(object sender, RoutedEventArgs e)
        {
            if (sender == rbChooseRandom)
            {
                icNumberOfCalculations.IsEnabled = rbChooseRandom.IsChecked.Value;
                ircRandomChooseXRange.IsEnabled = !rbChooseRandom.IsChecked.Value;
            }
            else if (sender == rbChooseRange)
            {
                icNumberOfCalculations.IsEnabled = !rbChooseRange.IsChecked.Value;
                ircRandomChooseXRange.IsEnabled = rbChooseRange.IsChecked.Value;
            }
        }

        private void InputSingleControl_Execute(PrimesBigInteger value)
        {
            DoExecute();
        }

        private void InputRangeControl_Execute(PrimesBigInteger from, PrimesBigInteger to, PrimesBigInteger second)
        {
            DoExecute();
        }

        public void UnlockAll()
        {
            ControlHandler.SetPropertyValue(icNumberOfCalculations, "IsEnabled", true);
            ControlHandler.SetPropertyValue(ircRandomChooseXRange, "IsEnabled", true);
            ControlHandler.SetPropertyValue(icNumberOfFormulars, "IsEnabled", true);
            ControlHandler.SetPropertyValue(ircSystematicChooseXRange, "IsEnabled", true);

            ControlHandler.SetPropertyValue(rbChooseRandom, "IsEnabled", true);
            ControlHandler.SetPropertyValue(rbChooseRange, "IsEnabled", true);

            UIElementCollection childs = ControlHandler.GetPropertyValue(pnlFactors, "Children") as UIElementCollection;
            if (childs != null)
            {
                IEnumerator _enum = ControlHandler.ExecuteMethod(childs, "GetEnumerator") as IEnumerator;
                while ((bool)ControlHandler.ExecuteMethod(_enum, "MoveNext"))
                {
                    UIElement element = ControlHandler.GetPropertyValue(_enum, "Current") as UIElement;
                    if (element.GetType() == typeof(InputRangeControl))
                    {
                        (element as InputRangeControl).UnLockControls();
                    }
                }
            }
        }

        public void LockAll()
        {
            ControlHandler.SetPropertyValue(icNumberOfCalculations, "IsEnabled", false);
            ControlHandler.SetPropertyValue(ircRandomChooseXRange, "IsEnabled", false);
            ControlHandler.SetPropertyValue(icNumberOfFormulars, "IsEnabled", false);
            ControlHandler.SetPropertyValue(ircSystematicChooseXRange, "IsEnabled", false);

            ControlHandler.SetPropertyValue(rbChooseRandom, "IsEnabled", false);
            ControlHandler.SetPropertyValue(rbChooseRange, "IsEnabled", false);

            UIElementCollection childs = ControlHandler.GetPropertyValue(pnlFactors, "Children") as UIElementCollection;
            if (childs != null)
            {
                IEnumerator _enum = ControlHandler.ExecuteMethod(childs, "GetEnumerator") as IEnumerator;
                while ((bool)ControlHandler.ExecuteMethod(_enum, "MoveNext"))
                {
                    UIElement element = ControlHandler.GetPropertyValue(_enum, "Current") as UIElement;
                    if (element.GetType() == typeof(InputRangeControl))
                    {
                        (element as InputRangeControl).LockControls();
                    }
                }
            }
        }
    }
}
