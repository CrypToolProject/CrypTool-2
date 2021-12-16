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
using System;
using System.Collections.Generic;
using System.Text;

namespace Primes.WpfControls.Primegeneration.Function
{
    public class GenerateMDigitPrimes : Primes.WpfControls.Components.IExpression
    {
        public static readonly string LEN = "Lenght";
        private readonly IList<PrimesBigInteger> m_GeneratedPrimes;
        private PrimesBigInteger m_LastPrime;
        private PrimesBigInteger m_Length;
        public event VoidDelegate NonFurtherPrimeFound;

        private readonly Random rnd = new Random();

        public GenerateMDigitPrimes()
        {
            m_GeneratedPrimes = new List<PrimesBigInteger>();
        }

        #region IFunction Members

        public PrimesBigInteger Execute(PrimesBigInteger input)
        {
            int l = m_Length.IntValue;

            // try to find a random prime in the range
            for (int i = 0; i < 2; i++)
            {
                m_LastPrime = GetStartBigInteger();
                if (m_LastPrime.ToString().Length != l)
                {
                    continue;
                }

                if (!m_GeneratedPrimes.Contains(m_LastPrime))
                {
                    m_GeneratedPrimes.Add(m_LastPrime);
                    return m_LastPrime;
                }
            }

            // if that fails, try to find a prime systematically from the start of the range
            StringBuilder r = new StringBuilder("1");
            for (int i = 1; i < l; i++)
            {
                r.Append("0");
            }

            m_LastPrime = new PrimesBigInteger(r.ToString());

            for (int i = 0; i < 1000; i++)
            {
                m_LastPrime = m_LastPrime.NextProbablePrime();
                if (m_LastPrime.ToString().Length != l)
                {
                    break;
                }

                if (!m_GeneratedPrimes.Contains(m_LastPrime))
                {
                    m_GeneratedPrimes.Add(m_LastPrime);
                    return m_LastPrime;
                }
            }

            if (NonFurtherPrimeFound != null)
            {
                NonFurtherPrimeFound();
            }

            return m_LastPrime;
        }

        private PrimesBigInteger GetStartBigInteger()
        {
            StringBuilder r = new StringBuilder("");
            r.Append(1 + rnd.Next(9));
            for (int j = 1; j < m_Length.IntValue; j++)
            {
                r.Append(rnd.Next(10));
            }

            return new PrimesBigInteger(r.ToString()).NextProbablePrime();
        }

        public void SetParameter(string name, PrimesBigInteger value)
        {
            if (name.Equals(LEN))
            {
                m_Length = value;
            }
            else
            {
                throw new ArgumentException("Invalid Name");
            }
        }

        public void Reset()
        {
            m_LastPrime = null;
        }

        #endregion
    }
}
