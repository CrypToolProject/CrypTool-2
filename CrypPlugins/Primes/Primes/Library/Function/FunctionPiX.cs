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

namespace Primes.Library.Function
{
    public class FunctionPiX : BaseFunction, IFunction
    {
        #region IFunction Members

        private long m_LastNumber;
        private bool usePrimesCountList;

        public override double FormerValue => (m_Counter == 0) ? double.NaN : m_Counter;

        public FunctionPiX()
        {
            m_FormerValue = 1;
            m_StartAt = 2.0;
            usePrimesCountList = true;
            m_LastNumber = 0;
        }

        private double m_StartAt;

        public double StartAt
        {
            get => m_StartAt;
            set => m_StartAt = value;
        }

        private bool m_ShowIntermediateResult;

        public bool ShowIntermediateResult
        {
            get => m_ShowIntermediateResult;
            set => m_ShowIntermediateResult = value;
        }

        private long m_Counter = 0;

        private (long input, long output) lastCalc;

        public double Execute(double input)
        {
            long value = (long)input;
            if (usePrimesCountList)
            {
                usePrimesCountList = PrimesCountList.Initialzed;
            }

            if (PrimesCountList.Initialzed && usePrimesCountList)
            {
                if (value <= PrimesCountList.MaxNumber)
                {
                    m_FormerValue = m_Counter;
                    m_Counter = PrimesCountList.GetPrime((long)input);
                    m_LastNumber = value;
                }
                else
                {
                    m_Counter = PrimesCountList.GetPrime(PrimesCountList.MaxNumber);
                    m_LastNumber = PrimesCountList.MaxNumber;
                    usePrimesCountList = false;
                }
            }
            else
            {
                long i = value / 100000;
                if (i < PrimeNumbers.numberofprimes.Length)
                {
                    m_Counter = PrimeNumbers.numberofprimes[i];
                    m_LastNumber = i * 100000;
                }
            }

            if (PrimesBigInteger.ValueOf(m_LastNumber).IsPrime(10))
            {
                m_LastNumber++;
            }

            if (m_LastNumber < lastCalc.input && value > lastCalc.input)
            {
                //Reuse result of last calculation:
                m_LastNumber = lastCalc.input + 1;
                m_Counter = lastCalc.output;
            }

            while (m_LastNumber < value)
            {
                if (PrimesBigInteger.ValueOf(m_LastNumber).IsPrime(10))
                {
                    m_Counter++;
                    if (m_ShowIntermediateResult && Executed != null)
                    {
                        Executed(m_Counter);
                    }
                }
                m_LastNumber++;
            }
            if (PrimesBigInteger.ValueOf(value).IsPrime(10) && !usePrimesCountList)
            {
                m_FormerValue = m_Counter;
                m_Counter++;
            }
            m_LastNumber = value;
            if (Executed != null)
            {
                Executed(m_Counter);
            }

            lastCalc = (value, m_Counter);
            return m_Counter;
        }

        #endregion

        #region IFunction Members

        public void Reset()
        {
            m_FormerValue = 1;
            m_Counter = 0;
            usePrimesCountList = true;
            m_LastNumber = 0;
        }

        #endregion

        #region IFunction Members

        public bool CanEstimate => false;

        private FunctionState m_FunctionState;

        public FunctionState FunctionState
        {
            get => m_FunctionState;
            set => m_FunctionState = value;
        }

        #endregion

        #region IFunction Members

        public double MaxValue
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        #endregion

        #region IFunction Members

        public event ObjectParameterDelegate Executed;

        public double DrawTo => double.PositiveInfinity;

        #endregion
    }
}
