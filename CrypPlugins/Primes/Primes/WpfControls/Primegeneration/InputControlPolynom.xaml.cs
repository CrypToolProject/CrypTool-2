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
using System.Windows;
using System.Windows.Controls;

namespace Primes.WpfControls.Primegeneration
{
    /// <summary>
    /// Interaction logic for InputControlExpression.xaml
    /// </summary>
    internal delegate void ExecutePolynomDelegate(PrimesBigInteger from, PrimesBigInteger to, IPolynom p);
    public partial class InputControlPolynom : UserControl
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
                    Label lbl = new Label
                    {
                        Margin = new Thickness(0, 7, 0, 0),
                        Content = factor.Name
                    };
                    Grid.SetColumn(lbl, 0);
                    Grid.SetRow(lbl, i);
                    pnlFactors.Children.Add(lbl);
                    InputSingleControl tbInput = CreateInputControl(factor);
                    Grid.SetColumn(tbInput, 1);
                    Grid.SetRow(tbInput, i);
                    pnlFactors.Children.Add(tbInput);

                    i++;
                }
            }
        }

        private InputSingleControl CreateInputControl(PolynomFactor factor)
        {
            InputSingleControl result = new InputSingleControl
            {
                InputRangeControlType = InputRangeControlType.Vertical,
                ShowCalcInput = false,
                ShowButtons = false,
                Width = (Width * 0.9) - 60,
                HorizontalAlignment = HorizontalAlignment.Left,
                Name = factor.Name,
                VerticalAlignment = VerticalAlignment.Top,
                Tag = factor.Name,
                FreeText = factor.Value.ToString(),
                IsEnabled = !factor.Readonly
            };

            InputValidator<PrimesBigInteger> validatorFree = new InputValidator<PrimesBigInteger>
            {
                DefaultValue = "0",
                Validator = new BigIntegerValidator()
            };
            result.AddInputValidator(InputSingleControl.Free, validatorFree);

            InputValidator<PrimesBigInteger> validatorFactor = new InputValidator<PrimesBigInteger>
            {
                DefaultValue = "0",
                Validator = new BigIntegerMinValueValidator(null, PrimesBigInteger.Zero)
            };

            InputValidator<PrimesBigInteger> validatorBase = new InputValidator<PrimesBigInteger>
            {
                DefaultValue = "1",
                Validator = new BigIntegerValidator()
            };

            InputValidator<PrimesBigInteger> validatorExp = new InputValidator<PrimesBigInteger>
            {
                DefaultValue = "0",
                Validator = new BigIntegerValidator()
            };

            InputValidator<PrimesBigInteger> validatorSum = new InputValidator<PrimesBigInteger>
            {
                DefaultValue = "0",
                Validator = new BigIntegerMinValueMaxValueValidator(null, PrimesBigInteger.ValueOf(-100), PrimesBigInteger.ValueOf(100))
            };
            result.AddInputValidator(InputSingleControl.CalcFactor, validatorFactor);
            result.AddInputValidator(InputSingleControl.CalcBase, validatorBase);
            result.AddInputValidator(InputSingleControl.CalcExp, validatorExp);
            result.AddInputValidator(InputSingleControl.CalcSum, validatorSum);

            return result;
        }

        private InputRangeControl CreateInputControl(RangePolynomFactor factor)
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
                Tag = factor.Name
            };

            InputValidator<PrimesBigInteger> validatorFreeFrom = new InputValidator<PrimesBigInteger>
            {
                DefaultValue = "0",
                Validator = new BigIntegerMinValueMaxValueValidator(null, PrimesBigInteger.ValueOf(-1000), PrimesBigInteger.ValueOf(1000))
            };
            result.AddInputValidator(InputRangeControl.FreeFrom, validatorFreeFrom);

            InputValidator<PrimesBigInteger> validatorFreeTo = new InputValidator<PrimesBigInteger>
            {
                DefaultValue = "0",
                Validator = new BigIntegerMinValueMaxValueValidator(null, PrimesBigInteger.ValueOf(-1000), PrimesBigInteger.ValueOf(1000))
            };
            result.AddInputValidator(InputRangeControl.FreeTo, validatorFreeTo);

            return result;
        }

        #endregion

        #region Init

        public InputControlPolynom()
        {
            InitializeComponent();
            SetFreeInputValidators();
            SetCalcInputValidators();
        }

        public InputControlPolynom(IPolynom expression)
            : this()
        {
            Polynom = expression;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            rangeinputcontrol.Execute += new Primes.WpfControls.Components.ExecuteDelegate(rangeinputcontrol_Execute);
            rangeinputcontrol.Cancel += new VoidDelegate(rangeinputcontrol_Cancel);

            //rangeinputcontrol.AddSingleAdisors(InputRangeControl.From, new LargeNumberAdvisor(PrimesBigInteger.ValueOf(200), OnlineHelpActions.Graph_LargeNumbers));
            //rangeinputcontrol.AddSingleAdisors(InputRangeControl.To, new LargeNumberAdvisor(PrimesBigInteger.ValueOf(200), OnlineHelpActions.Graph_LargeNumbers));
        }

        private void rangeinputcontrol_Cancel()
        {
            UnlockAll();
            if (Cancel != null)
            {
                Cancel();
            }
        }

        public void Stop()
        {
            UnlockAll();
            rangeinputcontrol.UnLockControls();
        }

        private void SetFreeInputValidators()
        {
            InputValidator<PrimesBigInteger> validatorFrom = new InputValidator<PrimesBigInteger>
            {
                DefaultValue = "0",
                Validator = new BigIntegerMinValueValidator(null, PrimesBigInteger.Zero)
            };

            InputValidator<PrimesBigInteger> validatorTo = new InputValidator<PrimesBigInteger>
            {
                DefaultValue = "1",
                Validator = new BigIntegerMinValueValidator(null, PrimesBigInteger.One)
            };

            rangeinputcontrol.AddInputValidator(InputRangeControl.FreeFrom, validatorFrom);
            rangeinputcontrol.AddInputValidator(InputRangeControl.FreeTo, validatorTo);
        }

        private void SetCalcInputValidators()
        {
            InputValidator<PrimesBigInteger> validatorFromFactor = new InputValidator<PrimesBigInteger>
            {
                DefaultValue = "1",
                Validator = new BigIntegerMinValueValidator(null, PrimesBigInteger.Zero)
            };

            InputValidator<PrimesBigInteger> validatorFromBase = new InputValidator<PrimesBigInteger>
            {
                DefaultValue = "1",
                Validator = new BigIntegerMinValueValidator(null, PrimesBigInteger.One)
            };

            InputValidator<PrimesBigInteger> validatorFromExp = new InputValidator<PrimesBigInteger>
            {
                DefaultValue = "0",
                Validator = new BigIntegerMinValueValidator(null, PrimesBigInteger.Zero)
            };

            InputValidator<PrimesBigInteger> validatorFromSum = new InputValidator<PrimesBigInteger>
            {
                DefaultValue = "0",
                Validator = new BigIntegerMinValueMaxValueValidator(null, PrimesBigInteger.ValueOf(-100), PrimesBigInteger.ValueOf(100))
            };

            InputValidator<PrimesBigInteger> validatorToFactor = new InputValidator<PrimesBigInteger>
            {
                DefaultValue = "1",
                Validator = new BigIntegerMinValueValidator(null, PrimesBigInteger.Zero)
            };

            InputValidator<PrimesBigInteger> validatorToBase = new InputValidator<PrimesBigInteger>
            {
                DefaultValue = "1",
                Validator = new BigIntegerMinValueValidator(null, PrimesBigInteger.One)
            };

            InputValidator<PrimesBigInteger> validatorToExp = new InputValidator<PrimesBigInteger>
            {
                DefaultValue = "0",
                Validator = new BigIntegerMinValueValidator(null, PrimesBigInteger.Zero)
            };

            InputValidator<PrimesBigInteger> validatorToSum = new InputValidator<PrimesBigInteger>
            {
                DefaultValue = "0",
                Validator = new BigIntegerMinValueMaxValueValidator(null, PrimesBigInteger.ValueOf(-100), PrimesBigInteger.ValueOf(100))
            };

            rangeinputcontrol.AddInputValidator(InputRangeControl.CalcFromFactor, validatorFromFactor);
            rangeinputcontrol.AddInputValidator(InputRangeControl.CalcFromBase, validatorFromBase);
            rangeinputcontrol.AddInputValidator(InputRangeControl.CalcFromExp, validatorFromExp);
            rangeinputcontrol.AddInputValidator(InputRangeControl.CalcFromSum, validatorFromSum);
            rangeinputcontrol.AddInputValidator(InputRangeControl.CalcToFactor, validatorToFactor);
            rangeinputcontrol.AddInputValidator(InputRangeControl.CalcToBase, validatorToBase);
            rangeinputcontrol.AddInputValidator(InputRangeControl.CalcToExp, validatorToExp);
            rangeinputcontrol.AddInputValidator(InputRangeControl.CalcToSum, validatorToSum);
        }

        public void SetText(string key, string value)
        {
            rangeinputcontrol.SetText(key, value);
        }

        public void SetPolynomParameterText(string name, string value)
        {
            foreach (Control element in pnlFactors.Children)
            {
                if (element.GetType() == typeof(InputSingleControl))
                {
                    InputSingleControl isc = element as InputSingleControl;
                    if (name.Equals(isc.Tag))
                    {
                        isc.SetText(InputSingleControl.Free, value);
                    }
                }
            }
        }

        #endregion

        #region Events

        internal event ExecutePolynomDelegate Execute;
        internal event VoidDelegate Cancel;

        #endregion

        private void rangeinputcontrol_Execute(PrimesBigInteger from, PrimesBigInteger to, PrimesBigInteger second)
        {
            bool doExecute = true;

            foreach (Control element in pnlFactors.Children)
            {
                if (element.GetType() == typeof(InputSingleControl))
                {
                    InputSingleControl isc = element as InputSingleControl;
                    PrimesBigInteger i = isc.GetValue();
                    doExecute &= i != null;
                    if (doExecute)
                    {
                        m_Polynom.SetParameter(isc.Tag.ToString(), i);
                    }
                }
            }

            if (Execute != null && doExecute)
            {
                LockAll();
                Execute(from, to, m_Polynom);
            }
        }

        public void UnlockAll()
        {
            rangeinputcontrol.UnLockControls();

            //ControlHandler.SetPropertyValue(icNumberOfCalculations, "IsEnabled", true);
            //ControlHandler.SetPropertyValue(ircRandomChooseXRange, "IsEnabled", true);
            //ControlHandler.SetPropertyValue(icNumberOfFormulars, "IsEnabled", true);
            //ControlHandler.SetPropertyValue(ircSystematicChooseXRange, "IsEnabled", true);

            //ControlHandler.SetPropertyValue(rbChooseRandom, "IsEnabled", true);
            //ControlHandler.SetPropertyValue(rbChooseRange, "IsEnabled", true);

            UIElementCollection childs = ControlHandler.GetPropertyValue(pnlFactors, "Children") as UIElementCollection;
            if (childs != null)
            {
                IEnumerator _enum = ControlHandler.ExecuteMethod(childs, "GetEnumerator") as IEnumerator;
                while ((bool)ControlHandler.ExecuteMethod(_enum, "MoveNext"))
                {
                    UIElement element = ControlHandler.GetPropertyValue(_enum, "Current") as UIElement;
                    if (element.GetType() == typeof(InputSingleControl))
                    {
                        (element as InputSingleControl).UnLockControls();
                    }
                }
            }
        }

        public void LockAll()
        {
            rangeinputcontrol.LockControls();
            //ControlHandler.SetPropertyValue(ircRandomChooseXRange, "IsEnabled", false);
            //ControlHandler.SetPropertyValue(icNumberOfFormulars, "IsEnabled", false);
            //ControlHandler.SetPropertyValue(ircSystematicChooseXRange, "IsEnabled", false);

            //ControlHandler.SetPropertyValue(rbChooseRandom, "IsEnabled", false);
            //ControlHandler.SetPropertyValue(rbChooseRange, "IsEnabled", false);

            UIElementCollection childs = ControlHandler.GetPropertyValue(pnlFactors, "Children") as UIElementCollection;
            if (childs != null)
            {
                IEnumerator _enum = ControlHandler.ExecuteMethod(childs, "GetEnumerator") as IEnumerator;
                while ((bool)ControlHandler.ExecuteMethod(_enum, "MoveNext"))
                {
                    UIElement element = ControlHandler.GetPropertyValue(_enum, "Current") as UIElement;
                    if (element.GetType() == typeof(InputSingleControl))
                    {
                        (element as InputSingleControl).LockControls();
                    }
                }
            }
        }
    }
}
