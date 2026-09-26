/*
   Copyright (C) CrypTool 2 Team

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

namespace Primes.WpfControls.Primetest.SieveOfEratosthenes
{
    public enum StepResult { SUCCESS, FAILED, END }

    public class Step
    {
        private PrimesBigInteger m_Current;

        public PrimesBigInteger Current => m_Current;

        private PrimesBigInteger m_Expected;

        public PrimesBigInteger Expected => m_Expected;

        private readonly PrimesBigInteger m_MaxValue;

        private readonly Numbergrid.Numbergrid m_Numbergrid;

        public Step(Numbergrid.Numbergrid numbergrid, PrimesBigInteger maxValue)
        {
            m_Expected = m_Current = PrimesBigInteger.Two;
            m_Numbergrid = numbergrid;
            m_MaxValue = maxValue;
        }

        public StepResult DoStep(PrimesBigInteger value)
        {
            if (m_Expected.CompareTo(value) == 0)
            {
                m_Numbergrid.RemoveMulipleOf(value);
                m_Expected = m_Expected.NextProbablePrime();
                m_Current = value;
                if (m_Current.Pow(2).CompareTo(m_MaxValue) >= 0)
                {
                    return StepResult.END;
                }

                return StepResult.SUCCESS;
            }
            else
            {
                return StepResult.FAILED;
            }
        }

        public void Reset()
        {
            m_Expected = m_Current = PrimesBigInteger.Two;
        }
    }
}