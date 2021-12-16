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
using Primes.Library.Function;
using Primes.WpfControls.Components;
using Primes.WpfControls.Threads;
using Primes.WpfControls.Validation;
using Primes.WpfControls.Validation.Validator;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Primes.WpfControls.PrimesDistribution.Graph
{
    /// <summary>
    /// Interaction logic for GraphControl.xaml
    /// </summary>
    ///

    internal delegate void SetButtonEnabled(bool value, Button b);

    public partial class GraphControl : UserControl, IPrimeMethodDivision
    {
        private PrimesBigInteger m_From;
        private PrimesBigInteger m_To;
        private CountPiXThread m_CountPixThread;
        private double m_RangeYFrom;
        private double m_RangeYTo;
        private readonly IFunction m_FunctionPix = null;
        private readonly IFunction m_FunctionLiN = null;
        private readonly IFunction m_FunctionPiGauss = null;

        public GraphControl()
        {
            try
            {
                InitializeComponent();

                cgraph.OnFunctionStart = FunctionStart;
                cgraph.OnFunctionStop = FunctionStop;
                cgraph.ClearPaintArea();

                m_FunctionPix = new FunctionPiX();
                m_FunctionPix.Executed += new ObjectParameterDelegate(m_FunctionPix_Executed);
                m_FunctionLiN = new FunctionLiN();
                m_FunctionLiN.Executed += new ObjectParameterDelegate(m_FunctionLiN_Executed);
                m_FunctionPiGauss = new FunctionPiGauss();
                m_FunctionPiGauss.Executed += new ObjectParameterDelegate(m_FunctionPiGauss_Executed);

                m_From = PrimesBigInteger.Zero;
                m_To = PrimesBigInteger.Zero;

                ircCountPrimes.Execute += new Primes.WpfControls.Components.ExecuteDelegate(ircCountPrimes_Execute);
                ircCountPrimes.Cancel += new VoidDelegate(ircCountPrimes_Cancel);
                OnStopPiX += new VoidDelegate(GraphControl_OnStopPiX);

                ircCountPrimes.SetText(InputRangeControl.FreeFrom, "2");
                ircCountPrimes.SetText(InputRangeControl.FreeTo, "100");

                ircCountPrimes.SetText(InputRangeControl.CalcFromFactor, "1");
                ircCountPrimes.SetText(InputRangeControl.CalcFromBase, "2");
                ircCountPrimes.SetText(InputRangeControl.CalcFromExp, "1");
                ircCountPrimes.SetText(InputRangeControl.CalcFromSum, "0");

                ircCountPrimes.SetText(InputRangeControl.CalcToFactor, "1");
                ircCountPrimes.SetText(InputRangeControl.CalcToBase, "10");
                ircCountPrimes.SetText(InputRangeControl.CalcToExp, "2");
                ircCountPrimes.SetText(InputRangeControl.CalcToSum, "0");

                ircCountPrimes.AddValueValidator(InputRangeControl.From, new BigIntegerMinValueValidator(null, PrimesBigInteger.Two));
                ircCountPrimes.AddValueValidator(InputRangeControl.To, new BigIntegerMinValueMaxValueValidator(null, PrimesBigInteger.Ten, PrimesBigInteger.ValueOf(int.MaxValue)));

                IValidator<PrimesBigInteger> rangevalidator = new BigIntegerMinValueValidator(null, PrimesBigInteger.ValueOf(10))
                {
                    Message = Primes.Resources.lang.WpfControls.Distribution.Distribution.graph_validatorrangemessage
                };
                ircCountPrimes.RangeValueValidator = rangevalidator;

                ircCountPrimes.KeyDown += new ExecuteDelegate(ircCountPrimes_KeyDown);
                ircCountPrimes.SetButtonCancelButtonEnabled(false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.EventLog.WriteEntry("Primes.GraphControl", ex.Message, System.Diagnostics.EventLogEntryType.Information);
                System.Diagnostics.EventLog.WriteEntry("Primes.GraphControl", ex.StackTrace, System.Diagnostics.EventLogEntryType.Information);
                if (ex.InnerException != null)
                {
                    System.Diagnostics.EventLog.WriteEntry("Primes.GraphControl", ex.InnerException.Message, System.Diagnostics.EventLogEntryType.Information);
                    System.Diagnostics.EventLog.WriteEntry("Primes.GraphControl", ex.InnerException.StackTrace, System.Diagnostics.EventLogEntryType.Information);
                }
            }
        }

        private void ircCountPrimes_KeyDown(PrimesBigInteger from, PrimesBigInteger to, PrimesBigInteger second)
        {
        }

        private void ircCountPrimes_Cancel()
        {
            Cancel();
        }

        private void ircCountPrimes_Execute(PrimesBigInteger from, PrimesBigInteger to, PrimesBigInteger second)
        {
            ExecuteGraph(from, to);
        }

        private void m_FunctionPiGauss_Executed(object obj)
        {
            SetInfo(tbInfoGaußPrimeTheorem, obj);
        }

        private void m_FunctionLiN_Executed(object obj)
        {
            SetInfo(tbInfoLin, obj);
        }

        private int functionPixExecutedSeqNo;

        private async void m_FunctionPix_Executed(object obj)
        {
            int thisSeqNo = ++functionPixExecutedSeqNo;
            if (thisSeqNo % 1000 > 0)
            {
                //Optimization: Immediately return process control back to caller and only proceed with rest of method
                //later if this method has not been called again in the meantime:
                await Task.Yield();
                if (thisSeqNo != functionPixExecutedSeqNo)
                {
                    //Method has been called in the meantime (with more recent infos), so we don't do anything:
                    return;
                }
            }

            if (PrimesCountList.Initialzed && m_To.LongValue <= PrimesCountList.MaxNumber)
            {
                ControlHandler.SetPropertyValue(lblCalcInfoPiX, "Text", Primes.Resources.lang.WpfControls.Distribution.Distribution.graph_pincountinfo);
                SetInfo(tbInfoPiX, PrimesCountList.GetPrime(m_To.LongValue));
            }
            else
            {
                SetInfo(tbInfoPiX, obj);
            }

        }

        private void SetInfo(Run tb, object value)
        {
            if (value != null)
            {
                string info = string.Empty;
                if (value.GetType() == typeof(double))
                {
                    info = ((double)value).ToString("N");
                }
                else if (value.GetType() == typeof(PrimesBigInteger))
                {
                    info = ((PrimesBigInteger)value).ToString();
                }
                else if (value.GetType() == typeof(int))
                {
                    info = ((int)value).ToString();
                }
                else if (value.GetType() == typeof(long))
                {
                    info = ((long)value).ToString();
                }

                ControlHandler.SetPropertyValue(tb, "Text", info);
            }
        }

        private void btnExec_Click(object sender, RoutedEventArgs e)
        {
            PrimesBigInteger from = null;
            PrimesBigInteger to = null;
            if (ircCountPrimes.GetValue(ref from, ref to))
            {
                ExecuteGraph(from, to);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
        }

        #region Events

        private event VoidDelegate OnStopPiX;

        private void GraphControl_OnStopPiX()
        {
            FunctionStop(m_FunctionPix);
        }

        #endregion

        private void Cancel()
        {
            CancelPiX();
            cgraph.CancelFunction(m_FunctionLiN);
            cgraph.CancelFunction(m_FunctionPiGauss);
            cgraph.CancelFunction(m_FunctionPix);
            ircCountPrimes.UnLockControls();
            ControlHandler.SetPropertyValue(lblCalcInfoPiXTextBlock, "Visibility", Visibility.Collapsed);
            ControlHandler.SetPropertyValue(lblCalcInfoPiGaussTextBlock, "Visibility", Visibility.Collapsed);
            ControlHandler.SetPropertyValue(lblCalcInfoLiNTextBlock, "Visibility", Visibility.Collapsed);

            //EnabledButton(btnExec);
            //DisabledButton(btnCancel);
            cgraph.ClearPaintArea();
            cgraph.RefreshCoordinateAxis();
            cgraph.CleanUp();
        }

        private void ResetControls()
        {
            cbPiX.IsEnabled = true;
            ircCountPrimes.LockControls();

            //Info("", lblInfoFunction, tbCalculateFromBase);
            //Info("", lblInfoFunction, tbCalculateFromExp);
            //Info("", lblInfoFunction, tbCalculateFromFactor);
            //Info("", lblInfoFunction, tbCalculateToBase);
            //Info("", lblInfoFunction, tbCalculateToExp);
            //Info("", lblInfoFunction, tbCalculateToFactor);

            //Info("", lblInfoFree, tbFreeTo);
            //Info("", lblInfoFunction, tbFreeFrom);
            //btnHelpLargeParamsFree.Visibility = Visibility.Hidden;
            //btnHelpLargeParamsFunction.Visibility = Visibility.Hidden;
        }

        private void ExecuteGraph(PrimesBigInteger from, PrimesBigInteger to)
        {
            CancelPiX();

            ResetControls();
            m_FunctionLiN.Reset();
            m_FunctionPiGauss.Reset();
            m_FunctionPix.Reset();

            //bool valid = false;
            //if (rbFreeRange.IsChecked.Value)
            //{
            //  valid = GetFreeParameters(ref from, ref to);
            //}
            //else if (rbFunctionRange.IsChecked.Value)
            //{
            //  valid = GetFunctionParameters(ref from, ref to);
            //}
            //if (valid)
            //{
            cgraph.ClearFunctions();

            if (m_From.CompareTo(from) != 0)
            {
                m_From = from;
                try
                {
                    m_RangeYFrom = Math.Min(m_FunctionPiGauss.Execute(m_From.DoubleValue), m_FunctionLiN.Execute(m_From.DoubleValue));
                    tbInfoGaußPrimeTheorem.Text = "";
                }
                catch (ResultNotDefinedException) { m_RangeYFrom = 2; }
                m_FunctionPiGauss.Reset();
                m_FunctionLiN.Reset();
            }

            m_FunctionLiN.MaxValue = to.DoubleValue;
            if (m_To != to)
            {
                m_To = to;
                m_RangeYTo = m_FunctionLiN.Execute(m_To.DoubleValue);
                m_FunctionLiN.Reset();
                tbInfoLin.Text = "";
            }

            cgraph.RangeX = new RangeX(from, to);

            string strFrom = Convert.ToInt32(Math.Floor(m_RangeYFrom)).ToString();
            string strTo = Convert.ToInt32(Math.Ceiling(m_RangeYTo)).ToString();

            cgraph.RangeY = new RangeY(new PrimesBigInteger(strFrom), new PrimesBigInteger(strTo));
            if (cbPiGauss.IsChecked.Value)
            {
                cgraph.AddFunctionExecute(m_FunctionPiGauss, from, to, FunctionType.CONSTANT, Brushes.Blue);
            }

            if (cbPiX.IsChecked.Value)
            {
                cgraph.AddFunctionExecute(m_FunctionPix, m_From, m_To, FunctionType.STAIR, Brushes.Red);
            }

            if (cbLin.IsChecked.Value)
            {
                cgraph.AddFunctionExecute(m_FunctionLiN, from, to, FunctionType.CONSTANT, Brushes.Green);
            }

            cgraph.ExecuteFunctions();

            if (to.CompareTo(PrimesBigInteger.ValueOf(10000)) > 0)
            {
                //cbPiX.IsEnabled = false;
                //m_CountPixThread = new CountPiXThread(m_FunctionPix as FunctionPiX, this.Dispatcher, m_FunctionPix_Executed, m_To);
                //m_CountPixThread.OnFunctionStart += new FunctionEvent(FunctionStart);
                //m_CountPixThread.OnFunctionStop += new FunctionEvent(FunctionStop);

                //m_CountPixThread.Start();
                //m_ThreadPix = new Thread(new ThreadStart(CalculatePiX));
                //m_ThreadPix.Start();
            }

            ControlHandler.SetPropertyValue(lblCalcInfoPiXTextBlock, "Visibility", Visibility.Visible);
            ControlHandler.SetPropertyValue(lblCalcInfoPiGaussTextBlock, "Visibility", Visibility.Visible);
            ControlHandler.SetPropertyValue(lblCalcInfoLiNTextBlock, "Visibility", Visibility.Visible);
            lblCalcInfoLiN.Text = Primes.Resources.lang.WpfControls.Distribution.Distribution.graph_lincountinfoCalculating;
            lblCalcInfoPiGauss.Text = Primes.Resources.lang.WpfControls.Distribution.Distribution.graph_gausscountinfoCalculating;
            lblCalcInfoPiX.Text = Primes.Resources.lang.WpfControls.Distribution.Distribution.graph_pincountinfoCalculating;
            tbInfoLin.Text = "";
            tbInfoGaußPrimeTheorem.Text = "";
            tbInfoPiX.Text = "";
            //}
        }

        private void CancelPiX()
        {
            if (m_CountPixThread != null)
            {
                m_CountPixThread.Abort();
                m_CountPixThread = null;
            }
            //if (m_ThreadPix != null)
            //{
            //  m_ThreadPix.Abort();
            //  m_ThreadPix = null;
            //}
        }

        private void CalculatePiX()
        {
            long n = 2;
            long counter = 0;

            if (PrimesCountList.Initialzed)
            {
                n = PrimesCountList.MaxNumber;
                counter = PrimesCountList.GetPrime(n);
            }
            m_FunctionPix_Executed(n);
            m_FunctionPix.FunctionState = FunctionState.Running;

            while (n < m_To.LongValue)
            {
                n++;
                counter = (long)m_FunctionPix.Execute(n);
            }
            m_FunctionPix.FunctionState = FunctionState.Stopped;
            if (OnStopPiX != null)
            {
                OnStopPiX();
            }
        }

        //#region FreeRange Validation
        //
        //public void InfoFreeParams(string message, TextBox textbox)
        //{
        //  Info(message, lblInfoFree, textbox);
        //}
        //public void ErrorFreeParams(string message, TextBox textbox)
        //{
        //  Error(message, lblInfoFree, textbox);
        //}

        //public void ErrorLargeFreeParams(TextBox textbox)
        //{
        //  ErrorFreeParams("Achtung: Bitte beachten Sie die Hilfe, bei Eingabe von großen Zahlen.", textbox);
        //  btnHelpLargeParamsFree.Visibility = Visibility.Visible;
        //}

        //private bool GetFreeParameters(ref PrimesBigInteger ifrom, ref PrimesBigInteger ito)
        //{
        //  PrimesBigInteger from = ValidateFreeParameter(tbFreeFrom);
        //  PrimesBigInteger to = ValidateFreeParameter(tbFreeTo);

        //  if (from != null && to != null)
        //  {
        //    if (from.CompareTo(to) >= 0)
        //    {
        //      Info("Der Wert im Feld \"von\" muss kleiner sein, als der im Feld \"bis\"", lblInfoFree, tbFreeFrom);
        //      return false;
        //    }
        //    else
        //    {
        //      if (to.CompareTo(new PrimesBigInteger("5000000000")) > 0)
        //      {
        //        Info("Achtung: Werte größer als 500.000.000 können nicht analysiert werden.", lblInfoFree, tbFreeTo);
        //        return false;
        //      }
        //      ifrom = from;
        //      ito = to;
        //      return true;
        //    }
        //  }
        //  else
        //  {
        //    return false;
        //  }
        //}

        //private PrimesBigInteger ValidateFreeParameter(TextBox tbSource)
        //{
        //  PrimesBigInteger result = PrimesBigInteger.Zero;
        //  try
        //  {
        //    TextBoxValidator<PrimesBigInteger> tbvalidator = new TextBoxValidator<PrimesBigInteger>(m_BiMaxValueValidator, tbSource);
        //    tbvalidator.Validate(ref result);
        //  }
        //  catch (ControlValidationException cvex)
        //  {
        //    switch (cvex.ValidationResult)
        //    {
        //      case Primes.WpfControls.Validation.ValidationResult.WARNING:
        //        InfoFreeParams(cvex.Message, cvex.Control as TextBox);
        //        break;
        //      case Primes.WpfControls.Validation.ValidationResult.ERROR:
        //        ErrorFreeParams(cvex.Message, cvex.Control as TextBox);
        //        break;
        //    }
        //  }
        //  return result;
        //}

        //private void tbFree_KeyUp(object sender, KeyEventArgs e)
        //{
        //  tb_KeyUp(sender, e, GetFreeParameters, ErrorLargeFreeParams, (sender as TextBox).Name.ToLower().Contains("from"));
        //}
        //
        //private void tbInputKeyDown(object sender, KeyEventArgs e)
        //{
        //  if (e.Key == Key.Enter)
        //  {
        //    ExecuteGraph();
        //  }
        //}

        //#endregion

        //#region FunctionRange Validation
        //
        //private void InfoFunctionParams(string message, TextBox textbox)
        //{
        //  Info(message, lblInfoFunction, textbox);
        //}
        //private void ErrorFunctionParams(string message, TextBox textbox)
        //{
        //  Error(message, lblInfoFunction, textbox);
        //}

        //private void ErrorLargeFunctionParams(TextBox tbsource)
        //{
        //  if (tbsource == tbCalculateFromBase || tbsource == tbCalculateFromExp || tbsource == tbCalculateFromFactor)
        //  {
        //    ErrorFunctionParams("Achtung: Bitte beachten Sie die Hilfe, bei Eingabe von großen Zahlen.", tbCalculateFromBase);
        //    ErrorFunctionParams("Achtung: Bitte beachten Sie die Hilfe, bei Eingabe von großen Zahlen.", tbCalculateFromExp);
        //    ErrorFunctionParams("Achtung: Bitte beachten Sie die Hilfe, bei Eingabe von großen Zahlen.", tbCalculateFromFactor);
        //  }
        //  else
        //  {
        //    ErrorFunctionParams("Achtung: Bitte beachten Sie die Hilfe, bei Eingabe von großen Zahlen.", tbCalculateToBase);
        //    ErrorFunctionParams("Achtung: Bitte beachten Sie die Hilfe, bei Eingabe von großen Zahlen.", tbCalculateToExp);
        //    ErrorFunctionParams("Achtung: Bitte beachten Sie die Hilfe, bei Eingabe von großen Zahlen.", tbCalculateToFactor);
        //  }
        //  btnHelpLargeParamsFunction.Visibility = Visibility.Visible;
        //}

        //private bool GetFunctionParameters(ref PrimesBigInteger ifrom, ref PrimesBigInteger ito)
        //{
        //  PrimesBigInteger from = ValidateFunctionParameter(tbCalculateFromFactor, tbCalculateFromBase, tbCalculateFromExp);
        //  PrimesBigInteger to = ValidateFunctionParameter(tbCalculateToFactor, tbCalculateToBase, tbCalculateToExp);
        //  if (from != null && to != null)
        //  {
        //    if (from.CompareTo(to) >= 0)
        //    {
        //      Info("Der Wert im Feld \"von\" muss kleiner sein, als der im Feld \"bis\"", lblInfoFunction, tbFreeFrom);
        //      return false;
        //    }
        //    else
        //    {
        //      ifrom = from;
        //      ito = to;
        //      return true;
        //    }
        //  }
        //  else
        //  {
        //    return false;
        //  }
        //}

        //private PrimesBigInteger ValidateFunctionParameter(TextBox tbSourceFactor, TextBox tbSourceBase, TextBox tbSourceExp)
        //{
        //  PrimesBigInteger result = null;
        //  try
        //  {
        //    PrimesBigInteger factor = ValidateFunctionParameter(tbSourceFactor);
        //    PrimesBigInteger base_ = ValidateFunctionParameter(tbSourceBase);
        //    PrimesBigInteger exp = ValidateFunctionParameter(tbSourceExp);
        //    if (factor != null && base_ != null && exp != null)
        //    {
        //      result = factor.Multiply(base_.Pow(exp.IntValue));
        //      if (result.CompareTo(new PrimesBigInteger("50000000000000000")) > 0)
        //      {
        //        result = null;
        //        Info("Achtung: Werte größer als 50.000.000.000 können nicht analysiert werden.", lblInfoFunction, tbSourceFactor);
        //        Info("Achtung: Werte größer als 50.000.000.000 können nicht analysiert werden.", lblInfoFunction, tbSourceBase);
        //        Info("Achtung: Werte größer als 50.000.000.000 können nicht analysiert werden.", lblInfoFunction, tbSourceExp);
        //      }

        //    }
        //  }
        //  catch (ControlValidationException cvex)
        //  {
        //    switch (cvex.ValidationResult)
        //    {
        //      case Primes.WpfControls.Validation.ValidationResult.WARNING:
        //        InfoFunctionParams(cvex.Message, cvex.Control as TextBox);
        //        break;
        //      case Primes.WpfControls.Validation.ValidationResult.ERROR:
        //        ErrorFunctionParams(cvex.Message, cvex.Control as TextBox);
        //        break;
        //    }
        //  }
        //  return result;
        //}

        //private PrimesBigInteger ValidateFunctionParameter(TextBox tbSource)
        //{
        //  PrimesBigInteger result = PrimesBigInteger.Zero;

        //  IValidator<PrimesBigInteger> biValidator = new PositiveMaxValueBigIntegerValidator(tbSource.Text, PrimesBigInteger.ValueOf(10));
        //  TextBoxValidator<PrimesBigInteger> tbvalidator = new TextBoxValidator<PrimesBigInteger>(biValidator, tbSource);
        //  tbvalidator.Validate(ref result);
        //  return result;
        //}

        //private void tbFunction_KeyUp(object sender, KeyEventArgs e)
        //{
        //  tb_KeyUp(sender, e, GetFunctionParameters, ErrorLargeFunctionParams, (sender as TextBox).Name.ToLower().Contains("from"));
        //}

        //internal delegate bool GetParameters(ref PrimesBigInteger from, ref PrimesBigInteger to);
        //internal delegate void ErrorLargeParams(TextBox tbsource);
        //private void tb_KeyUp(object sender, KeyEventArgs e, GetParameters getparams, ErrorLargeParams errorlargeparams, bool isfrom)
        //{
        //  if (e.Key != Key.Left && e.Key != Key.Right && e.Key != Key.LeftCtrl && e.Key != Key.RightCtrl)
        //  {
        //    ResetControls();
        //    if (sender != null && sender.GetType() == typeof(TextBox))
        //    {
        //      TextBox tbSource = sender as TextBox;
        //      if (!string.IsNullOrEmpty(tbSource.Text))
        //      {
        //        PrimesBigInteger from = PrimesBigInteger.Zero;
        //        PrimesBigInteger to = PrimesBigInteger.Zero;

        //        getparams(ref from, ref to);
        //        if (isfrom)
        //        {
        //          if (from.CompareTo(PrimesBigInteger.ValueOf(200))>0)
        //          {
        //            errorlargeparams(tbSource);
        //          }

        //        }
        //        else
        //        {
        //          if (to.CompareTo(PrimesBigInteger.ValueOf(200)) > 0)
        //          {
        //            errorlargeparams(tbSource);
        //          }
        //        }
        //      }
        //    }
        //  }
        //}

        //#endregion

        //#region Info-Functions
        //
        //public void Info(string message, Label lbl, TextBox tb)
        //{
        //  if (!string.IsNullOrEmpty(message) && lbl != null && tb != null)
        //  {
        //    lbl.Foreground = Brushes.Blue;
        //    tb.Background = Brushes.LightBlue;

        //    lbl.Content = message;
        //  }
        //  else
        //  {
        //    lbl.Content = string.Empty;
        //    tb.Background = Brushes.White;
        //    tb.Foreground = Brushes.Black;
        //  }
        //}
        //public void Error(string message, Label lbl, TextBox tb)
        //{
        //  if (!string.IsNullOrEmpty(message) && lbl != null && tb != null)
        //  {
        //    lbl.Foreground = Brushes.Red;
        //    tb.Background = Brushes.Red;
        //    tb.Foreground = Brushes.WhiteSmoke;
        //    lbl.Content = message;
        //  }
        //  else
        //  {
        //    lbl.Content = string.Empty;
        //    tb.Background = Brushes.White;
        //    tb.Foreground = Brushes.Black;
        //  }
        //}

        //#endregion

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            int padding = 50;

            base.OnRenderSizeChanged(sizeInfo);

            double width = sizeInfo.NewSize.Width - 2 * padding;
            if (width < 0)
            {
                width = sizeInfo.NewSize.Width;
            }

            double height = sizeInfo.NewSize.Height - ircCountPrimes.ActualHeight - gbFunctions.ActualHeight - padding - sliderTest.ActualHeight;
            height = Math.Max(0, height);

            cgraph.Width = width;
            cgraph.Height = height;
            PaintArea.Width = width;
            PaintArea.Height = height;
            vb.Width = width;
            vb.Height = height;
        }

        #region Setting Buttons

        // private void rbFreeRange_Checked(object sender, RoutedEventArgs e)
        // {
        //   if(tbFreeFrom!=null&&tbFreeTo!=null)
        //     tbFreeFrom.IsEnabled = tbFreeTo.IsEnabled = rbFreeRange.IsChecked.Value;

        //   if (rbFunctionRange != null)
        //     rbFunctionRange.IsChecked = !rbFreeRange.IsChecked.Value;
        // }

        // private void rbFunctionRange_Checked(object sender, RoutedEventArgs e)
        // {
        //   rbFreeRange.IsChecked = !rbFunctionRange.IsChecked.Value;
        //   if (tbCalculateFromFactor != null && tbCalculateFromBase != null && tbCalculateFromExp != null &&
        //       tbCalculateToFactor != null && tbCalculateToBase != null && tbCalculateToExp != null)
        //   {
        //     tbCalculateFromFactor.IsEnabled = tbCalculateFromBase.IsEnabled = tbCalculateFromExp.IsEnabled
        //       = tbCalculateToFactor.IsEnabled = tbCalculateToBase.IsEnabled = tbCalculateToExp.IsEnabled = rbFunctionRange.IsChecked.Value;
        //   }
        // }

        public void Dispose()
        {
            cgraph.CleanUp();
            CancelPiX();
        }

        private void GetFunctionButtons(IFunction function, ref Button btnStop, ref Button btnResume)
        {
            if (function == m_FunctionPiGauss)
            {
                btnStop = btnStopPiGauss;
                btnResume = btnResumePiGauss;
            }
            else if (function == m_FunctionPix)
            {
                btnStop = btnStopPix;
                btnResume = btnResumePix;
            }
            else if (function == m_FunctionLiN)
            {
                btnStop = btnStopLin;
                btnResume = btnResumeLin;
            }
        }

        private void StopFunctionClick(object sender, RoutedEventArgs e)
        {
            IFunction function = null;
            if (sender.GetType() == typeof(Button))
            {
                switch ((sender as Button).Name)
                {
                    case "btnStopPiGauss":
                        function = m_FunctionPiGauss;
                        break;
                    case "btnStopPix":
                        function = m_FunctionPix;
                        break;
                    case "btnStopLin":
                        function = m_FunctionLiN;
                        break;
                }
            }
            if (function != null)
            {
                Button btnStop = null;
                Button btnResume = null;

                GetFunctionButtons(function, ref btnStop, ref btnResume);

                cgraph.StopFunction(function);
                if (m_CountPixThread != null)
                {
                    m_CountPixThread.Suspend();
                }

                EnabledButton(btnResume);
                DisabledButton(btnStop);
            }
        }

        private void ResumeFunctionClick(object sender, RoutedEventArgs e)
        {
            IFunction function = null;
            if (sender.GetType() == typeof(Button))
            {
                switch ((sender as Button).Name)
                {
                    case "btnResumePiGauss":
                        function = m_FunctionPiGauss;
                        break;
                    case "btnResumePix":
                        function = m_FunctionPix;
                        break;
                    case "btnResumeLin":
                        function = m_FunctionLiN;
                        break;
                }
            }

            if (function != null)
            {
                Button btnStop = null;
                Button btnResume = null;

                GetFunctionButtons(function, ref btnStop, ref btnResume);
                cgraph.ResumeFunction(function);
                if (m_CountPixThread != null)
                {
                    m_CountPixThread.Resume();
                }
                DisabledButton(btnResume);
                EnabledButton(btnStop);
            }
        }

        private void EnabledButton(Button btn)
        {
            ControlHandler.EnableButton(btn);
        }

        private void DisabledButton(Button btn)
        {
            ControlHandler.DisableButton(btn);
        }

        public void FunctionStart(IFunction function)
        {
            Button btnStop = null;
            Button btnResume = null;

            GetFunctionButtons(function, ref btnStop, ref btnResume);

            if (btnStop != null)
            {
                EnabledButton(btnStop);
            }

            if (btnResume != null)
            {
                DisabledButton(btnResume);
            }

            ircCountPrimes.LockControls();
            //DisabledButton(btnExec);
            //EnabledButton(btnCancel);
        }

        private readonly object m_LockObjectFunctionStop = new object();

        public void FunctionStop(IFunction function)
        {
            Button btnStop = null;
            Button btnResume = null;
            lock (m_LockObjectFunctionStop)
            {
                GetFunctionButtons(function, ref btnStop, ref btnResume);

                if (m_FunctionLiN.FunctionState == FunctionState.Stopped
                  && m_FunctionPix.FunctionState == FunctionState.Stopped
                  && m_FunctionPiGauss.FunctionState == FunctionState.Stopped)
                {
                    ircCountPrimes.UnLockControls();
                }

                if (function == m_FunctionPix)
                {
                    ControlHandler.SetPropertyValue(lblCalcInfoPiX, "Text", Primes.Resources.lang.WpfControls.Distribution.Distribution.graph_pincountinfo);
                }
                else if (function == m_FunctionLiN)
                {
                    ControlHandler.SetPropertyValue(lblCalcInfoLiN, "Text", Primes.Resources.lang.WpfControls.Distribution.Distribution.graph_lincountinfo);
                }
                else if (function == m_FunctionPiGauss)
                {
                    ControlHandler.SetPropertyValue(lblCalcInfoPiGauss, "Text", Primes.Resources.lang.WpfControls.Distribution.Distribution.graph_gausscountinfo);
                }

                if (btnStop != null)
                {
                    DisabledButton(btnStop);
                }

                if (btnResume != null)
                {
                    DisabledButton(btnResume);
                }
            }
        }

        #endregion

        private void btnHelpLargeParamsClick(object sender, RoutedEventArgs e)
        {
            Primes.OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(Primes.OnlineHelp.OnlineHelpActions.Graph_LargeNumbers);
        }

        private void btnHelpFunctionTypeClick(object sender, MouseButtonEventArgs e)
        {
            if (sender.GetType() == typeof(Image))
            {
                Image sender_ = sender as Image;
                if (sender_ == btnHelpLiN)
                {
                    Primes.OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(Primes.OnlineHelp.OnlineHelpActions.Graph_LiN);
                }
                else if (sender_ == btnHelpPiGauss)
                {
                    Primes.OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(Primes.OnlineHelp.OnlineHelpActions.Graph_GaussPi);
                }
                else if (sender_ == btnHelpPiX)
                {
                    Primes.OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(Primes.OnlineHelp.OnlineHelpActions.Graph_PiX);
                }
            }

            e.Handled = true;
        }

        private void cbDontPaint_Click(object sender, RoutedEventArgs e)
        {
            pnlFunctions.IsEnabled = !cbDontPaint.IsChecked.Value;
        }

        #region IPrimeUserControl Members

        public void SetTab(int i)
        {
        }

        #endregion


        #region IPrimeUserControl Members

        public event VoidDelegate Execute;

        public event VoidDelegate Stop;

        private void FireExecuteEvent()
        {
            if (Execute != null)
            {
                Execute();
            }
        }

        private void FireStopEvent()
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
    }
}
