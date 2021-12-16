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
using System;
using System.Collections.Generic;
using System.Threading;

namespace Primes.WpfControls.Primegeneration.Function
{
    public delegate void ExpressionResultDelegate(PrimesBigInteger result, PrimesBigInteger input);
    public delegate void PolynomeRangeResultDelegate(IPolynom p, PrimesBigInteger primesCount, PrimesBigInteger primesCountReal, PrimesBigInteger count);
    public class ExpressionExecuter
    {
        protected Thread m_Thread;
        public event ExpressionResultDelegate FunctionResult;
        public event VoidDelegate Start;
        public event VoidDelegate Stop;

        private PrimesBigInteger m_From;

        public PrimesBigInteger From
        {
            get => m_From;
            set => m_From = value;
        }

        private PrimesBigInteger m_To;

        public PrimesBigInteger To
        {
            get => m_To;
            set => m_To = value;
        }

        protected Primes.WpfControls.Components.IExpression m_Function;

        internal virtual Primes.WpfControls.Components.IExpression Function
        {
            get => m_Function;
            set => m_Function = value;
        }

        public virtual void Execute(PrimesBigInteger from, PrimesBigInteger to)
        {
            m_To = to;
            m_From = from;
            Execute();
        }

        public virtual void Execute()
        {
            if (m_From == null)
            {
                throw new ArgumentNullException("from");
            }

            if (m_To == null)
            {
                throw new ArgumentNullException("to");
            }

            if (m_From.CompareTo(m_To) > 0)
            {
                throw new ArgumentException("from must be greater than to", "from");
            }

            StartThread();
        }

        private void StartThread()
        {
            Cancel();
            m_Thread = new Thread(new ThreadStart(DoExecute))
            {
                CurrentCulture = Thread.CurrentThread.CurrentCulture,
                CurrentUICulture = Thread.CurrentThread.CurrentUICulture
            };
            m_Thread.Start();
        }

        public void Cancel()
        {
            if (m_Thread != null)
            {
                m_Thread.Abort();
                m_Thread = null;
            }
            if (m_Function != null)
            {
                m_Function.Reset();
            }
        }

        protected virtual void DoExecute()
        {
            if (Start != null)
            {
                Start();
            }

            while (m_From.CompareTo(m_To) <= 0)
            {
                PrimesBigInteger result = m_Function.Execute(m_From);
                if (FunctionResult != null)
                {
                    FunctionResult(result, m_From);
                }

                m_From = m_From.Add(PrimesBigInteger.One);
            }
            if (Stop != null)
            {
                Stop();
            }

            Cancel();
        }
    }

    public class PolynomRangeExecuter : ExpressionExecuter
    {
        #region events

        public new event PolynomeRangeResultDelegate FunctionResult;
        public new event VoidDelegate Start;
        public new event VoidDelegate Stop;

        #endregion

        #region Properties

        private PolynomRangeExecuterMode m_PolynomRangeExecuterMode;

        public PolynomRangeExecuterMode PolynomRangeExecuterMode
        {
            get => m_PolynomRangeExecuterMode;
            set => m_PolynomRangeExecuterMode = value;
        }

        private PrimesBigInteger m_NumberOfCalculations;

        public PrimesBigInteger NumberOfCalculations
        {
            get => m_NumberOfCalculations;
            set => m_NumberOfCalculations = value;
        }

        private PrimesBigInteger m_NumberOfFormulars;

        public PrimesBigInteger NumberOfFormulars
        {
            get => m_NumberOfFormulars;
            set => m_NumberOfFormulars = value;
        }

        private IList<KeyValuePair<string, Range>> m_Parameters;

        public IList<KeyValuePair<string, Range>> Parameters
        {
            get => m_Parameters;
            set => m_Parameters = value;
        }

        #endregion

        #region override ExpressionExecuter

        public override void Execute(PrimesBigInteger from, PrimesBigInteger to)
        {
            Execute();
        }

        public override void Execute()
        {
            if (m_NumberOfCalculations == null)
            {
                throw new ArgumentNullException("NumberOfCalculations");
            }

            if (NumberOfFormulars == null)
            {
                throw new ArgumentNullException("NumberOfFormulars");
            }

            StartThread();
        }

        private void StartThread()
        {
            Cancel();
            m_Thread = new Thread(new ThreadStart(DoExecute))
            {
                CurrentCulture = Thread.CurrentThread.CurrentCulture,
                CurrentUICulture = Thread.CurrentThread.CurrentUICulture
            };
            m_Thread.Start();
        }

        protected override void DoExecute()
        {
            if (Start != null)
            {
                Start();
            }

            if (m_PolynomRangeExecuterMode == PolynomRangeExecuterMode.Random)
            {
                ComputeRandom();
            }
            else
            {
                ComputeSystematic();
            }
            if (Stop != null)
            {
                Stop();
            }

            Cancel();
        }

        //private void ComputeRandom()
        //{
        //  PrimesBigInteger i = PrimesBigInteger.One;
        //  while (i.CompareTo(this.NumberOfFormulars) < 0)
        //  {
        //    PrimesBigInteger j = PrimesBigInteger.One;
        //    PrimesBigInteger to = this.NumberOfCalculations;
        //    if (NumberOfCalculations.Equals(PrimesBigInteger.NaN))
        //    {
        //      j = From;
        //      to = To;
        //    }
        //    PrimesBigInteger counter = PrimesBigInteger.Zero;
        //    PrimesBigInteger primesCounter = PrimesBigInteger.Zero;
        //    while(j.CompareTo(to)<=0){
        //      foreach (KeyValuePair<string, Range> kvp in this.Parameters)
        //      {
        //        PrimesBigInteger value = PrimesBigInteger.RandomM(kvp.Value.From.Add(kvp.Value.RangeAmount)).Add(PrimesBigInteger.One);
        //        Function.SetParameter(kvp.Key, value);
        //      }
        //      PrimesBigInteger input = j;
        //      if (!NumberOfCalculations.Equals(PrimesBigInteger.NaN))
        //      {
        //        input = PrimesBigInteger.RandomM(NumberOfCalculations);
        //      }
        //      PrimesBigInteger res = Function.Execute(input);
        //      if(res.IsPrime(10)){
        //        primesCounter = primesCounter.Add(PrimesBigInteger.One);
        //      }
        //      j = j.Add(PrimesBigInteger.One);
        //      counter = counter.Add(PrimesBigInteger.One);
        //    }
        //    if (this.FunctionResult != null) this.FunctionResult(this.Function as IPolynom, primesCounter, counter);
        //    i = i.Add(PrimesBigInteger.One);
        //  }
        //}

        private void ComputeRandom()
        {
            Random r = new Random();

            PrimesBigInteger i = PrimesBigInteger.One;
            PrimesBigInteger j = null;
            PrimesBigInteger to = null;
            PrimesBigInteger counter = null;
            PrimesBigInteger primesCounter = null;
            IList<PrimesBigInteger> m_PrimeList = new List<PrimesBigInteger>();

            while (i.CompareTo(NumberOfFormulars) <= 0)
            {
                j = PrimesBigInteger.One;
                to = NumberOfCalculations;
                if (NumberOfCalculations.Equals(PrimesBigInteger.NaN))
                {
                    j = From;
                    to = To;
                }
                counter = PrimesBigInteger.Zero;
                primesCounter = PrimesBigInteger.Zero;
                foreach (KeyValuePair<string, Range> kvp in Parameters)
                {
                    PrimesBigInteger mod = kvp.Value.To.Subtract(kvp.Value.From).Add(PrimesBigInteger.One);
                    PrimesBigInteger value = PrimesBigInteger.ValueOf(r.Next(int.MaxValue)).Mod(mod).Add(kvp.Value.From);//PrimesBigInteger.RandomM(kvp.Value.From.Add(kvp.Value.RangeAmount)).Add(PrimesBigInteger.One);
                    Function.SetParameter(kvp.Key, value);
                }
                while (j.CompareTo(to) <= 0)
                {
                    PrimesBigInteger input = j;
                    if (!NumberOfCalculations.Equals(PrimesBigInteger.NaN))
                    {
                        input = PrimesBigInteger.ValueOf(r.Next(int.MaxValue)).Mod(NumberOfCalculations);//PrimesBigInteger.RandomM(NumberOfCalculations);
                    }
                    PrimesBigInteger res = Function.Execute(input);
                    if (res.CompareTo(PrimesBigInteger.Zero) >= 0 && res.IsPrime(10))
                    {
                        if (!m_PrimeList.Contains(res))
                        {
                            m_PrimeList.Add(res);
                        }
                        primesCounter = primesCounter.Add(PrimesBigInteger.One);
                    }
                    j = j.Add(PrimesBigInteger.One);
                    counter = counter.Add(PrimesBigInteger.One);
                }
                if (FunctionResult != null)
                {
                    FunctionResult(Function as IPolynom, primesCounter, PrimesBigInteger.ValueOf(m_PrimeList.Count), counter);
                }

                i = i.Add(PrimesBigInteger.One);
            }
        }

        private void ComputeSystematic()
        {
            Range rangea = Parameters[0].Value;
            Range rangeb = Parameters[1].Value;
            Range rangec = Parameters[2].Value;
            PrimesBigInteger ca = rangea.From;
            PrimesBigInteger counter = PrimesBigInteger.Zero;
            while (ca.CompareTo(rangea.To) <= 0)
            {
                m_Function.SetParameter("a", ca);
                PrimesBigInteger cb = rangeb.From;
                while (cb.CompareTo(rangeb.To) <= 0)
                {
                    m_Function.SetParameter("b", cb);
                    PrimesBigInteger cc = rangec.From;
                    while (cc.CompareTo(rangec.To) <= 0)
                    {
                        m_Function.SetParameter("c", cc);

                        PrimesBigInteger from = From;
                        PrimesBigInteger primesCounter = PrimesBigInteger.Zero;
                        IList<PrimesBigInteger> m_PrimeList = new List<PrimesBigInteger>();
                        while (from.CompareTo(To) <= 0)
                        {
                            PrimesBigInteger res = Function.Execute(from);
                            if (res.IsPrime(10))
                            {
                                if (!m_PrimeList.Contains(res))
                                {
                                    m_PrimeList.Add(res);
                                }
                                primesCounter = primesCounter.Add(PrimesBigInteger.One);
                            }
                            counter = counter.Add(PrimesBigInteger.One);
                            from = from.Add(PrimesBigInteger.One);
                        }
                        if (FunctionResult != null)
                        {
                            FunctionResult(Function as IPolynom, primesCounter, PrimesBigInteger.ValueOf(m_PrimeList.Count), counter);
                        }

                        cc = cc.Add(PrimesBigInteger.One);
                    }
                    cb = cb.Add(PrimesBigInteger.One);
                }
                ca = ca.Add(PrimesBigInteger.One);
            }
        }

        #endregion
    }
}
