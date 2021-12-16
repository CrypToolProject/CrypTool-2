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
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace Primes.WpfControls.Factorization.Rho
{
    /// <summary>
    /// Interaction logic for RhoControl.xaml
    /// </summary>
    public partial class RhoControl : UserControl, IFactorizer
    {
        private PrimesBigInteger m_A;
        private PrimesBigInteger m_StartFX;
        private PrimesBigInteger m_Value;
        private Thread m_Thread;
        private readonly IDictionary<PrimesBigInteger, PrimesBigInteger> m_Factors;
        private readonly IDictionary<PrimesBigInteger, int> m_FactorsTmp;

        public RhoControl()
        {
            InitializeComponent();
            InputValidator<PrimesBigInteger> validatorStartX = new InputValidator<PrimesBigInteger>
            {
                Validator = new BigIntegerMinValueMaxValueValidator(null, PrimesBigInteger.ValueOf(1), PrimesBigInteger.ValueOf(1000)),
                LinkOnlinehelp = Primes.OnlineHelp.OnlineHelpActions.Factorization_BruteForce
            };

            startfx.AddInputValidator(InputSingleControl.Free, validatorStartX);

            InputValidator<PrimesBigInteger> validatorA = new InputValidator<PrimesBigInteger>
            {
                Validator = new BigIntegerMinValueMaxValueValidator(null, PrimesBigInteger.ValueOf(1), PrimesBigInteger.ValueOf(1000)),
                LinkOnlinehelp = Primes.OnlineHelp.OnlineHelpActions.Factorization_Rho
            };
            a.AddInputValidator(InputSingleControl.Free, validatorA);
            a.SetText(InputSingleControl.Free, "2");
            startfx.SetText(InputSingleControl.Free, "23");

            //log.Columns = 1;
            a.Execute += new ExecuteSingleDelegate(ForceGetValue_Execute);
            startfx.Execute += new ExecuteSingleDelegate(ForceGetValue_Execute);
            m_Factors = new Dictionary<PrimesBigInteger, PrimesBigInteger>();
            m_FactorsTmp = new Dictionary<PrimesBigInteger, int>();
        }

        private void ForceGetValue_Execute(PrimesBigInteger value)
        {
            FireEventForceGetValue();
        }

        private void FireEventForceGetValue()
        {
            if (ForceGetInteger != null)
            {
                ForceGetInteger(new ExecuteIntegerDelegate(Execute));
            }
        }

        #region IFactorizer Members

        public void Execute(PrimesBigInteger from, PrimesBigInteger to) { }

        public void Execute(PrimesBigInteger value)
        {
            m_Value = value;
            m_A = a.GetValue();
            m_StartFX = startfx.GetValue();

            if (m_A != null && m_StartFX != null)
            {
                CancelThread();
                log.Clear();
                log.Columns = 1;
                m_Thread = new Thread(new ThreadStart(new VoidDelegate(Factorize)))
                {
                    CurrentCulture = Thread.CurrentThread.CurrentCulture,
                    CurrentUICulture = Thread.CurrentThread.CurrentUICulture
                };
                m_Thread.Start();
            }
        }

        public bool isRunning
        {
            get
            {
                if (m_Thread == null)
                {
                    return false;
                }

                return m_Thread.IsAlive;
            }
        }

        private void Factorize()
        {
            m_Factors.Clear();
            FireStartEvent();
            DoFactorize(m_Value);
            FireStopEvent();
        }

        private void DoFactorize(PrimesBigInteger value)
        {
            /*
             *        if (N.compareTo(ONE) == 0) return;
              if (N.isProbablePrime(20)) { System.out.println(N); return; }
              BigInteger divisor = rho(N);
              factor(divisor);
              factor(N.divide(divisor));
             */
            if (value.Equals(PrimesBigInteger.One))
            {
                return;
            }
            if (value.IsProbablePrime(10))
            {
                if (!m_Factors.ContainsKey(value))
                {
                    m_Factors.Add(value, PrimesBigInteger.Zero);
                }

                PrimesBigInteger tmp = m_Factors[value];
                m_Factors[value] = tmp.Add(PrimesBigInteger.One);

                if (FoundFactor != null)
                {
                    FoundFactor(m_Factors.GetEnumerator());
                }

                log.Info(value.ToString());
                return;
            }
            else
            {
                if (!m_FactorsTmp.ContainsKey(value))
                {
                    m_FactorsTmp.Add(value, 0);
                }

                m_FactorsTmp[value]++;
                if (m_FactorsTmp[value] > 3)
                {
                    log.Info(value.ToString() + " Zu oft");
                    m_A = PrimesBigInteger.RandomM(value).Add(PrimesBigInteger.Two);
                    m_FactorsTmp.Remove(value);
                }
            }
            PrimesBigInteger div = CalculateFactor(value);
            DoFactorize(div);
            DoFactorize(value.Divide(div));
        }

        private PrimesBigInteger CalculateFactor(PrimesBigInteger value)
        {
            PrimesBigInteger x = m_StartFX;
            PrimesBigInteger y = m_StartFX;
            PrimesBigInteger d = PrimesBigInteger.One;
            PrimesBigInteger a = m_A;
            int i = 0;
            if (value.Mod(PrimesBigInteger.Two).Equals(PrimesBigInteger.Zero))
            {
                return PrimesBigInteger.Two;
            }

            do
            {
                x = x.ModPow(PrimesBigInteger.Two, value).Add(a).Mod(value);
                y = y.ModPow(PrimesBigInteger.Two, value).Add(a).Mod(value);
                y = y.ModPow(PrimesBigInteger.Two, value).Add(a).Mod(value);
                d = PrimesBigInteger.GCD(x.Subtract(y), value);
                i++;
                if (y.Equals(x))
                {
                    log.Info("Change Values");
                    a = PrimesBigInteger.ValueOf(new Random().Next());
                    x = y = PrimesBigInteger.ValueOf(new Random().Next());
                    i = 0;
                }
            }
            while (d.Equals(PrimesBigInteger.One));
            return d;
        }

        public void CancelFactorization()
        {
            CancelThread();
            m_Factors.Clear();

            FireCancelEvent();
        }

        private void CancelThread()
        {
            if (m_Thread != null)
            {
                m_Thread.Abort();
                m_Thread = null;
            }
        }

        public event FoundFactor FoundFactor;

        public event VoidDelegate Start;

        public event VoidDelegate Stop;

        public event VoidDelegate Cancel;

        private void FireStartEvent()
        {
            if (Start != null)
            {
                Start();
            }
        }

        private void FireStopEvent()
        {
            if (Stop != null)
            {
                Stop();
            }
        }

        private void FireCancelEvent()
        {
            if (Cancel != null)
            {
                Cancel();
            }
        }

        public void CancelExecute()
        {
            //throw new NotImplementedException();
        }

        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //startfx.FreeText = "42";
            //a.FreeText = "23";
        }

        #region IFactorizer Members

        public TimeSpan Needs => throw new NotImplementedException();

        #endregion

        public event CallbackDelegateGetInteger ForceGetInteger;

        private void FireForceGetInteger()
        {
            ForceGetInteger(null);
        }

        public event CallbackDelegateGetInteger ForceGetIntegerInterval;

        private void FireForceGetIntegerInterval()
        {
            ForceGetIntegerInterval(null);
        }

        public IValidator<PrimesBigInteger> Validator => throw new NotImplementedException();
    }
}
