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
using Primes.Library.FactorTree;
using Primes.WpfControls.Components;
using Primes.WpfControls.Factorization.QS;
using Primes.WpfControls.Validation;
using Primes.WpfControls.Validation.Validator;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace Primes.WpfControls.Factorization
{
    /// <summary>
    /// Interaction logic for FactorizationGraph.xaml
    /// </summary>
    public partial class FactorizationControl : UserControl, IPrimeMethodDivision
    {
        private PrimesBigInteger m_Integer;

        private IFactorizer _rho;
        private IFactorizer _bruteforce;
        private IFactorizer _qs;

        private struct InputValues
        {
            public string FactorizationInfo;
            public string factors;
            public string lblInput;
            public string FreeText;
            public string CalcFactorText;
            public string CalcBaseText;
            public string CalcExpText;
            public string CalcSumText;
        };

        private InputValues QS, BF;

        public FactorizationControl()
        {
            InitializeComponent();

            QS.FactorizationInfo = Primes.Resources.lang.WpfControls.Factorization.Factorization.fac_result;
            QS.factors = "";
            QS.lblInput = "";
            QS.FreeText = "100";
            QS.CalcFactorText = "2";
            QS.CalcBaseText = "13";
            QS.CalcExpText = "2";
            QS.CalcSumText = "7";

            BF.FactorizationInfo = Primes.Resources.lang.WpfControls.Factorization.Factorization.fac_result;
            BF.factors = "";
            BF.lblInput = "";
            BF.FreeText = "100";
            BF.CalcFactorText = "2";
            BF.CalcBaseText = "13";
            BF.CalcExpText = "2";
            BF.CalcSumText = "7";

            tabItemBruteForce.OnTabContentChanged += content =>
            {
                OnFactorizationStop_BF();
                _bruteforce = (IFactorizer)content;
                _bruteforce.Start += OnFactorizationStart;
                _bruteforce.Stop += OnFactorizationStop_BF;
                _bruteforce.FoundFactor += OnFoundFactor;
                _bruteforce.Cancel += new VoidDelegate(_bruteforce_Cancel);
                _bruteforce.ForceGetInteger += new CallbackDelegateGetInteger(_rho_ForceGetValue);
            };

            tabItemRho.OnTabContentChanged += content =>
            {
                OnFactorizationStop();
                _rho = (IFactorizer)content;
                _rho.Start += OnFactorizationStart;
                _rho.Stop += OnFactorizationStop;
                _rho.FoundFactor += OnFoundFactor;
                _rho.Cancel += new VoidDelegate(_rho_Cancel);
                _rho.ForceGetInteger += new CallbackDelegateGetInteger(_rho_ForceGetValue);
            };

            tabItemQS.OnTabContentChanged += content =>
            {
                OnFactorizationStop_QS();
                _qs = (IFactorizer)content;
                _qs.Start += OnFactorizationStart;
                _qs.Stop += OnFactorizationStop_QS;
                _qs.FoundFactor += OnFoundFactor;
                _qs.Cancel += new VoidDelegate(_qs_Cancel);
            };

            inputnumbermanager.Execute += new ExecuteSingleDelegate(InputSingleNumberControl_Execute);
            inputnumbermanager.Cancel += new VoidDelegate(InputSingleNumberControl_Cancel);
            inputnumbermanager.HelpActionGenerateRandomNumber = Primes.OnlineHelp.OnlineHelpActions.Factorization_Generate;
            inputnumbermanager.generateNumberControlVertFree.OnRandomNumberGenerated += new GmpBigIntegerParameterDelegate(ResetMessages);
            inputnumbermanager.KeyDown += new ExecuteSingleDelegate(inputnumbermanager_ValueChanged);

            SetInputValidators();

            UpdateMessages();
        }

        private void SetInputValidators()
        {
            InputValidator<PrimesBigInteger> ivExp = new InputValidator<PrimesBigInteger>
            {
                Validator = new BigIntegerMinValueMaxValueValidator(null, PrimesBigInteger.One, PrimesBigInteger.OneHundred)
            };
            inputnumbermanager.AddInputValidator(InputSingleControl.CalcExp, ivExp);
        }

        private void _rho_ForceGetValue(ExecuteIntegerDelegate del)
        {
            PrimesBigInteger value = inputnumbermanager.GetValue();
            if (value != null && del != null)
            {
                del(value);
            }
        }

        private void Factorize(PrimesBigInteger value)
        {
            ClearInfoPanel();
            string inputvalue = m_Integer.ToString();
            if (lblInput.ToolTip == null)
            {
                lblInput.ToolTip = new ToolTip();
            }
            (lblInput.ToolTip as ToolTip).Content = StringFormat.FormatString(inputvalue, 80);

            if (inputvalue.Length > 7)
            {
                inputvalue = inputvalue.Substring(0, 6) + "...";
            }

            System.Windows.Documents.Underline ul = new Underline();

            if (CurrentFactorizer == _bruteforce)
            {
                BF.lblInput = inputvalue;
            }
            else
            {
                QS.lblInput = inputvalue;
            }

            UpdateMessages();

            CurrentFactorizer.Execute(value);
        }

        public void OnFactorizationStart()
        {
            if (CurrentFactorizer == _bruteforce)
            {
                BF.FactorizationInfo = Primes.Resources.lang.WpfControls.Factorization.Factorization.fac_resultrunning;
                BF.factors = "";
            }
            else
            {
                QS.FactorizationInfo = Primes.Resources.lang.WpfControls.Factorization.Factorization.fac_resultrunning;
                QS.factors = "";
            }

            UpdateMessages();

            inputnumbermanager.LockControls();
        }

        public void OnFactorizationStop_BF()
        {
            if (m_Integer != null)
            {
                BF.FactorizationInfo = string.Format(Primes.Resources.lang.WpfControls.Factorization.Factorization.fac_resultfinishedtime, TimeString(_bruteforce.Needs));
                UpdateMessages();
            }

            inputnumbermanager.UnLockControls();
        }

        public void OnFactorizationStop_QS()
        {
            if (m_Integer != null)
            {
                QS.FactorizationInfo = Primes.Resources.lang.WpfControls.Factorization.Factorization.fac_resultfinished;
                UpdateMessages();
            }

            inputnumbermanager.UnLockControls();
        }

        public void OnFactorizationStop()
        {
            if (m_Integer != null)
            {
                if (CurrentFactorizer == _bruteforce)
                {
                    BF.FactorizationInfo = string.Format(Primes.Resources.lang.WpfControls.Factorization.Factorization.fac_resultfinishedtime, TimeString(_bruteforce.Needs));
                }
                else
                {
                    QS.FactorizationInfo = Primes.Resources.lang.WpfControls.Factorization.Factorization.fac_resultfinished;
                }

                UpdateMessages();
            }

            inputnumbermanager.UnLockControls();
        }

        public void OnFactorizationCancel()
        {
            CurrentFactorizer.CancelFactorization();

            if (CurrentFactorizer == _bruteforce)
            {
                BF.FactorizationInfo = string.Format(Primes.Resources.lang.WpfControls.Factorization.Factorization.fac_resultabortedtime, TimeString(_bruteforce.Needs));
            }
            else
            {
                QS.FactorizationInfo = Primes.Resources.lang.WpfControls.Factorization.Factorization.fac_resultaborted;
            }

            UpdateMessages();
        }

        #region IPrimeUserControl Members

        public void Dispose()
        {
            if (_bruteforce?.isRunning ?? false)
            {
                _bruteforce.CancelExecute();
                _bruteforce.CancelFactorization();
            }
            if (_rho?.isRunning ?? false)
            {
                _rho.CancelExecute();
                _rho.CancelFactorization();
            }
            if (_qs?.isRunning ?? false)
            {
                _qs.CancelExecute();
                _qs.CancelFactorization();
            }
        }

        #endregion

        public void OnFoundFactor(object o)
        {
            if (o is GmpFactorTree)
            {
                BF.factors = OnFoundFactor_FactorTree(o as GmpFactorTree);
            }
            else if (o is IEnumerator<KeyValuePair<PrimesBigInteger, PrimesBigInteger>>)
            {
                QS.factors = OnFoundFactor_Enumerator(o as IEnumerator<KeyValuePair<PrimesBigInteger, PrimesBigInteger>>);
            }

            UpdateMessages();
        }

        public string OnFoundFactor_FactorTree(GmpFactorTree ft)
        {
            StringBuilder sbFactors = new StringBuilder();

            if (ft != null)
            {
                sbFactors.Append(" = ");

                foreach (string factor in ft.Factors)
                {
                    sbFactors.Append(factor.ToString());
                    PrimesBigInteger factorcount = ft.GetFactorCount(factor);
                    if (factorcount > 1)
                    {
                        sbFactors.AppendFormat("^{0}", factorcount.ToString());
                    }

                    sbFactors.Append(" * ");
                }

                if (ft.Remainder != null)
                {
                    sbFactors.Append(ft.Remainder.ToString());
                }
                else
                {
                    sbFactors = sbFactors.Remove(sbFactors.Length - 2, 2);
                }
            }

            return sbFactors.ToString();
        }

        public string OnFoundFactor_Enumerator(IEnumerator<KeyValuePair<PrimesBigInteger, PrimesBigInteger>> _enum)
        {
            StringBuilder sbFactors = new StringBuilder();

            sbFactors.Append(" = ");

            while (_enum.MoveNext())
            {
                KeyValuePair<PrimesBigInteger, PrimesBigInteger> current = _enum.Current;
                sbFactors.Append(current.Key.ToString());
                PrimesBigInteger factorcount = current.Value;
                if (factorcount > 1)
                {
                    sbFactors.AppendFormat("^{0}", factorcount.ToString());
                }

                sbFactors.Append(" * ");
            }

            sbFactors = sbFactors.Remove(sbFactors.Length - 2, 2);

            return sbFactors.ToString();
        }

        private void InputSingleNumberControl_Execute(PrimesBigInteger integer)
        {
            m_Integer = integer;
            Factorize(integer);
        }

        private void InputSingleNumberControl_Cancel()
        {
            OnFactorizationCancel();
        }

        private string TimeString(TimeSpan t)
        {
            StringBuilder result = new StringBuilder();

            if (t.Hours > 0)
            {
                result.AppendFormat("{0} {1}, ", t.Hours, (t.Hours == 1) ? Primes.Resources.lang.WpfControls.Factorization.Factorization.fac_timehour : Primes.Resources.lang.WpfControls.Factorization.Factorization.fac_timehours);
            }

            if (t.Hours > 0 || t.Minutes > 0)
            {
                result.AppendFormat("{0} {1}, ", t.Minutes, (t.Minutes == 1) ? Primes.Resources.lang.WpfControls.Factorization.Factorization.fac_timeminute : Primes.Resources.lang.WpfControls.Factorization.Factorization.fac_timeminutes);
            }

            result.AppendFormat("{0}.{1:D3} {2}", t.Seconds, t.Milliseconds, Primes.Resources.lang.WpfControls.Factorization.Factorization.fac_timeseconds);

            return result.ToString();
        }

        private void _bruteforce_Cancel()
        {
            BF.FactorizationInfo = string.Format(Primes.Resources.lang.WpfControls.Factorization.Factorization.fac_resultabortedtime, TimeString(_bruteforce.Needs));
        }

        private void _qs_Cancel()
        {
            QS.FactorizationInfo = Primes.Resources.lang.WpfControls.Factorization.Factorization.fac_resultaborted;
        }

        private void _rho_Cancel()
        {
        }

        private void lblInput_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            if (lblInput.ContextMenu != null)
            {
                lblInput.ContextMenu.IsOpen = true;
            }
        }

        private void MenuItemCopyInputClick(object sender, RoutedEventArgs e)
        {
            if (m_Integer != null)
            {
                Clipboard.SetText(m_Integer.ToString(), TextDataFormat.Text);
            }
        }

        private void lblInputMouseMove(object sender, MouseEventArgs e)
        {
        }

        private void lblInputMouseLeave(object sender, MouseEventArgs e)
        {
        }

        private void inputnumbermanager_ValueChanged(PrimesBigInteger value)
        {
            switch (KindOfFactorization)
            {
                case KOF.BruteForce:
                    BF.FactorizationInfo = Primes.Resources.lang.WpfControls.Factorization.Factorization.fac_result;
                    BF.factors = "";
                    BF.lblInput = "";
                    BF.FreeText = inputnumbermanager.FreeText;
                    BF.CalcFactorText = inputnumbermanager.CalcFactorText;
                    BF.CalcBaseText = inputnumbermanager.CalcBaseText;
                    BF.CalcExpText = inputnumbermanager.CalcExpText;
                    BF.CalcSumText = inputnumbermanager.CalcSumText;
                    ((FactorizationGraph)_bruteforce).Reset();
                    break;

                case KOF.QS:
                    QS.FactorizationInfo = Primes.Resources.lang.WpfControls.Factorization.Factorization.fac_result;
                    QS.factors = "";
                    QS.lblInput = "";
                    QS.FreeText = inputnumbermanager.FreeText;
                    QS.CalcFactorText = inputnumbermanager.CalcFactorText;
                    QS.CalcBaseText = inputnumbermanager.CalcBaseText;
                    QS.CalcExpText = inputnumbermanager.CalcExpText;
                    QS.CalcSumText = inputnumbermanager.CalcSumText;
                    ((QS.QuadraticSieveControl)_qs).Reset();
                    break;
            }

            UpdateMessages();
        }

        private void ResetMessages()
        {
            switch (KindOfFactorization)
            {
                case KOF.BruteForce:
                    BF.FactorizationInfo = Primes.Resources.lang.WpfControls.Factorization.Factorization.fac_result;
                    BF.factors = "";
                    BF.lblInput = "";
                    ((FactorizationGraph)_bruteforce).Reset();
                    break;

                case KOF.QS:
                    QS.FactorizationInfo = Primes.Resources.lang.WpfControls.Factorization.Factorization.fac_result;
                    QS.factors = "";
                    QS.lblInput = "";
                    ((QS.QuadraticSieveControl)_qs).Reset();
                    break;
            }

            UpdateMessages();
        }

        private void ResetMessages(PrimesBigInteger value)
        {
            switch (KindOfFactorization)
            {
                case KOF.BruteForce:
                    BF.FreeText = value.ToString();
                    break;

                case KOF.QS:
                    QS.FreeText = value.ToString();
                    break;
            }

            ResetMessages();
        }

        private void UpdateMessages()
        {
            switch (KindOfFactorization)
            {
                case KOF.BruteForce:
                    ControlHandler.SetPropertyValue(gbFactorizationInfo, "Header", BF.FactorizationInfo);
                    ControlHandler.SetPropertyValue(lblFactors, "Text", BF.factors);
                    ControlHandler.SetPropertyValue(lblInput, "Text", BF.lblInput);
                    ControlHandler.SetPropertyValue(inputnumbermanager.tbVertFree, "Text", BF.FreeText);
                    ControlHandler.SetPropertyValue(inputnumbermanager.tbVertCalcFactor, "Text", BF.CalcFactorText);
                    ControlHandler.SetPropertyValue(inputnumbermanager.tbVertCalcBase, "Text", BF.CalcBaseText);
                    ControlHandler.SetPropertyValue(inputnumbermanager.tbVertCalcExp, "Text", BF.CalcExpText);
                    ControlHandler.SetPropertyValue(inputnumbermanager.tbVertCalcSum, "Text", BF.CalcSumText);
                    break;

                case KOF.QS:
                    ControlHandler.SetPropertyValue(gbFactorizationInfo, "Header", QS.FactorizationInfo);
                    ControlHandler.SetPropertyValue(lblFactors, "Text", QS.factors);
                    ControlHandler.SetPropertyValue(lblInput, "Text", QS.lblInput);
                    ControlHandler.SetPropertyValue(inputnumbermanager.tbVertFree, "Text", QS.FreeText);
                    ControlHandler.SetPropertyValue(inputnumbermanager.tbVertCalcFactor, "Text", QS.CalcFactorText);
                    ControlHandler.SetPropertyValue(inputnumbermanager.tbVertCalcBase, "Text", QS.CalcBaseText);
                    ControlHandler.SetPropertyValue(inputnumbermanager.tbVertCalcExp, "Text", QS.CalcExpText);
                    ControlHandler.SetPropertyValue(inputnumbermanager.tbVertCalcSum, "Text", QS.CalcSumText);
                    break;
            }
        }

        private KOF KindOfFactorization
        {
            get
            {
                object selecteditem = ControlHandler.GetPropertyValue(tbctrl, "SelectedItem");
                if (selecteditem == tabItemBruteForce)
                {
                    return KOF.BruteForce;
                }
                else if (selecteditem == tabItemRho)
                {
                    return KOF.Rho;
                }
                else if (selecteditem == tabItemQS)
                {
                    return KOF.QS;
                }

                return KOF.None;
            }
        }

        private enum KOF { None, BruteForce, Rho, QS }

        private void tbctrl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (KindOfFactorization)
            {
                case KOF.BruteForce:
                    inputnumbermanager.SetValueValidator(InputSingleControl.Value, _bruteforce.Validator);
                    inputnumbermanager.generateNumberControlVertFree.MaxValue = ((System.Numerics.BigInteger)1) << 100;
                    break;
                case KOF.QS:
                    inputnumbermanager.SetValueValidator(InputSingleControl.Value, _qs.Validator);
                    inputnumbermanager.generateNumberControlVertFree.MaxValue = (_qs as QuadraticSieveControl).MaxValue;
                    break;
            }

            UpdateMessages();

            inputnumbermanager.GetValue();

            if (CurrentFactorizer.isRunning)
            {
                inputnumbermanager.LockControls();
                inputnumbermanager.CancelButtonIsEnabled = true;
                inputnumbermanager.ExecuteButtonIsEnabled = false;
            }
            else if (inputnumbermanager.GetValue() == null)
            {
                inputnumbermanager.UnLockControls();
                inputnumbermanager.CancelButtonIsEnabled = false;
                inputnumbermanager.ExecuteButtonIsEnabled = false;
            }
            else
            {
                inputnumbermanager.UnLockControls();
                inputnumbermanager.CancelButtonIsEnabled = false;
                inputnumbermanager.ExecuteButtonIsEnabled = true;
            }
        }

        private IFactorizer CurrentFactorizer
        {
            get
            {
                switch (KindOfFactorization)
                {
                    case KOF.BruteForce: return _bruteforce;
                    case KOF.Rho: return _rho;
                    case KOF.QS: return _qs;
                    default: return null;
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void ClearInfoPanel()
        {
            lblInput.Text = "";
            lblFactors.Text = "";
        }

        #region IPrimeUserControl Members

        public void SetTab(int i)
        {
            if (i >= 0 && i < tbctrl.Items.Count)
            {
                tbctrl.SelectedIndex = i;
            }

            tbctrl_SelectionChanged(null, null);
        }

        #endregion

        #region IPrimeUserControl Members

        public event VoidDelegate Execute;

        public void FireExecuteEvent()
        {
            if (Execute != null)
            {
                Execute();
            }
        }

        public event VoidDelegate Stop;

        public void FireStopEvent()
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

        private void HelpTabItem_HelpButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender == tabItemBruteForce)
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(Primes.OnlineHelp.OnlineHelpActions.Factorization_BruteForce);
            }
            else if (sender == tabItemRho)
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(Primes.OnlineHelp.OnlineHelpActions.Factorization_Rho);
            }
            else if (sender == tabItemQS)
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(Primes.OnlineHelp.OnlineHelpActions.Factorization_QS);
            }
        }
    }
}
