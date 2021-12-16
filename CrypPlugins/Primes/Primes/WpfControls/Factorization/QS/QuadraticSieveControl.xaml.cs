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
using Primes.WpfControls.Validation.Validator;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Primes.WpfControls.Factorization.QS
{
    /// <summary>
    /// Interaction logic for QuadraticSieveControl.xaml
    /// </summary>
    public partial class QuadraticSieveControl : UserControl, IFactorizer
    {
        private Thread m_Thread;
        private long m_Value;
        private readonly long m_Count;

        private readonly IQSStep m_Step1;
        private readonly IQSStep m_Step2;
        private readonly IQSStep m_Step3;
        private readonly IQSStep m_Step4;

        private QSResult m_State;

        private QSData m_Data;

        private readonly ManualResetEvent resetEvent;
        private readonly IDictionary<PrimesBigInteger, PrimesBigInteger> m_Factors;

        private readonly bool stepwise;

        public PrimesBigInteger MinValue = 4;
        public PrimesBigInteger MaxValue = 10000;

        public QuadraticSieveControl()
        {
            InitializeComponent();

            m_State = QSResult.Ok;

            stepwise = false;
            resetEvent = new ManualResetEvent(false);
            m_Count = 10;
            m_Step1 = new Step1(gridFirstStep, lblInfoStep1);

            m_Step2 = new Step2(gridSecondStep, lblInfoStep2);
            m_Step2.PreStep();

            m_Step3 = new Step3(gridThirdStep, lblInfoStep3);
            m_Step3.PreStep();

            m_Step4 = new Step4(gridFourthStep, lblInfoStep4);
            m_Step4.FoundFactor += new FoundFactor(m_Step4_FoundFactor);
            m_Step4.PreStep();

            m_Data = new QSData();
            m_Factors = new Dictionary<PrimesBigInteger, PrimesBigInteger>();
            //m_Data.From = -41;
            //m_Data.To = 41;
        }

        #region IFactorizer Members

        public void Execute(PrimesBigInteger from, PrimesBigInteger to) { }

        public void Execute(PrimesBigInteger value)
        {
            m_Value = value.LongValue;
            m_Data.N = m_Value;

            StartThread();
        }

        public void CancelFactorization()
        {
            FireOnCancel();
            CancelThread();
        }

        private void StartThread()
        {
            StartThread(ExecuteFactorization);
        }

        private void StartThread(VoidDelegate _del)
        {
            m_Thread = new Thread(new ThreadStart(_del))
            {
                CurrentCulture = Thread.CurrentThread.CurrentCulture,
                CurrentUICulture = Thread.CurrentThread.CurrentUICulture
            };
            m_Thread.Start();
        }

        public bool isRunning
        {
            get
            {
                if (m_State != QSResult.Ok)
                {
                    return true;
                }

                if (m_Thread == null)
                {
                    return false;
                }

                return m_Thread.IsAlive;
            }
        }

        #region Factorize

        private void ExecuteFactorization()
        {
            try
            {
                FireOnStart();
                DoExecute();
            }
            catch
            {
            }
        }

        private void ResetExpandersAndResumeButtons()
        {
            ControlHandler.SetPropertyValue(btnRestart_Step4, "Visibility", Visibility.Collapsed);
            ControlHandler.SetPropertyValue(expFirst, "IsExpanded", false);
            ControlHandler.SetPropertyValue(expSecond, "IsExpanded", false);
            ControlHandler.SetPropertyValue(expThird, "IsExpanded", false);
            ControlHandler.SetPropertyValue(expFourth, "IsExpanded", false);
        }

        private void WaitStepWise()
        {
            if (stepwise)
            {
                resetEvent.Reset();
                resetEvent.WaitOne();
            }
        }

        public void Reset()
        {
            m_Data.Reset();
            m_Factors.Clear();

            ResetExpandersAndResumeButtons();
        }

        private void DoExecute()
        {
            m_State = QSResult.Ok;

            Reset();

            ControlHandler.SetPropertyValue(expFirst, "IsExpanded", true);
            m_Step1.PreStep();
            m_Step1.Execute(ref m_Data);
            m_Step1.PostStep();

            WaitStepWise();

            ControlHandler.SetPropertyValue(expSecond, "IsExpanded", true);
            m_Step2.PreStep();
            m_Step2.Execute(ref m_Data);
            m_Step2.PostStep();

            WaitStepWise();

            ExecuteStep3();
        }

        private void ExecuteStep3()
        {
            m_State = QSResult.Ok;

            ControlHandler.SetPropertyValue(expThird, "IsExpanded", true);
            m_Step3.PreStep();
            m_Step3.Execute(ref m_Data);
            m_Step3.PostStep();
            WaitStepWise();

            ControlHandler.SetPropertyValue(expFourth, "IsExpanded", true);
            ControlHandler.SetPropertyValue(btnRestart_Step4, "Visibility", Visibility.Hidden);
            ControlHandler.SetPropertyValue(btnRestart, "Visibility", Visibility.Hidden);

            m_Step4.PreStep();
            m_State = m_Step4.Execute(ref m_Data);
            m_Step4.PostStep();

            switch (m_State)
            {
                case QSResult.Ok:
                    break;
                case QSResult.Failed:
                    ControlHandler.SetPropertyValue(btnRestart_Step4, "Visibility", Visibility.Visible);
                    break;
                case QSResult.Restart:
                    ControlHandler.SetPropertyValue(btnRestart, "Visibility", Visibility.Visible);
                    break;
                default:
                    break;
            }

            if (m_State == QSResult.Ok)
            {
                FireOnStop();
            }
        }

        #region Step1

        private void Step1()
        {
            long sqrt = (long)Math.Floor(Math.Sqrt(m_Value)) + 1;
            ControlHandler.SetPropertyValue(
              lblInfoStep1,
              "Text",
              string.Format(Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step1_squareroot, new object[] { m_Value.ToString("D"), Math.Sqrt(m_Value).ToString("N"), sqrt }));
            int counter = 0;

            ControlHandler.ExecuteMethod(this, "PrepareStep1");

            for (long i = Math.Min(m_Count, 0); i <= Math.Abs(m_Count); i++)
            {
                ControlHandler.AddRowDefintion(gridFirstStep, 1, GridUnitType.Auto);
                string a = (sqrt + counter).ToString("N") + "²";
                string aminus1 = (Math.Pow((sqrt + counter), 2) - m_Value).ToString("N");

                ControlHandler.ExecuteMethod(
                  this,
                  "AddToGrid",
                  new object[] { gridFirstStep, a, counter + 1, 0, 0, 0 });
                ControlHandler.ExecuteMethod(
                  this,
                  "AddToGrid",
                  new object[] { gridFirstStep, aminus1, counter + 1, 1, 0, 0 });

                counter++;
            }
        }

        public void PrepareStep1()
        {
            gridFirstStep.RowDefinitions.Clear();
            gridFirstStep.Children.Clear();
            RowDefinition rd = new RowDefinition
            {
                Height = new GridLength(1, GridUnitType.Auto)
            };
            gridFirstStep.RowDefinitions.Add(rd);

            TextBlock tbA = new TextBlock
            {
                Text = "a²",
                Margin = new Thickness(5)
            };

            TextBlock tbAMinusN = new TextBlock
            {
                Text = "a²-n",
                Margin = new Thickness(5)
            };

            Grid.SetColumn(tbA, 0);
            Grid.SetRow(tbA, 0);
            gridFirstStep.Children.Add(tbA);
            Grid.SetColumn(tbAMinusN, 1);
            Grid.SetRow(tbAMinusN, 0);
            gridFirstStep.Children.Add(tbAMinusN);
        }

        public void AddToGrid(Grid g, string text, int row, int col, int rowspan, int columnspan)
        {
            if (!string.IsNullOrEmpty(text))
            {
                TextBlock tb = new TextBlock
                {
                    Text = text,
                    Margin = new Thickness(5)
                };
                if (columnspan > 0)
                {
                    Grid.SetColumnSpan(tb, columnspan);
                }

                if (rowspan > 0)
                {
                    Grid.SetRowSpan(tb, rowspan);
                }

                Grid.SetColumn(tb, col);
                Grid.SetRow(tb, row);

                g.Children.Add(tb);
            }
        }

        #endregion

        #endregion

        private void CancelThread()
        {
            if (m_Thread != null)
            {
                m_Thread.Abort();
                m_Thread = null;
            }
        }

        #region Events

        public event FoundFactor FoundFactor;

        public event VoidDelegate Start;
        public event VoidDelegate Cancel;
        public event VoidDelegate Stop;

        private void FireOnStart()
        {
            if (Start != null)
            {
                Start();
            }
        }

        private void FireOnStop()
        {
            if (Stop != null)
            {
                Stop();
            }
        }

        private void FireOnCancel()
        {
            if (Cancel != null)
            {
                Cancel();
            }
        }

        private void FireFoundFactorEvent(object o)
        {
            if (FoundFactor != null)
            {
                FoundFactor(o);
            }
        }

        private void m_Step4_FoundFactor(object o)
        {
            if (o != null && o.GetType() == typeof(PrimesBigInteger))
            {
                PrimesBigInteger value = o as PrimesBigInteger;
                if (!m_Factors.ContainsKey(value))
                {
                    m_Factors.Add(value, PrimesBigInteger.Zero);
                }

                PrimesBigInteger tmp = m_Factors[value];
                m_Factors[value] = tmp.Add(PrimesBigInteger.One);
            }
            FireFoundFactorEvent(m_Factors.GetEnumerator());
        }

        #endregion

        public void CancelExecute()
        {
            CancelThread();
        }

        public event CallbackDelegateGetInteger ForceGetInteger;

        private void FireForceGetInteger()
        {
            ForceGetInteger(null);
        }

        #endregion

        private void exp_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender.GetType() == typeof(Expander))
            {
                (sender as Expander).IsExpanded = !((sender as Expander).IsExpanded);
            }
        }

        #region IFactorizer Members

        public TimeSpan Needs => new TimeSpan(0);

        #endregion

        private void btnRestart_Click(object sender, RoutedEventArgs e)
        {
            StartThread(ExecuteStep3);
        }

        private void btnResume_Step1_Click(object sender, RoutedEventArgs e)
        {
            resetEvent.Set();
        }

        #region IPrimeVisualization Members

        public event CallbackDelegateGetInteger ForceGetIntegerInterval;

        private void FireForceGetIntegerInterval()
        {
            ForceGetIntegerInterval(null);
        }

        #endregion

        private void btnRestart_Click_1(object sender, RoutedEventArgs e)
        {
            StartThread(DoExecute);
        }

        #region IFactorizer Members

        public Primes.WpfControls.Validation.IValidator<PrimesBigInteger> Validator => new BigIntegerMinValueMaxValueValidator(null, MinValue, MaxValue);

        #endregion
    }
}
