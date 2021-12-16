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
    public delegate void ExecuteSingleDelegate(PrimesBigInteger value);
    public partial class InputSingleControl : UserControl
    {
        #region Contants

        public const string Free = "Free";
        public const string Value = "Value";

        public const string CalcFactor = "CalcFromFactor";
        public const string CalcBase = "CalcBase";
        public const string CalcExp = "CalcExp";
        public const string CalcSum = "CalcSum";

        #endregion

        private enum Selection { Free, Calc }

        public InputSingleControl()
        {
            try
            {
                InitializeComponent();
                m_Validators = new Dictionary<string, InputValidator<PrimesBigInteger>>();
                m_SingleAdvisors = new Dictionary<string, IList<IValidator<PrimesBigInteger>>>
                {
                    { Value, new List<IValidator<PrimesBigInteger>>() }
                };
                m_ValueValidators = new Dictionary<string, IValidator<PrimesBigInteger>>();
                Title = "";
                generateNumberControlVertFree.OnRandomNumberGenerated += new GmpBigIntegerParameterDelegate(generateNumberControlVertFree_OnRandomNumberGenerated);
                m_ShowInfoErrorText = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.EventLog.WriteEntry("Primes.InputSingleControl", ex.Message, System.Diagnostics.EventLogEntryType.Information);
                System.Diagnostics.EventLog.WriteEntry("Primes.InputSingleControl", ex.StackTrace, System.Diagnostics.EventLogEntryType.Information);
                if (ex.InnerException != null)
                {
                    System.Diagnostics.EventLog.WriteEntry("Primes.InputSingleControl", ex.InnerException.Message, System.Diagnostics.EventLogEntryType.Information);
                    System.Diagnostics.EventLog.WriteEntry("Primes.InputSingleControl", ex.InnerException.StackTrace, System.Diagnostics.EventLogEntryType.Information);
                }
            }
        }

        private void generateNumberControlVertFree_OnRandomNumberGenerated(PrimesBigInteger value)
        {
            tbVertFree.Text = value.ToString();
            GetValue();
            //ValidateFreeInput(ref value);
        }

        #region Components

        private RadioButton m_RbFree;
        private RadioButton m_RbCalc;
        private TextBlock m_LblInfoFree;
        private TextBlock m_LblInfoCalc;
        // Textboxes Free
        private TextBox m_tbFree;
        // Textboxes Calc
        private TextBox m_tbCalcFactor;
        private TextBox m_tbCalcBase;
        private TextBox m_tbCalcExp;
        private TextBox m_tbCalcSum;

        private Image m_HelpImageFree;
        private Image m_HelpImageCalc;

        private Panel m_InputPnlFree;
        private Panel m_InputPnlCalc;
        private Panel m_PnlFree;
        private Panel m_PnlCalc;

        #endregion

        #region Events

        public event ExecuteSingleDelegate Execute;
        public new event ExecuteSingleDelegate KeyDown;
        public event MessageDelegate KeyDownNoValidation;
        public event MessageDelegate OnInfoError;
        public event VoidDelegate Cancel;

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
                    if (gbTitle != null)
                    {
                        gbTitle.Content = pnlParent;
                        Content = gbTitle;
                    }
                }
                if (gbTitle != null)
                {
                    gbTitle.Header = value;
                }
            }
            get => gbTitle.Header.ToString();
        }

        public double FreeTextboxWidth
        {
            set => m_tbFree.Width = value;
            get => m_tbFree.Width;
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

        private InputRangeControlType m_InputRangeControlType;

        public InputRangeControlType InputRangeControlType
        {
            get => m_InputRangeControlType;
            set
            {
                m_InputRangeControlType = value;
                switch (m_InputRangeControlType)
                {
                    case InputRangeControlType.Vertical:
                        pnlHorizontal.Visibility = Visibility.Collapsed;
                        pnlVertical.Visibility = Visibility.Visible;
                        m_RbFree = rbVertFree;
                        m_RbCalc = rbVertCalc;
                        m_LblInfoFree = lblInfoVertFree;
                        m_LblInfoCalc = lblInfoVertCalc;
                        m_tbFree = tbVertFree;
                        m_tbCalcFactor = tbVertCalcFactor;
                        m_tbCalcBase = tbVertCalcBase;
                        m_tbCalcExp = tbVertCalcExp;
                        m_tbCalcSum = tbVertCalcSum;

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
                        m_tbFree = tbHorFree;

                        m_HelpImageFree = btnHelpHorFree;
                        m_HelpImageCalc = btnHelpHorCalc;
                        m_InputPnlFree = pnlHorFree;
                        m_InputPnlCalc = pnlHorCalc;
                        m_PnlFree = pnlInputControlHorFree;
                        m_PnlCalc = pnlInputControlHorCalc;

                        m_tbCalcFactor = tbHorCalcFactor;
                        m_tbCalcBase = tbHorCalcBase;
                        m_tbCalcExp = tbHorCalcExp;
                        m_tbCalcSum = tbHorCalcSum;
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

        private readonly IDictionary<string, IList<IValidator<PrimesBigInteger>>> m_SingleAdvisors;

        public void AddSingleAdisors(string key, IValidator<PrimesBigInteger> advisor)
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

        private readonly IDictionary<string, IValidator<PrimesBigInteger>> m_ValueValidators;

        public void SetValueValidator(string key, IValidator<PrimesBigInteger> validator)
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

        public bool ShowButtons
        {
            get => pnlButtons.Visibility == Visibility.Visible;
            set
            {
                pnlButtons.Visibility = (value) ? Visibility.Visible : Visibility.Collapsed;
                pnlButtons.Margin = (value) ? new Thickness(0, 7, 0, 0) : new Thickness(0, 0, 0, 0);
            }
        }

        public string FreeText
        {
            set
            {
                if (m_tbFree != null)
                {
                    m_tbFree.Text = value;
                    if (!m_RbFree.IsChecked.Value)
                    {
                        m_RbFree.IsChecked = true;
                        RadioButton_Click(m_RbFree, null);
                    }
                }
            }
            get => m_tbFree.Text;
        }

        public string CalcFactorText
        {
            set => m_tbCalcFactor.Text = value;
            get => m_tbCalcFactor.Text;
        }

        public string CalcBaseText
        {
            set => m_tbCalcBase.Text = value;
            get => m_tbCalcBase.Text;
        }

        public string CalcExpText
        {
            set => m_tbCalcExp.Text = value;
            get => m_tbCalcExp.Text;
        }

        public string CalcSumText
        {
            set => m_tbCalcSum.Text = value;
            get => m_tbCalcSum.Text;
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

        public bool ShowGenerateRandomNumber
        {
            get => (generateNumberControlVertFree != null) ? generateNumberControlVertFree.Visibility == Visibility.Hidden : false;
            set
            {
                if (generateNumberControlVertFree != null)
                {
                    generateNumberControlVertFree.Visibility = (value) ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        public bool GenerateRandomNumberShowMultipleFactors
        {
            get => generateNumberControlVertFree.ShowMultipleFactors;
            set => generateNumberControlVertFree.ShowMultipleFactors = value;
        }

        public bool GenerateRandomNumberShowTwoBigFactors
        {
            get => generateNumberControlVertFree.ShowTwoBigFactors;
            set => generateNumberControlVertFree.ShowTwoBigFactors = value;
        }

        public string GenerateRandomNumberTitle
        {
            get => (generateNumberControlVertFree != null) ? generateNumberControlVertFree.Title : null;
            set
            {
                if (generateNumberControlVertFree != null)
                {
                    generateNumberControlVertFree.Title = value;
                }
            }
        }

        public bool ShowGenerateRandomNumberHelpLink
        {
            set { }
        }

        public Primes.OnlineHelp.OnlineHelpActions HelpActionGenerateRandomNumber
        {
            get => (generateNumberControlVertFree != null) ? generateNumberControlVertFree.HelpAction : Primes.OnlineHelp.OnlineHelpActions.None;
            set
            {
                if (generateNumberControlVertFree != null)
                {
                    generateNumberControlVertFree.HelpAction = value;
                }
            }
        }

        private bool m_ShowInfoErrorText;

        public bool ShowInfoErrorText
        {
            get => m_ShowInfoErrorText;
            set => m_ShowInfoErrorText = value;
        }

        private bool m_NoMargin;

        public bool NoMargin
        {
            get => m_NoMargin;
            set
            {
                m_NoMargin = value;
                if (m_NoMargin)
                {
                    m_PnlCalc.Margin = new Thickness(0);
                    m_PnlFree.Margin = new Thickness(0);
                    m_HelpImageFree.Margin = new Thickness(0);
                    m_HelpImageFree.Visibility = Visibility.Collapsed;
                    m_HelpImageCalc.Margin = new Thickness(0);
                    m_HelpImageCalc.Visibility = Visibility.Collapsed;
                    m_LblInfoCalc.Margin = new Thickness(0);
                    m_LblInfoFree.Margin = new Thickness(0);
                }
                else
                {
                    m_PnlCalc.Margin = new Thickness(10);
                    m_PnlFree.Margin = new Thickness(10);
                    m_HelpImageFree.Margin = new Thickness(7, 0, 0, 0);
                    m_HelpImageFree.Visibility = Visibility.Hidden;
                    m_HelpImageCalc.Margin = new Thickness(7, 0, 0, 0);
                    m_HelpImageCalc.Visibility = Visibility.Hidden;
                    m_LblInfoCalc.Margin = new Thickness(5, 0, 0, 0);
                    m_LblInfoFree.Margin = new Thickness(5, 0, 0, 0);
                }
            }
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

            if (KeyDown != null)
            {
                KeyDown(null);
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

        private void btnExecute_Click(object sender, RoutedEventArgs e)
        {
            DoExecute(true);
        }

        public PrimesBigInteger GetValue()
        {
            PrimesBigInteger value = null;

            ResetMessages();

            if (m_RbSelection == Selection.Free)
            {
                ValidateFreeInput(ref value);
            }
            else if (m_RbSelection == Selection.Calc)
            {
                ValidateCalcInput(ref value);
            }

            if (value != null)
            {
                SetButtonExecuteButtonEnabled(true);
                if (m_ValueValidators.ContainsKey(Value))
                {
                    IValidator<PrimesBigInteger> validator = m_ValueValidators[Value];
                    validator.Value = value.ToString();
                    Primes.WpfControls.Validation.ValidationResult result = validator.Validate(ref value);
                    if (result != Primes.WpfControls.Validation.ValidationResult.OK)
                    {
                        switch (result)
                        {
                            case Primes.WpfControls.Validation.ValidationResult.WARNING:
                                if (m_RbSelection == Selection.Calc)
                                {
                                    InfoCalc(validator.Message, new TextBox[] { m_tbCalcBase, m_tbCalcExp, m_tbCalcFactor, m_tbCalcSum }, validator.HelpLink);
                                }
                                else
                                {
                                    InfoFree(validator.Message, m_tbFree, validator.HelpLink);
                                }
                                SetButtonExecuteButtonEnabled(false);

                                break;
                            case Primes.WpfControls.Validation.ValidationResult.ERROR:
                                if (m_RbSelection == Selection.Calc)
                                {
                                    ErrorCalc(validator.Message, new TextBox[] { m_tbCalcBase, m_tbCalcExp, m_tbCalcFactor, m_tbCalcSum }, validator.HelpLink);
                                }
                                else
                                {
                                    ErrorFree(validator.Message, m_tbFree, validator.HelpLink);
                                }
                                SetButtonExecuteButtonEnabled(false);
                                break;
                            default:
                                break;
                        }
                        return null;
                    }
                }
                foreach (IValidator<PrimesBigInteger> validator in m_SingleAdvisors[Value])
                {
                    if (validator.Validate(ref value) != Primes.WpfControls.Validation.ValidationResult.OK)
                    {
                        if (m_RbSelection == Selection.Free)
                        {
                            InfoFree(validator.Message, new TextBox[] { m_tbFree }, validator.HelpLink);
                        }
                        else if (m_RbSelection == Selection.Calc)
                        {
                            InfoFree(validator.Message, new TextBox[] { m_tbCalcFactor, m_tbCalcBase, m_tbCalcExp, m_tbCalcSum }, validator.HelpLink);
                        }
                        break;
                    }
                }
            }
            else
            {
                SetButtonExecuteButtonEnabled(false);
            }
            return value;
        }

        private void DoExecute(bool doExecute)
        {
            PrimesBigInteger value = GetValue();

            if (Execute != null && doExecute && value != null)
            {
                LockControls();
                Execute(value);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            UnLockControls();
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

        public void LockControls()
        {
            ControlHandler.SetButtonEnabled(btnCancel, true);
            ControlHandler.SetButtonEnabled(btnExecute, false);
            ControlHandler.SetPropertyValue(m_tbFree, "IsEnabled", false);
            ControlHandler.SetPropertyValue(m_tbCalcFactor, "IsEnabled", false);
            ControlHandler.SetPropertyValue(m_tbCalcBase, "IsEnabled", false);
            ControlHandler.SetPropertyValue(m_tbCalcExp, "IsEnabled", false);
            ControlHandler.SetPropertyValue(m_tbCalcSum, "IsEnabled", false);
            ControlHandler.SetPropertyValue(m_RbCalc, "IsEnabled", false);
            ControlHandler.SetPropertyValue(m_RbFree, "IsEnabled", false);
            ControlHandler.SetPropertyValue(generateNumberControlVertFree, "IsEnabled", false);
        }

        public void UnLockControls()
        {
            ControlHandler.SetButtonEnabled(btnCancel, false);
            ControlHandler.SetButtonEnabled(btnExecute, true);
            ControlHandler.SetPropertyValue(m_tbFree, "IsEnabled", true);
            ControlHandler.SetPropertyValue(m_tbCalcFactor, "IsEnabled", true);
            ControlHandler.SetPropertyValue(m_tbCalcBase, "IsEnabled", true);
            ControlHandler.SetPropertyValue(m_tbCalcExp, "IsEnabled", true);
            ControlHandler.SetPropertyValue(m_tbCalcSum, "IsEnabled", true);
            ControlHandler.SetPropertyValue(m_RbCalc, "IsEnabled", true);
            ControlHandler.SetPropertyValue(m_RbFree, "IsEnabled", true);
            ControlHandler.SetPropertyValue(generateNumberControlVertFree, "IsEnabled", true);
        }

        #endregion

        #region Validation of Free Input

        public void ValidateFreeInput(ref PrimesBigInteger value)
        {
            try
            {
                value = ValidateInput(m_tbFree);
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

        public void ValidateCalcInput(ref PrimesBigInteger value)
        {
            try
            {
                PrimesBigInteger factor = ValidateInput(m_tbCalcFactor);
                PrimesBigInteger base_ = ValidateInput(m_tbCalcBase);
                PrimesBigInteger exp = ValidateInput(m_tbCalcExp);
                PrimesBigInteger sum = ValidateInput(m_tbCalcSum);
                value = base_.Pow(exp.IntValue).Multiply(factor).Add(sum);
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

            if (OnInfoError != null)
            {
                OnInfoError(message);
            }

            foreach (TextBox tb in tbSource)
            {
                tb.Background = textBoxBackground;
                tb.Foreground = textboxForegorund;
                target.Foreground = textColor;
                if (m_ShowInfoErrorText)
                {
                    target.Text = message;
                    target.Visibility = Visibility.Visible;
                }
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
                target.Visibility = Visibility.Hidden;
                btnHelpHorCalc.Visibility = Visibility.Collapsed;
                btnHelpVertCalc.Visibility = Visibility.Collapsed;
                btnHelpHorFree.Visibility = Visibility.Collapsed;
                btnHelpVertFree.Visibility = Visibility.Collapsed;
            }
        }

        public void ResetMessages()
        {
            ResetMessages(m_LblInfoFree, new TextBox[] { m_tbFree });
            ResetMessages(m_LblInfoCalc, new TextBox[] { m_tbCalcFactor, m_tbCalcBase, m_tbCalcExp, m_tbCalcSum });
        }

        #endregion

        #region Setting Text

        public void SetText(string key, string value)
        {
            SetText(key, value, true);
        }

        private void SetText(string key, string value, bool dooverride)
        {
            TextBox target = null;

            switch (key)
            {
                case Free:
                    target = m_tbFree;
                    break;
                case CalcFactor:
                    target = m_tbCalcFactor;
                    break;
                case CalcBase:
                    target = m_tbCalcBase;
                    break;
                case CalcExp:
                    target = m_tbCalcExp;
                    break;
                case CalcSum:
                    target = m_tbCalcSum;
                    break;
            }

            if (target != null && dooverride)
            {
                target.Text = value;
            }
        }

        #endregion

        private void Help_Click(object sender, MouseButtonEventArgs e)
        {
            OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(m_HelpAction);
        }

        private void tb_KeyDown(object sender, KeyEventArgs e)
        {
            if (KeyDownNoValidation != null)
            {
                KeyDownNoValidation((sender as TextBox).Text);
            }

            PrimesBigInteger value = GetValue();
            if (value != null && KeyDown != null)
            {
                KeyDown(value);
            }
            if (Execute != null && e.Key == Key.Enter && value != null)
            {
                Execute(value);
            }
        }

        private void tb_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender != null && sender.GetType() == typeof(TextBox))
            {
                (sender as TextBox).SelectAll();
            }
        }

        public void SetBorderBrush(Brush b)
        {
            m_tbFree.BorderBrush = b;
        }
    }
}
