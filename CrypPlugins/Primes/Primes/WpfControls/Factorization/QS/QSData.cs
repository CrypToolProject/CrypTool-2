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
using System;
using System.Collections.Generic;

namespace Primes.WpfControls.Factorization.QS
{
    public enum QuadraticStatus { Nope, Part, Quadratic, Ignore }

    public class QSData : List<QuadraticPair>, IList<QuadraticPair>
    {

        public QSData()
        {
            m_IgnoreQuadrats = new List<PrimesBigInteger>();
        }

        #region Properties

        public IList<int> CalculateFactorBase()
        {
            List<int> result = new List<int>();
            PrimesBigInteger b = PrimesBigInteger.ValueOf(m_B);
            PrimesBigInteger c = PrimesBigInteger.Two;
            while (c.CompareTo(b) <= 0)
            {
                result.Add(c.IntValue);
                c = c.NextProbablePrime();
            }
            return result;
        }

        public IList<QuadraticPair> BSmooth
        {
            get
            {
                List<QuadraticPair> result = new List<QuadraticPair>();
                foreach (QuadraticPair pair in this)
                {
                    if (pair.IsBSmooth)
                    {
                        result.Add(pair);
                    }
                }

                return result;
            }
        }

        private long m_B;   // upper limit for the factor base primes

        public long B => m_B;

        private long m_N;   // the number to factor
        private double m_L; // size of the sieving interval
        private int m_From;
        private int m_To;

        public long N
        {
            get => m_N;
            set
            {
                m_N = value;
                double lnn = Math.Log(m_N);
                double lnlnn = Math.Log(lnn);
                m_L = Math.Exp(Math.Sqrt(lnlnn * lnn));
                m_B = (long)Math.Floor(Math.Sqrt(m_L));
                m_From = (int)Math.Ceiling(Math.Sqrt(m_N) - m_L);
                //m_From = Math.Max(m_From, 0);
                m_To = (int)Math.Floor(Math.Sqrt(m_N) + m_L);
            }
        }

        public int From => m_From;

        public int To => m_To;

        #endregion

        private readonly IList<PrimesBigInteger> m_IgnoreQuadrats;

        public void AddIgnoreQuadrat(PrimesBigInteger value)
        {
            m_IgnoreQuadrats.Add(value);
        }

        public bool IsIgnored(PrimesBigInteger value)
        {
            return m_IgnoreQuadrats.Contains(value);
        }

        public void ClearAll()
        {
            base.Clear();
            m_IgnoreQuadrats.Clear();
            N = 0;
        }

        public void Reset()
        {
            Clear();
            m_IgnoreQuadrats.Clear();
        }
    }

    public class QuadraticPair
    {
        public QuadraticPair()
        {
            m_Exponents = new Dictionary<int, int>();
        }

        public QuadraticPair(long a, long b)
            : this()
        {
            A = a;
            B = b;
        }

        #region Properties

        private long m_A;

        public long A
        {
            get => m_A;
            set => m_A = value;
        }

        private long m_B;

        public long B
        {
            get => m_B;
            set => m_B = value;
        }

        private bool m_IsBSmooth;

        public bool IsBSmooth
        {
            get => m_IsBSmooth;
            set => m_IsBSmooth = value;
        }

        private QuadraticStatus m_QuadraticStatus;

        public QuadraticStatus QuadraticStatus
        {
            get => m_QuadraticStatus;
            set => m_QuadraticStatus = value;
        }

        #endregion

        private readonly IDictionary<int, int> m_Exponents;

        public void AddExponent(int factor, int exp)
        {
            if (!m_Exponents.ContainsKey(factor))
            {
                m_Exponents.Add(factor, exp);
            }
            else
            {
                m_Exponents[factor] = exp;
            }
        }

        internal void Reset()
        {
            m_Exponents.Clear();
            IsBSmooth = false;
        }
    }
}
