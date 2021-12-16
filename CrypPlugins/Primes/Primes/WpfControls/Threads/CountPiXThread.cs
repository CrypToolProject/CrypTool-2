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
using Primes.WpfControls.PrimesDistribution.Graph;
using System.Windows.Threading;

namespace Primes.WpfControls.Threads
{
    public class CountPiXThread : SuspendableThread
    {
        private readonly FunctionPiX m_FunctionPiX;
        private readonly ObjectParameterDelegate m_objdelegate;
        public event FunctionEvent OnFunctionStart;
        public event FunctionEvent OnFunctionStop;
        private readonly Dispatcher m_Dispatcher;
        private long counter = 0;
        private long n = 2;
        private readonly PrimesBigInteger m_To;

        public CountPiXThread(FunctionPiX functionPiX, Dispatcher dispatcher, ObjectParameterDelegate objdelegate, PrimesBigInteger to)
        {
            m_FunctionPiX = functionPiX;
            m_Dispatcher = dispatcher;
            m_objdelegate = objdelegate;
            m_To = to;
        }

        /*
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
          if (OnStopPiX != null) OnStopPiX();
         */

        protected override void OnDoWork()
        {
            if (OnFunctionStart != null)
            {
                OnFunctionStart(m_FunctionPiX);
            }

            if (m_FunctionPiX != null)
            {
                if (PrimesCountList.Initialzed)
                {
                    n = PrimesCountList.MaxNumber;
                    counter = PrimesCountList.GetPrime(n);
                }
                m_objdelegate(n);
                m_FunctionPiX.FunctionState = FunctionState.Running;
                while (!HasTerminateRequest() && n < m_To.LongValue)
                //for (long i = m_From; i <= fe.Range.To * factor || !HasTerminateRequest(); i += inci)
                {
                    bool awokenByTerminate = SuspendIfNeeded();

                    if (awokenByTerminate)
                    {
                        return;
                    }
                    n++;
                    counter = (long)m_FunctionPiX.Execute(n);
                }
                m_FunctionPiX.Reset();
                m_FunctionPiX.FunctionState = FunctionState.Stopped;
                if (OnFunctionStop != null)
                {
                    OnFunctionStop(m_FunctionPiX);
                }
            }
        }

        public void Abort()
        {
            m_FunctionPiX.FunctionState = FunctionState.Stopped;
            m_FunctionPiX.Reset();
            if (OnFunctionStop != null)
            {
                OnFunctionStop(m_FunctionPiX);
            }

            Thread.Abort();
        }
    }
}
