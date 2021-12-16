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
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Primes.WpfControls.Components
{
    /// <summary>
    /// Interaction logic for InputRangeControl.xaml
    /// </summary>

    public delegate void ExecuteDelegate(PrimesBigInteger from, PrimesBigInteger to, PrimesBigInteger second);

    public enum InputRangeControlType { Vertical, Horizontal };

    public partial class InputRangeControl : UserControl
    {

        #region Constants

        public const string FreeFrom = "FreeFrom";
        public const string FreeTo = "FreeTo";
        public const string CalcFromFactor = "CalcFromFactor";
        public const string CalcFromBase = "CalcFromBase";
        public const string CalcFromExp = "CalcFromExp";
        public const string CalcFromSum = "CalcFromSum";
        public const string CalcToFactor = "CalcToFactor";
        public const string CalcToBase = "CalcToBase";
        public const string CalcToExp = "CalcToExp";
        public const string CalcToSum = "CalcToSum";
        public const string From = "From";
        public const string To = "To";
        public const string SecondParameter = "SecondParameter";

        #endregion

        private enum Selection { Free, Calc }

        public InputRangeControl()
        {
            InitializeComponent();

            m_Validators = new Dictionary<string, InputValidator<PrimesBigInteger>>();
            m_ValueValidators = new Dictionary<string, IValidator<PrimesBigInteger>>();
            m_SingleAdvisors = new Dictionary<string, IList<IValidator<PrimesBigInteger>>>
            {
                { From, new List<IValidator<PrimesBigInteger>>() },
                { To, new List<IValidator<PrimesBigInteger>>() }
            };
            SecondParameterPresent = false;
        }

        #region Components

        private RadioButton m_RbFree;
        private RadioButton m_RbCalc;
        private TextBlock m_LblInfoFree;
        private TextBlock m_LblInfoCalc;
        private TextBlock m_LblInfoSecond;

        // Textboxes Free
        private TextBox m_tbFromFree;
        private TextBox m_tbToFree;

        // Textboxes Calc
        private TextBox m_tbFromCalcFactor;
        private TextBox m_tbFromCalcBase;
        private TextBox m_tbFromCalcExp;
        private TextBox m_tbFromCalcSum;
        private TextBox m_tbToCalcFactor;
        private TextBox m_tbToCalcBase;
        private TextBox m_tbToCalcExp;
        private TextBox m_tbToCalcSum;

        private Image m_HelpImageFree;
        private Image m_HelpImageCalc;

        private UIElement m_InputPnlFree;
        private UIElement m_InputPnlCalc;
        private DockPanel m_PnlFree;
        private DockPanel m_PnlCalc;

        #endregion

        #region Events

        public event ExecuteDelegate Execute;
        public event VoidDelegate Cancel;
        public new event ExecuteDelegate KeyDown;

        #endregion

        #region Properties

        public string Title
        {
            set
            {
                Content = null;
                if (string.IsNullOrEmpty(value))
                {
                    gbTitle.Content = null;
                    Content = pnlParent;
                }
                else
                {
                    gbTitle.Content = pnlParent;
                    Content = gbTitle;
                }
                gbTitle.Header = value;
            }
            get => gbTitle.Header.ToString();
        }

        public double FreeTextboxWidth
        {
            set
            {
                m_tbFromFree.Width = value;
                m_tbToFree.Width = value;
            }
            get => tbHorFreeFrom.Width;
        }

        public Brush BorderColor
        {
            get => gbTitle.BorderBrush;
            set => gbTitle.BorderBrush = value;
        }

        public bool ShowFreeInput
        {
            get
            {
                UIElement pnlFree = null;
                UIElement pnlCalc = null;
                GetPanels(ref pnlFree, ref pnlCalc);
                return pnlFree.Visibility == Visibility.Visible;
            }
            set => SetShowInput(value, ShowCalcInput);
        }

        public bool ShowCalcInput
        {
            get
            {
                UIElement pnlFree = null;
                UIElement pnlCalc = null;
                GetPanels(ref pnlFree, ref pnlCalc);
                return pnlCalc.Visibility == Visibility.Visible;
            }
            set => SetShowInput(ShowFreeInput, value);
        }

        private Selection m_RbSelection;
        private OnlineHelp.OnlineHelpActions m_HelpAction;

        private void SetShowInput(bool showFree, bool showCalc)
        {
            m_RbSelection = (showFree) ? Selection.Free : Selection.Calc;
            UIElement pnlFree = null;
            UIElement pnlCalc = null;
            GetPanels(ref pnlFree, ref pnlCalc);
            pnlCalc.Visibility = (showCalc) ? Visibility.Visible : Visibility.Collapsed;
            pnlFree.Visibility = (showFree) ? Visibility.Visible : Visibility.Collapsed;
            m_InputPnlFree.IsEnabled = showFree;
            m_InputPnlCalc.IsEnabled = !showFree;
            m_RbFree.Visibility = (showCalc) ? Visibility.Visible : Visibility.Collapsed;
            m_RbCalc.Visibility = (showFree) ? Visibility.Visible : Visibility.Collapsed;
        }

        private bool m_SecondParameterPresent;

        public bool SecondParameterPresent
        {
            get => m_SecondParameterPresent;
            set
            {
                m_SecondParameterPresent = value;
                pnlSecondParameter.Visibility = m_SecondParameterPresent ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private InputRangeControlType m_InputRangeControlType;

        public InputRangeControlType InputRangeControlType
        {
            get => m_InputRangeControlType;
            set
            {
                m_InputRangeControlType = value;
                m_LblInfoSecond = lblInfoSecond;

                switch (m_InputRangeControlType)
                {
                    case InputRangeControlType.Vertical:
                        pnlHorizontal.Visibility = Visibility.Collapsed;
                        pnlVertical.Visibility = Visibility.Visible;
                        m_RbFree = rbVertFree;
                        m_RbCalc = rbVertCalc;
                        m_LblInfoFree = lblInfoVertFree;
                        m_LblInfoCalc = lblInfoVertCalc;
                        m_tbFromFree = tbVertFreeFrom;
                        m_tbToFree = tbVertFreeTo;
                        m_tbFromCalcFactor = tbVertCalcFromFactor;
                        m_tbFromCalcBase = tbVertCalcFromBase;
                        m_tbFromCalcExp = tbVertCalcFromExp;
                        m_tbFromCalcSum = tbVertCalcFromSum;
                        m_tbToCalcFactor = tbVertCalcToFactor;
                        m_tbToCalcBase = tbVertCalcToBase;
                        m_tbToCalcExp = tbVertCalcToExp;
                        m_tbToCalcSum = tbVertCalcToSum;
                        m_InputPnlFree = pnlVertFree;
                        m_InputPnlCalc = pnlVertCalc;
                        m_PnlFree = pnlInputControlVertFree;
                        m_PnlCalc = pnlInputControlVertCalc;
                        m_HelpImageFree = btnHelpVertFree;
                        m_HelpImageCalc = btnHelpVertCalc;
                        break;

                    case InputRangeControlType.Horizontal:
                        pnlHorizontal.Visibility = Visibility.Visible;
                        pnlVertical.Visibility = Visibility.Collapsed;
                        m_RbFree = rbHorFree;
                        m_RbCalc = rbHorCalc;
                        m_LblInfoFree = lblInfoHorFree;
                        m_LblInfoCalc = lblInfoHorCalc;
                        m_tbFromFree = tbHorFreeFrom;
                        m_tbToFree = tbHorFreeTo;
                        m_HelpImageFree = btnHelpHorFree;
                        m_HelpImageCalc = btnHelpHorCalc;
                        m_InputPnlFree = pnlHorFree;
                        m_InputPnlCalc = pnlHorCalc;
                        m_PnlFree = pnlInputControlHorFree;
                        m_PnlCalc = pnlInputControlHorCalc;
                        m_tbFromCalcFactor = tbHorCalcFromFactor;
                        m_tbFromCalcBase = tbHorCalcFromBase;
                        m_tbFromCalcExp = tbHorCalcFromExp;
                        m_tbFromCalcSum = tbHorCalcFromSum;
                        m_tbToCalcFactor = tbHorCalcToFactor;
                        m_tbToCalcBase = tbHorCalcToBase;
                        m_tbToCalcExp = tbHorCalcToExp;
                        m_tbToCalcSum = tbHorCalcToSum;
                        break;
                }
            }
        }

        private readonly IDictionary<string, InputValidator<PrimesBigInteger>> m_Validators;

        public void AddInputValidator(string key, InputValidator<PrimesBigInteger> validator)
        {
            if (m_Validators.ContainsKey(key))
            {
                m_Validators[key] = validator;
            }
            else
            {
                m_Validators.Add(key, validator);
            }

            SetText(key, validator.DefaultValue, false);
        }

        private readonly IDictionary<string, IValidator<PrimesBigInteger>> m_ValueValidators;

        public void AddValueValidator(string key, IValidator<PrimesBigInteger> validator)
        {
            if (m_ValueValidators.ContainsKey(key))
            {
                m_ValueValidators[key] = validator;
            }
            else
            {
                m_ValueValidators.Add(key, validator);
            }
        }

        private IValidator<PrimesBigInteger> m_RangeValueValidator;

        public IValidator<PrimesBigInteger> RangeValueValidator
        {
            get => m_RangeValueValidator;
            set => m_RangeValueValidator = value;
        }

        public void SetText(string key, string value)
        {
            SetText(key, value, true);
        }

        private void SetText(string key, string value, bool dooverride)
        {
            TextBox target = null;

            switch (key)
            {
                case FreeTo:
                    target = m_tbToFree;
                    break;
                case FreeFrom:
                    target = m_tbFromFree;
                    break;
                case CalcFromFactor:
                    target = m_tbFromCalcFactor;
                    break;
                case CalcFromBase:
                    target = m_tbFromCalcBase;
                    break;
                case CalcFromExp:
                    target = m_tbFromCalcExp;
                    break;
                case CalcFromSum:
                    target = m_tbFromCalcSum;
                    break;
                case CalcToFactor:
                    target = m_tbToCalcFactor;
                    break;
                case CalcToBase:
                    target = m_tbToCalcBase;
                    break;
                case CalcToExp:
                    target = m_tbToCalcExp;
                    break;
                case CalcToSum:
                    target = m_tbToCalcSum;
                    break;
            }

            if (target != null && (string.IsNullOrEmpty(target.Text) || dooverride))
            {
                target.Text = value;
            }
        }

        private readonly IDictionary<string, IList<IValidator<PrimesBigInteger>>> m_SingleAdvisors;

        public void AddSingleAdvisors(string key, IValidator<PrimesBigInteger> advisor)
        {
            if (m_SingleAdvisors.ContainsKey(key))
            {
                m_SingleAdvisors[key].Add(advisor);
            }
            else
            {
                throw new ArgumentException("No such key", key);
            }
        }

        public bool ButtonExecuteIsEnabled
        {
            get => btnExecute.IsEnabled;
            set => btnExecute.IsEnabled = value;
        }

        public bool ButtonCancelIsEnabled
        {
            get => btnCancel.IsEnabled;
            set => btnCancel.IsEnabled = value;
        }

        public bool ShowButtons
        {
            get => pnlButtons.Visibility == Visibility.Visible;
            set
            {
                pnlButtons.Visibility = (value) ? Visibility.Visible : Visibility.Collapsed;
                pnlButtons.Margin = (value) ? new Thickness(0, 7, 0, 0) : new Thickness(0, 0, 0, 0);
            }
        }

        public bool CancelButtonIsEnabled
        {
            get => btnCancel.IsEnabled;
            set => btnCancel.IsEnabled = value;
        }

        public bool ExecuteButtonIsEnabled
        {
            get => btnExecute.IsEnabled;
            set => btnExecute.IsEnabled = value;
        }

        public void LockControls()
        {
            ControlHandler.SetButtonEnabled(btnCancel, true);
            ControlHandler.SetButtonEnabled(btnExecute, false);
            ControlHandler.SetPropertyValue(m_tbFromFree, "IsEnabled", false);
            ControlHandler.SetPropertyValue(m_tbFromCalcFactor, "IsEnabled", false);
            ControlHandler.SetPropertyValue(m_tbFromCalcBase, "IsEnabled", false);
            ControlHandler.SetPropertyValue(m_tbFromCalcExp, "IsEnabled", false);
            ControlHandler.SetPropertyValue(m_tbFromCalcSum, "IsEnabled", false);
            ControlHandler.SetPropertyValue(m_tbToFree, "IsEnabled", false);
            ControlHandler.SetPropertyValue(m_tbToCalcFactor, "IsEnabled", false);
            ControlHandler.SetPropertyValue(m_tbToCalcBase, "IsEnabled", false);
            ControlHandler.SetPropertyValue(m_tbToCalcExp, "IsEnabled", false);
            ControlHandler.SetPropertyValue(m_tbToCalcSum, "IsEnabled", false);
            ControlHandler.SetPropertyValue(tbSecondParameter, "IsEnabled", false);

            ControlHandler.SetPropertyValue(m_RbCalc, "IsEnabled", false);
            ControlHandler.SetPropertyValue(m_RbFree, "IsEnabled", false);
        }

        public void UnLockControls()
        {
            ControlHandler.SetButtonEnabled(btnCancel, false);
            ControlHandler.SetButtonEnabled(btnExecute, true);
            ControlHandler.SetPropertyValue(m_tbFromFree, "IsEnabled", true);
            ControlHandler.SetPropertyValue(m_tbFromCalcFactor, "IsEnabled", true);
            ControlHandler.SetPropertyValue(m_tbFromCalcBase, "IsEnabled", true);
            ControlHandler.SetPropertyValue(m_tbFromCalcExp, "IsEnabled", true);
            ControlHandler.SetPropertyValue(m_tbFromCalcSum, "IsEnabled", true);
            ControlHandler.SetPropertyValue(m_tbToFree, "IsEnabled", true);
            ControlHandler.SetPropertyValue(m_tbToCalcFactor, "IsEnabled", true);
            ControlHandler.SetPropertyValue(m_tbToCalcBase, "IsEnabled", true);
            ControlHandler.SetPropertyValue(m_tbToCalcExp, "IsEnabled", true);
            ControlHandler.SetPropertyValue(m_tbToCalcSum, "IsEnabled", true);
            ControlHandler.SetPropertyValue(tbSecondParameter, "IsEnabled", true);

            ControlHandler.SetPropertyValue(m_RbCalc, "IsEnabled", true);
            ControlHandler.SetPropertyValue(m_RbFree, "IsEnabled", true);
        }

        private bool m_IntervalSizeCanBeZero;

        public bool IntervalSizeCanBeZero
        {
            get => m_IntervalSizeCanBeZero;
            set => m_IntervalSizeCanBeZero = value;
        }

        #endregion

        #region Handle Radio Button Click

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender == m_RbFree)
            {
                m_RbFree.IsChecked = true;
                m_RbCalc.IsChecked = false;
                m_InputPnlFree.IsEnabled = true;
                m_InputPnlCalc.IsEnabled = false;
                m_RbSelection = Selection.Free;
            }
            else
            {
                m_RbFree.IsChecked = false;
                m_RbCalc.IsChecked = true;
                m_InputPnlFree.IsEnabled = false;
                m_InputPnlCalc.IsEnabled = true;
                m_RbSelection = Selection.Calc;
            }
        }

        private void GetPanels(ref UIElement pnlFree, ref UIElement pnlCalc)
        {
            switch (m_InputRangeControlType)
            {
                case InputRangeControlType.Horizontal:
                    pnlFree = pnlInputControlHorFree;
                    pnlCalc = pnlInputControlHorCalc;
                    break;
                case InputRangeControlType.Vertical:
                    pnlFree = pnlInputControlVertFree;
                    pnlCalc = pnlInputControlVertCalc;
                    break;
            }
        }

        #endregion

        #region Buttons

        public bool GetValue(ref PrimesBigInteger from, ref PrimesBigInteger to, ref PrimesBigInteger second)
        {
            if (!GetValue(ref from, ref to))
            {
                return false;
            }

            ValidateSecondInput(ref second);
            return true;
        }

        public bool GetValue(ref PrimesBigInteger from, ref PrimesBigInteger to)
        {
            from = null;
            to = null;

            ResetMessages();

            if (m_RbSelection == Selection.Free)
            {
                ValidateFreeInput(ref from, ref to);
            }
            else if (m_RbSelection == Selection.Calc)
            {
                ValidateCalcInput(ref from, ref to);
            }

            if (from != null && to != null)
            {
                if (m_ValueValidators.ContainsKey(From))
                {
                    IValidator<PrimesBigInteger> validator = m_ValueValidators[From];
                    validator.Value = from;
                    if (validator.Validate(ref from) != Primes.WpfControls.Validation.ValidationResult.OK)
                    {
                        if (m_RbSelection == Selection.Free)
                        {
                            InfoFree(validator.Message, new TextBox[] { m_tbFromFree }, validator.HelpLink);
                        }
                        else if (m_RbSelection == Selection.Calc)
                        {
                            InfoFree(validator.Message, new TextBox[] { m_tbFromCalcFactor, m_tbFromCalcBase, m_tbFromCalcExp, m_tbFromCalcSum }, validator.HelpLink);
                        }
                        from = null;
                        to = null;
                    }
                }

                if (m_ValueValidators.ContainsKey(To))
                {
                    IValidator<PrimesBigInteger> validator = m_ValueValidators[To];
                    validator.Value = to;
                    if (validator.Validate(ref to) != Primes.WpfControls.Validation.ValidationResult.OK)
                    {
                        if (m_RbSelection == Selection.Free)
                        {
                            InfoFree(validator.Message, new TextBox[] { m_tbToFree }, validator.HelpLink);
                        }
                        else if (m_RbSelection == Selection.Calc)
                        {
                            InfoCalc(validator.Message, new TextBox[] { m_tbToCalcFactor, m_tbToCalcBase, m_tbToCalcExp, m_tbToCalcSum }, validator.HelpLink);
                        }
                        from = null;
                        to = null;
                    }
                }

                if (m_RangeValueValidator != null && from != null && to != null)
                {
                    m_RangeValueValidator.Value = to.Subtract(from);
                    PrimesBigInteger range = null;
                    if (m_RangeValueValidator.Validate(ref range) != Primes.WpfControls.Validation.ValidationResult.OK)
                    {
                        if (m_RbSelection == Selection.Free)
                        {
                            InfoFree(m_RangeValueValidator.Message, new TextBox[] { m_tbFromFree, m_tbToFree }, m_RangeValueValidator.HelpLink);
                        }
                        else if (m_RbSelection == Selection.Calc)
                        {
                            InfoCalc(m_RangeValueValidator.Message, new TextBox[] { m_tbFromCalcFactor, m_tbFromCalcBase, m_tbFromCalcExp, m_tbFromCalcSum, m_tbToCalcFactor, m_tbToCalcBase, m_tbToCalcExp, m_tbToCalcSum }, m_RangeValueValidator.HelpLink);
                        }
                        from = null;
                        to = null;
                    }
                }

                foreach (IValidator<PrimesBigInteger> validator in m_SingleAdvisors[From])
                {
                    if (validator.Validate(ref from) != Primes.WpfControls.Validation.ValidationResult.OK)
                    {
                        if (m_RbSelection == Selection.Free)
                        {
                            InfoFree(validator.Message, new TextBox[] { m_tbFromFree }, validator.HelpLink);
                        }
                        else if (m_RbSelection == Selection.Calc)
                        {
                            InfoCalc(validator.Message, new TextBox[] { m_tbFromCalcFactor, m_tbFromCalcBase, m_tbFromCalcExp, m_tbFromCalcSum }, validator.HelpLink);
                        }
                        break;
                    }
                }

                foreach (IValidator<PrimesBigInteger> validator in m_SingleAdvisors[To])
                {
                    if (validator.Validate(ref to) != Primes.WpfControls.Validation.ValidationResult.OK)
                    {
                        if (m_RbSelection == Selection.Free)
                        {
                            InfoFree(validator.Message, new TextBox[] { m_tbToFree }, validator.HelpLink);
                        }
                        else if (m_RbSelection == Selection.Calc)
                        {
                            InfoCalc(validator.Message, new TextBox[] { m_tbToCalcFactor, m_tbToCalcBase, m_tbToCalcExp, m_tbToCalcSum }, validator.HelpLink);
                        }
                        break;
                    }
                }
            }

            bool result = from != null && to != null;
            return result;
        }

        private void btnExecute_Click(object sender, RoutedEventArgs e)
        {
            DoExecute(true);
        }

        private void DoExecute(bool doExecute)
        {
            PrimesBigInteger from = null;
            PrimesBigInteger to = null;
            PrimesBigInteger second = null;

            GetValue(ref from, ref to);
            if (to == null)
            {
                to = from;
            }

            if (Execute == null || !doExecute)
            {
                return;
            }

            if (from == null || to == null)
            {
                return;
            }

            if (SecondParameterPresent && pnlSecondParameter.IsEnabled)
            {
                ValidateSecondInput(ref second);
            }

            LockControls();
            Execute(from, to, second);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (Cancel != null)
            {
                Cancel();
            }
        }

        public void SetButtonExecuteButtonEnabled(bool isEnabled)
        {
            ControlHandler.SetButtonEnabled(btnExecute, isEnabled);
        }

        public void SetButtonCancelButtonEnabled(bool isEnabled)
        {
            ControlHandler.SetButtonEnabled(btnCancel, isEnabled);
        }

        #endregion

        #region Validation of Free Input

        public void ValidateFreeInput(ref PrimesBigInteger from, ref PrimesBigInteger to)
        {
            try
            {
                from = ValidateInput(m_tbFromFree);
                to = ValidateInput(m_tbToFree);

                if (m_IntervalSizeCanBeZero)
                {
                    if (!(from <= to))
                    {
                        from = null;
                        to = null;
                        InfoFree(Primes.Resources.lang.Validation.Validation.IllegalRangeValidator2, new TextBox[] { m_tbFromFree, m_tbToFree }, OnlineHelp.OnlineHelpActions.None);
                    }
                }
                else
                {
                    if (!(from < to))
                    {
                        from = null;
                        to = null;
                        InfoFree(Primes.Resources.lang.Validation.Validation.IllegalRangeValidator, new TextBox[] { m_tbFromFree, m_tbToFree }, OnlineHelp.OnlineHelpActions.None);
                    }
                }
            }
            catch (ControlValidationException cvex)
            {
                switch (cvex.ValidationResult)
                {
                    case Primes.WpfControls.Validation.ValidationResult.ERROR:
                        ErrorFree(cvex.Message, cvex.Control as TextBox, cvex.HelpAction);
                        break;
                    case Primes.WpfControls.Validation.ValidationResult.WARNING:
                        InfoFree(cvex.Message, cvex.Control as TextBox, cvex.HelpAction);
                        break;
                }
            }
        }

        public bool ValidateSecondInput(ref PrimesBigInteger second)
        {
            try
            {
                second = ValidateInput(tbSecondParameter);
                return true;
            }
            catch (ControlValidationException cvex)
            {
                switch (cvex.ValidationResult)
                {
                    case Primes.WpfControls.Validation.ValidationResult.ERROR:
                        ErrorSecond(cvex.Message, cvex.Control as TextBox, cvex.HelpAction);
                        break;
                    case Primes.WpfControls.Validation.ValidationResult.WARNING:
                        InfoSecond(cvex.Message, cvex.Control as TextBox, cvex.HelpAction);
                        break;
                }
                second = null;
                return false;
            }
        }

        public PrimesBigInteger ValidateInput(TextBox tbSource)
        {
            PrimesBigInteger result = PrimesBigInteger.Zero;
            InputValidator<PrimesBigInteger> m_Validator;

            if (m_Validators.ContainsKey(tbSource.Tag.ToString()))
            {
                m_Validator = m_Validators[tbSource.Tag.ToString()];
            }
            else
            {
                m_Validator = new InputValidator<PrimesBigInteger>
                {
                    Validator = new BigIntegerValidator(tbSource.Text)
                };
            }

            try
            {
                TextBoxValidator<PrimesBigInteger> tbvalidator = new TextBoxValidator<PrimesBigInteger>(m_Validator.Validator, tbSource, m_Validator.DefaultValue);
                tbvalidator.Validate(ref result);
            }
            catch (ControlValidationException cvex)
            {
                cvex.HelpAction = m_Validator.LinkOnlinehelp;
                throw cvex;
            }

            return result;
        }

        #endregion

        #region Validation of Calculate Input

        public void ValidateCalcInput(ref PrimesBigInteger from, ref PrimesBigInteger to)
        {
            try
            {
                PrimesBigInteger fromFactor = ValidateInput(m_tbFromCalcFactor);
                PrimesBigInteger fromBase = ValidateInput(m_tbFromCalcBase);
                PrimesBigInteger fromExp = ValidateInput(m_tbFromCalcExp);
                PrimesBigInteger fromSum = ValidateInput(m_tbFromCalcSum);
                PrimesBigInteger toFactor = ValidateInput(m_tbToCalcFactor);
                PrimesBigInteger toBase = ValidateInput(m_tbToCalcBase);
                PrimesBigInteger toExp = ValidateInput(m_tbToCalcExp);
                PrimesBigInteger toSum = ValidateInput(m_tbToCalcSum);

                from = fromBase.Pow(fromExp.IntValue).Multiply(fromFactor).Add(fromSum);
                to = toBase.Pow(toExp.IntValue).Multiply(toFactor).Add(toSum);

                if (m_IntervalSizeCanBeZero)
                {
                    if (!(from <= to))
                    {
                        from = null;
                        to = null;
                        InfoCalc(Primes.Resources.lang.Validation.Validation.IllegalRangeValidator2, new TextBox[] { m_tbToCalcFactor, m_tbToCalcBase, m_tbToCalcExp, m_tbToCalcSum, m_tbFromCalcFactor, m_tbFromCalcBase, m_tbFromCalcExp, m_tbFromCalcSum }, OnlineHelp.OnlineHelpActions.None);
                    }
                }
                else
                {
                    if (!(from < to))
                    {
                        from = null;
                        to = null;
                        InfoCalc(Primes.Resources.lang.Validation.Validation.IllegalRangeValidator, new TextBox[] { m_tbToCalcFactor, m_tbToCalcBase, m_tbToCalcExp, m_tbToCalcSum, m_tbFromCalcFactor, m_tbFromCalcBase, m_tbFromCalcExp, m_tbFromCalcSum }, OnlineHelp.OnlineHelpActions.None);
                    }
                }
            }
            catch (ControlValidationException cvex)
            {
                switch (cvex.ValidationResult)
                {
                    case Primes.WpfControls.Validation.ValidationResult.ERROR:
                        ErrorCalc(cvex.Message, cvex.Control as TextBox, cvex.HelpAction);
                        break;
                    case Primes.WpfControls.Validation.ValidationResult.WARNING:
                        InfoCalc(cvex.Message, cvex.Control as TextBox, cvex.HelpAction);
                        break;
                }
            }
        }

        #endregion

        #region Show Info-Error

        public void InfoFree(string message, TextBox tbSource, OnlineHelp.OnlineHelpActions helplink)
        {
            InfoFree(message, new TextBox[] { tbSource }, helplink);
        }

        public void InfoCalc(string message, TextBox tbSource, OnlineHelp.OnlineHelpActions helplink)
        {
            InfoCalc(message, new TextBox[] { tbSource }, helplink);
        }

        public void InfoSecond(string message, TextBox tbSource, OnlineHelp.OnlineHelpActions helplink)
        {
            InfoSecond(message, new TextBox[] { tbSource }, helplink);
        }

        public void InfoFree(string message, TextBox[] tbSource, OnlineHelp.OnlineHelpActions helplink)
        {
            m_HelpAction = helplink;
            Info(message, m_LblInfoFree, tbSource, (helplink == OnlineHelp.OnlineHelpActions.None) ? null : m_HelpImageFree);
        }

        public void InfoCalc(string message, TextBox[] tbSource, OnlineHelp.OnlineHelpActions helplink)
        {
            m_HelpAction = helplink;
            Info(message, m_LblInfoCalc, tbSource, (helplink == OnlineHelp.OnlineHelpActions.None) ? null : m_HelpImageCalc);
        }

        public void InfoSecond(string message, TextBox[] tbSource, OnlineHelp.OnlineHelpActions helplink)
        {
            m_HelpAction = helplink;
            Info(message, m_LblInfoSecond, tbSource, null);
        }

        public void Info(string message, TextBlock target, TextBox[] tbSource, Image helpImage)
        {
            Warning(message, target, tbSource, helpImage, Brushes.Blue, Brushes.Black, Brushes.LightBlue);
        }

        public void ErrorFree(string message, TextBox tbSource, OnlineHelp.OnlineHelpActions helplink)
        {
            ErrorFree(message, new TextBox[] { tbSource }, helplink);
        }

        public void ErrorCalc(string message, TextBox tbSource, OnlineHelp.OnlineHelpActions helplink)
        {
            ErrorCalc(message, new TextBox[] { tbSource }, helplink);
        }

        public void ErrorSecond(string message, TextBox tbSource, OnlineHelp.OnlineHelpActions helplink)
        {
            ErrorSecond(message, new TextBox[] { tbSource }, helplink);
        }

        public void ErrorFree(string message, TextBox[] tbSource, OnlineHelp.OnlineHelpActions helplink)
        {
            m_HelpAction = helplink;
            Error(message, m_LblInfoFree, tbSource, (helplink == OnlineHelp.OnlineHelpActions.None) ? null : m_HelpImageFree);
        }

        public void ErrorCalc(string message, TextBox[] tbSource, OnlineHelp.OnlineHelpActions helplink)
        {
            m_HelpAction = helplink;
            Error(message, m_LblInfoCalc, tbSource, (helplink == OnlineHelp.OnlineHelpActions.None) ? null : m_HelpImageCalc);
        }

        public void ErrorSecond(string message, TextBox[] tbSource, OnlineHelp.OnlineHelpActions helplink)
        {
            m_HelpAction = helplink;
            Error(message, m_LblInfoSecond, tbSource, null);
        }

        public void Error(string message, TextBlock target, TextBox[] tbSource, Image helpImage)
        {
            Warning(message, target, tbSource, helpImage, Brushes.Red, Brushes.WhiteSmoke, Brushes.Red);
        }

        public void Warning(string message, TextBlock target, TextBox[] tbSource, Image helpImage, Brush textColor, Brush textboxForegorund, Brush textBoxBackground)
        {
            if (string.IsNullOrEmpty(message))
            {
                ResetMessages(target, tbSource);
                return;
            }

            foreach (TextBox tb in tbSource)
            {
                tb.Background = textBoxBackground;
                tb.Foreground = textboxForegorund;
                target.Foreground = textColor;
                target.Text = message;
                target.Visibility = Visibility.Visible;
                if (helpImage != null)
                {
                    helpImage.Visibility = Visibility.Visible;
                }
            }
        }

        public void ResetMessages(TextBlock target, TextBox[] tbSource)
        {
            foreach (TextBox tb in tbSource)
            {
                tb.Background = Brushes.White;
                tb.Foreground = Brushes.Black;
                target.Text = string.Empty;
                target.Visibility = Visibility.Collapsed;
                btnHelpHorCalc.Visibility = Visibility.Collapsed;
                btnHelpVertCalc.Visibility = Visibility.Collapsed;
                btnHelpHorFree.Visibility = Visibility.Collapsed;
                btnHelpVertFree.Visibility = Visibility.Collapsed;
            }
        }

        private void ResetMessages()
        {
            ResetMessages(m_LblInfoFree, new TextBox[] { m_tbFromFree, m_tbToFree });
            ResetMessages(m_LblInfoCalc, new TextBox[] { m_tbToCalcFactor, m_tbToCalcBase, m_tbToCalcExp, m_tbToCalcSum, m_tbFromCalcFactor, m_tbFromCalcBase, m_tbFromCalcExp, m_tbFromCalcSum });
            ResetMessages(m_LblInfoSecond, new TextBox[] { tbSecondParameter });
        }

        #endregion

        private void Help_Click(object sender, MouseButtonEventArgs e)
        {
            OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(m_HelpAction);
        }

        private void tb_KeyDown(object sender, KeyEventArgs e)
        {
            PrimesBigInteger from = null;
            PrimesBigInteger to = null;
            PrimesBigInteger second = null;

            GetValue(ref from, ref to);

            if (from != null && to != null)
            {
                if (KeyDown != null)
                {
                    KeyDown(from, to, second);
                }
            }

            DoExecute(e.Key == Key.Enter);
        }

        private void tb_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender != null && sender.GetType() == typeof(TextBox))
            {
                (sender as TextBox).SelectAll();
            }
        }
    }
}