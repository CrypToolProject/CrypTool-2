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
using Primes.OnlineHelp;
using Primes.WpfControls.Components;
using Primes.WpfControls.Primegeneration.Function;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Primes.WpfControls.Primegeneration
{
    /// <summary>
    /// Interaction logic for PrimesgenerationControl.xaml
    /// </summary>
    public partial class PrimesgenerationControl : UserControl, IPrimeMethodDivision
    {
        #region Properties

        private readonly ExpressionExecuter m_ExpressionExecuter;
        private readonly PolynomRangeExecuter m_PolynomRangeExecuter;
        private readonly InputControlNTimesM m_InputControlNTimesM;
        private readonly InputControlPolynom m_InputControlPolynom;
        private readonly InputControlPolynomRange m_InputControlPolynomRange;

        private int m_PolynomGeneratedCount;
        private int m_PolynomGeneratedPrimes;

        #endregion

        public PrimesgenerationControl()
        {
            InitializeComponent();
            m_ExpressionExecuter = new ExpressionExecuter();
            m_ExpressionExecuter.FunctionResult += new ExpressionResultDelegate(exec_FunctionResult);
            m_ExpressionExecuter.Start += new VoidDelegate(m_ExpressionExecuter_Start);
            m_ExpressionExecuter.Stop += new VoidDelegate(m_ExpressionExecuter_Stop);

            m_PolynomRangeExecuter = new PolynomRangeExecuter();
            m_PolynomRangeExecuter.FunctionResult += new PolynomeRangeResultDelegate(m_PolynomRangeExecuter_FunctionResult);
            m_PolynomRangeExecuter.Start += new VoidDelegate(m_PolynomRangeExecuter_Start);
            m_PolynomRangeExecuter.Stop += new VoidDelegate(m_PolynomRangeExecuter_Stop);

            m_InputControlNTimesM = new InputControlNTimesM();
            m_InputControlNTimesM.Execute += new Execute_N_Digits_Delegate(ExecuteGenerateNPrimes);
            m_InputControlNTimesM.Cancel += new VoidDelegate(m_InputControlNTimesM_Cancel);
            gnentimesm.Execute += new Execute_N_Digits_Delegate(ExecuteGenerateNPrimes);
            gnentimesm.Cancel += new VoidDelegate(m_InputControlNTimesM_Cancel);


            m_InputControlPolynom = new InputControlPolynom();
            m_InputControlPolynom.Execute += new ExecutePolynomDelegate(m_InputControlPolynom_Execute);
            m_InputControlPolynom.Cancel += new VoidDelegate(m_InputControlPolynom_Cancel);

            m_InputControlPolynomRange = new InputControlPolynomRange();
            m_InputControlPolynomRange.Execute += new ExecutePolynomRangeDelegate(m_InputControlPolynomRange_Execute);
            m_InputControlPolynomRange.Cancel += new VoidDelegate(m_InputControlPolynomRange_Cancel);

            ResetPolynomStats();
        }

        #region Input Polynom Range

        private void m_PolynomRangeExecuter_Stop()
        {
            int row = log.NewLine();

            bool rndExecution = m_PolynomRangeExecuter.From.Equals(PrimesBigInteger.NaN) && m_PolynomRangeExecuter.To.Equals(PrimesBigInteger.NaN);
            if (!rndExecution)
            {
                log.Info(
                  string.Format(
                    Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.statRndInterval,
                    new object[] { m_PolynomCounter.ToString(), m_PolynomRangeExecuter.From.ToString(), m_PolynomRangeExecuter.To.ToString() }), 0, row);
            }
            else
            {
                log.Info(
                  string.Format(
                    Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.statRnd,
                    new object[] { m_PolynomCounter.ToString() }), 0, row);
            }
            row = log.NewLine();
            log.Info("", 0, row);
            row = log.NewLine();
            try
            {
                /*Calculating exact*/
                double pc = m_PrimesCounter.DoubleValue;
                double rc = m_ResultCounter.DoubleValue;
                log.Info(
                  string.Format(
                    Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.statAvgCountPrimes,
                    new object[] { (pc / rc).ToString("N"), ((pc * 100.0) / rc).ToString("N") }), 0, row);
            }
            catch
            {
                log.Info(
                  string.Format(
                    Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.statAvgCountPrimes,
                    new object[] { m_PrimesCounter.Divide(m_ResultCounter).ToString(), m_PrimesCounter.Multiply(PrimesBigInteger.ValueOf(100)).Divide(m_ResultCounter).ToString() }), 0, row);
            }
            row = log.NewLine();
            if (!rndExecution)
            {
                log.Info(
                  Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.statFunctionsMostInterval, 0, row);
            }
            else
            {
                log.Info(
                  Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.statFunctionsMost, 0, row);
            }
            foreach (IPolynom p in m_ListMostPrimes)
            {
                row = log.NewLine();
                log.Info(
                  p.ToString(), 0, row);
            }
            row = log.NewLine();
            if (!rndExecution)
            {
                log.Info(
                  Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.statFunctionsMostMiscInterval, 0, row);
            }
            else
            {
                log.Info(
                  Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.statFunctionsMostMisc, 0, row);
            }
            foreach (IPolynom p in m_ListMostPrimesAbsolut)
            {
                row = log.NewLine();
                log.Info(
                  p.ToString(), 0, row);
            }
            row = log.NewLine();

            if (!rndExecution)
            {
                log.Info(
                Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.statFunctionsLeastInterval, 0, row);
            }
            else
            {
                log.Info(
                Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.statFunctionsLeast, 0, row);
            }
            foreach (IPolynom p in m_ListLeastPrimes)
            {
                row = log.NewLine();
                log.Info(
                  p.ToString(), 0, row);
            }

            m_ListMostPrimes.Clear();
            m_ListLeastPrimes.Clear();
            m_ResultCounter = PrimesBigInteger.Zero;
            m_PrimesCounter = PrimesBigInteger.Zero;
            m_PolynomCounter = PrimesBigInteger.Zero;

            m_MostPrimes = null;
            m_LeastPrimes = null;
            m_InputControlPolynomRange.Stop();
        }

        private void m_PolynomRangeExecuter_Start()
        {
            m_InputControlPolynomRange.Start();
        }

        private void m_InputControlPolynomRange_Execute(
          IPolynom p,
          PrimesBigInteger from,
          PrimesBigInteger to,
          PrimesBigInteger numberOfCalculations,
          PrimesBigInteger numberOfFormulars,
          IList<KeyValuePair<string, Primes.Library.Range>> parameters)
        {
            log.Clear();
            log.Columns = 2;
            log.ShowCounter = false;
            m_PolynomRangeExecuter.From = from;
            m_PolynomRangeExecuter.To = to;
            m_PolynomRangeExecuter.Function = p;
            m_PolynomRangeExecuter.Parameters = parameters;
            m_PolynomRangeExecuter.NumberOfCalculations = numberOfCalculations;
            m_PolynomRangeExecuter.NumberOfFormulars = numberOfFormulars;

            m_PolynomRangeExecuter.Execute();
        }

        private void m_InputControlPolynomRange_Cancel()
        {
            m_PolynomRangeExecuter.Cancel();
            m_InputControlPolynomRange.Stop();
        }

        private readonly IList<IPolynom> m_ListMostPrimes = new List<IPolynom>();
        private readonly IList<IPolynom> m_ListMostPrimesAbsolut = new List<IPolynom>();
        private readonly IList<IPolynom> m_ListLeastPrimes = new List<IPolynom>();
        private PrimesBigInteger m_ResultCounter = PrimesBigInteger.Zero;
        private PrimesBigInteger m_PrimesCounter = PrimesBigInteger.Zero;
        private PrimesBigInteger m_PolynomCounter = PrimesBigInteger.Zero;

        private PrimesBigInteger m_MostPrimes = null;
        private PrimesBigInteger m_MostPrimesAbsolut = null;

        private PrimesBigInteger m_LeastPrimes = null;

        private void m_PolynomRangeExecuter_FunctionResult(IPolynom p, PrimesBigInteger primesCount, PrimesBigInteger primesCountReal, PrimesBigInteger counter)
        {
            int row = log.NewLine();
            log.Info(p.ToString(), 0, row);
            PrimesBigInteger percent = primesCount.Multiply(PrimesBigInteger.ValueOf(100)).Divide(counter);
            PrimesBigInteger percentAbsolut = primesCountReal.Multiply(PrimesBigInteger.ValueOf(100)).Divide(counter);

            /*Most Primes*/
            if (m_MostPrimes == null)
            {
                m_MostPrimes = percent;
                m_ListMostPrimes.Add((p as SecondDegreePolynom).Clone() as IPolynom);
            }
            else
            {
                if (m_MostPrimes.Equals(percent))
                {
                    m_ListMostPrimes.Add((p as SecondDegreePolynom).Clone() as IPolynom);
                }
                else
                {
                    if (m_MostPrimes.CompareTo(percent) < 0)
                    {
                        m_MostPrimes = percent;
                        m_ListMostPrimes.Clear();
                        m_ListMostPrimes.Add((p as SecondDegreePolynom).Clone() as IPolynom);
                    }
                }
            }

            if (m_MostPrimesAbsolut == null)
            {
                m_MostPrimesAbsolut = percentAbsolut;
                m_ListMostPrimesAbsolut.Add((p as SecondDegreePolynom).Clone() as IPolynom);
            }
            else
            {
                if (m_MostPrimesAbsolut.Equals(percentAbsolut))
                {
                    m_ListMostPrimesAbsolut.Add((p as SecondDegreePolynom).Clone() as IPolynom);
                }
                else
                {
                    if (m_MostPrimesAbsolut.CompareTo(percentAbsolut) < 0)
                    {
                        m_MostPrimesAbsolut = percentAbsolut;
                        m_ListMostPrimesAbsolut.Clear();
                        m_ListMostPrimesAbsolut.Add((p as SecondDegreePolynom).Clone() as IPolynom);
                    }
                }
            }

            /*Least Primes*/
            if (m_LeastPrimes == null)
            {
                m_LeastPrimes = percent;
                m_ListLeastPrimes.Add((p as SecondDegreePolynom).Clone() as IPolynom);
            }
            else
            {
                if (m_LeastPrimes.Equals(percent))
                {
                    m_ListLeastPrimes.Add((p as SecondDegreePolynom).Clone() as IPolynom);
                }
                else
                {
                    if (m_LeastPrimes.CompareTo(percent) > 0)
                    {
                        m_LeastPrimes = percent;
                        m_ListLeastPrimes.Clear();
                        m_ListLeastPrimes.Add((p as SecondDegreePolynom).Clone() as IPolynom);
                    }
                }
            }

            m_ResultCounter = m_ResultCounter.Add(counter);
            m_PrimesCounter = m_PrimesCounter.Add(primesCount);
            m_PolynomCounter = m_PolynomCounter.Add(PrimesBigInteger.One);
            log.Info(string.Format(Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.statGenerated, new object[] { primesCount, percent, primesCountReal }), 1, row);
        }

        #endregion

        #region Input Simple Polnom

        private void ResetPolynomStats()
        {
            m_PolynomGeneratedCount = 0;
            m_PolynomGeneratedPrimes = 0;
        }

        private void PrintPolynomStats()
        {
            log.ShowCounter = false;
            log.Info(
              string.Format(
                Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.statGeneratedAbsolut,
                new object[] {
            m_PolynomGeneratedCount,
            m_PolynomGeneratedPrimes,
            ((m_PolynomGeneratedPrimes * 100.0) / m_PolynomGeneratedCount).ToString("00.00") }));
            ResetPolynomStats();
        }

        private void m_InputControlPolynom_Cancel()
        {
            m_ExpressionExecuter.Cancel();
            PrintPolynomStats();
        }

        private void m_InputControlPolynom_Execute(PrimesBigInteger from, PrimesBigInteger to, IPolynom p)
        {
            log.Clear();
            log.Columns = 2;
            log.ShowCounter = true;
            m_ExpressionExecuter.Function = p;
            m_ExpressionExecuter.Execute(from, to);
        }

        private void m_ExpressionExecuter_Stop()
        {
            m_InputControlNTimesM.SetButtonCancelEnable(false);
            m_InputControlNTimesM.SetButtonExecuteEnable(true);
            gnentimesm.SetButtonCancelEnable(false);
            gnentimesm.SetButtonExecuteEnable(true);

            m_InputControlPolynom.Stop();
            if (m_ExpressionExecuter.Function != null && m_ExpressionExecuter.Function.GetType().GetInterface("IPolynom") != null)
            {
                PrintPolynomStats();
            }
        }

        private void m_ExpressionExecuter_Start()
        {
            m_InputControlNTimesM.SetButtonCancelEnable(true);
            m_InputControlNTimesM.SetButtonExecuteEnable(false);
            gnentimesm.SetButtonCancelEnable(true);
            gnentimesm.SetButtonExecuteEnable(false);
        }

        private void m_InputControlNTimesM_Cancel()
        {
            m_ExpressionExecuter.Cancel();
            m_InputControlNTimesM.SetButtonCancelEnable(false);
            m_InputControlNTimesM.SetButtonExecuteEnable(true);
            gnentimesm.SetButtonCancelEnable(false);
            gnentimesm.SetButtonExecuteEnable(true);
        }

        #endregion

        #region IPrimeUserControl Members

        public void Dispose()
        {
            CancelAll();
        }

        #endregion

        private void Generate10Times_Click(object sender, RoutedEventArgs e)
        {
            log.Columns = 1;
            if (sender != null && sender.GetType() == typeof(Button))
            {
                PrimesBigInteger digits = null;
                if (sender == btnGeneratePrimes10Times20)
                {
                    digits = PrimesBigInteger.ValueOf(20);
                }
                else if (sender == btnGeneratePrimes10Times50)
                {
                    digits = PrimesBigInteger.ValueOf(50);
                }
                else if (sender == btnGeneratePrimes10Times100)
                {
                    digits = PrimesBigInteger.ValueOf(100);
                }
                else if (sender == btnGeneratePrimesNTimesM)
                {
                    SetInputControl(m_InputControlNTimesM);
                }
                if (digits != null)
                {
                    ExecuteGenerate10Primes(digits);
                }
            }
        }

        private void ExecuteGenerate10Primes(PrimesBigInteger digits)
        {
            ExecuteGenerateNPrimes(PrimesBigInteger.ValueOf(10), digits);
        }

        private void ExecuteGenerateNPrimes(PrimesBigInteger count, PrimesBigInteger digits)
        {
            log.Clear();
            log.Columns = 1;
            GenerateMDigitPrimes exp = new GenerateMDigitPrimes();
            exp.NonFurtherPrimeFound += new VoidDelegate(exp_NonFurtherPrimeFound);
            exp.SetParameter(GenerateMDigitPrimes.LEN, digits);

            m_ExpressionExecuter.Function = exp;
            m_ExpressionExecuter.Execute(PrimesBigInteger.ValueOf(1), count);
        }

        private void exp_NonFurtherPrimeFound()
        {
            log.Info(Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.statNonPrimes);
            m_InputControlNTimesM.SetButtonExecuteEnable(true);
            m_InputControlNTimesM.SetButtonCancelEnable(false);
            gnentimesm.SetButtonCancelEnable(false);
            gnentimesm.SetButtonExecuteEnable(true);

            m_ExpressionExecuter.Cancel();
        }

        private readonly object counterlockobject = new object();

        private void exec_FunctionResult(PrimesBigInteger result, PrimesBigInteger input)
        {
            int row = log.NewLine();
            if (m_ExpressionExecuter.Function != null && m_ExpressionExecuter.Function.GetType().GetInterface("IPolynom") != null)
            {
                log.Info(string.Format("f({0}) = {1}", new object[] { input, result }), 0, row);
                bool isPrime = result.IsPrime(10);
                lock (counterlockobject)
                {
                    m_PolynomGeneratedCount++;
                    m_PolynomGeneratedPrimes += (isPrime) ? 1 : 0;
                }
                log.Info((isPrime) ? Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.isPrime : Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.isNotPrime, 1, row);
            }
            else
            {
                log.Info(result.ToString(), 0, row);
            }
        }

        private void btnGeneratePolynom_Click(object sender, RoutedEventArgs e)
        {
            IPolynom p = null;
            if (sender == btnGenerateSecondDegree)
            {
                p = new SecondDegreePolynom();
                p.SetParameter("a", PrimesBigInteger.ValueOf(3));
                p.SetParameter("b", PrimesBigInteger.ValueOf(7));
                p.SetParameter("c", PrimesBigInteger.ValueOf(13));
                m_InputControlPolynom.SetText(InputRangeControl.FreeFrom, "0");
                m_InputControlPolynom.SetText(InputRangeControl.FreeTo, "49");

                m_InputControlPolynom.SetText(InputRangeControl.CalcFromFactor, "0");
                m_InputControlPolynom.SetText(InputRangeControl.CalcFromBase, "2");
                m_InputControlPolynom.SetText(InputRangeControl.CalcFromExp, "3");
                m_InputControlPolynom.SetText(InputRangeControl.CalcFromSum, "-1");
                m_InputControlPolynom.SetText(InputRangeControl.CalcToFactor, "1");
                m_InputControlPolynom.SetText(InputRangeControl.CalcToBase, "7");
                m_InputControlPolynom.SetText(InputRangeControl.CalcToExp, "2");
                m_InputControlPolynom.SetText(InputRangeControl.CalcToSum, "0");
            }
            else if (sender == btnGeneratePrimesEuler)
            {
                p = new EulerPolynom();
                m_InputControlPolynom.SetText(InputRangeControl.FreeFrom, "0");
                m_InputControlPolynom.SetText(InputRangeControl.FreeTo, "39");
            }
            if (p != null)
            {
                m_InputControlPolynom.Polynom = p;
                SetInputControl(m_InputControlPolynom);
            }
        }

        private void SetInputControl(UserControl uc)
        {
            CancelAll();
            pnlInput.Children.Clear();
            pnlInput.Children.Add(uc);
        }

        private void btnGeneratePolynoms_Click(object sender, RoutedEventArgs e)
        {
            if (sender == btnGenerateRandomPolynoms)
            {
                m_InputControlPolynomRange.Polynom = new SecondDegreePolynomRandom();
                m_PolynomRangeExecuter.PolynomRangeExecuterMode = m_InputControlPolynomRange.PolynomRangeExecuterMode = PolynomRangeExecuterMode.Random;
            }
            else if (sender == btnGenerateSystematicPolynoms)
            {
                m_InputControlPolynomRange.Polynom = new SecondDegreePolynomSystematic();
                m_PolynomRangeExecuter.PolynomRangeExecuterMode = m_InputControlPolynomRange.PolynomRangeExecuterMode = PolynomRangeExecuterMode.Systematic;
            }

            SetInputControl(m_InputControlPolynomRange);
        }

        private void btnHelp_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OnlineHelpActions helpaction = OnlineHelpActions.None;

            if (sender == btnHelpGenerateRandom)
            {
                helpaction = OnlineHelpActions.Generation_Random;
            }
            else if (sender == btnHelpGenerateNTimesM)
            {
                helpaction = OnlineHelpActions.Generation_RandomNTimesM;
            }
            else if (sender == btnHelpGenerateQuadratic)
            {
                helpaction = OnlineHelpActions.Generation_GenerateQuadratic;
            }
            else if (sender == btnHelpQuadratic)
            {
                helpaction = OnlineHelpActions.Generation_Quadratic;
            }
            else if (sender == btnHelpGenerateEuler1)
            {
                helpaction = OnlineHelpActions.Generation_GenerateEuler1;
            }
            else if (sender == btnHelpGenerateQuadraticRandom)
            {
                helpaction = OnlineHelpActions.Generation_GenerateQuadraticRandom;
            }
            else if (sender == btnHelpGenerateQuadraticSystematic)
            {
                helpaction = OnlineHelpActions.Generation_GenerateQuadraticSystematic;
            }

            if (helpaction != OnlineHelpActions.None)
            {
                OnlineHelpAccess.ShowOnlineHelp(helpaction);
            }

            e.Handled = true;
        }

        private void CancelAll()
        {
            m_ExpressionExecuter.Cancel();
            m_PolynomRangeExecuter.Cancel();
        }

        #region IPrimeUserControl Members

        public void SetTab(int i)
        {
        }

        #endregion

        private void tbRandom_Click(object sender, RoutedEventArgs e)
        {
            spRandom.Visibility = (tbRandom.IsChecked.Value) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void tbFormular_Click(object sender, RoutedEventArgs e)
        {
            spFormular.Visibility = svFormular.Visibility = (tbFormular.IsChecked.Value) ? Visibility.Collapsed : Visibility.Visible;
        }

        #region IPrimeUserControl Members

        public event VoidDelegate Execute;

        private void FireExecuteEvent()
        {
            if (Execute != null)
            {
                Execute();
            }
        }

        public event VoidDelegate Stop;

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

        private void gnentimesm_Loaded(object sender, RoutedEventArgs e)
        {
        }
    }
}
