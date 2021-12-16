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
using System.Collections.Generic;
using System.Threading;

namespace Primes.WpfControls.Threads
{
    public class FindPrimesThread : SuspendableThread
    {
        #region Constructors

        public FindPrimesThread(PrimesBigInteger max, EventWaitHandle finished) :
            base()
        {
            m_Max = max;
            m_Primes = new List<PrimesBigInteger>();
            base.m_Priority = System.Threading.ThreadPriority.Highest;
            m_Finished = finished;
        }

        #endregion

        #region Properties

        private IList<PrimesBigInteger> m_Primes;

        public IList<PrimesBigInteger> Primes
        {
            get => m_Primes;
            set => m_Primes = value;
        }

        private PrimesBigInteger m_Max;

        public PrimesBigInteger Max
        {
            get => m_Max;
            set => m_Max = value;
        }

        private readonly EventWaitHandle m_Finished;

        #endregion

        #region Events

        //public event VoidDelegate Finshed;

        #endregion

        #region Work

        protected override void OnDoWork()
        {
            m_Finished.Reset();
            PrimesBigInteger value = PrimesBigInteger.Two;
            while (value.CompareTo(m_Max) <= 0)
            {
                m_Primes.Add(value);
                value = value.NextProbablePrime();
            }
            m_Finished.Set();
        }

        #endregion
    }
}
