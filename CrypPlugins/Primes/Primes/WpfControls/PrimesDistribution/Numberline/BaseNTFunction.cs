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
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;

namespace Primes.WpfControls.PrimesDistribution.Numberline
{
    public abstract class BaseNTFunction : INTFunction
    {
        public BaseNTFunction(LogControl2 lc, TextBlock tbCalcInfo)
        {
            m_Log = lc;
            m_tbCalcInfo = tbCalcInfo;
        }

        #region Properties

        protected Thread m_Thread;
        protected PrimesBigInteger m_Value;
        protected Dictionary<PrimesBigInteger, long> m_Factors;

        #endregion

        #region INTFunction Members

        protected LogControl2 m_Log;

        public Primes.WpfControls.Components.LogControl2 Log
        {
            set => m_Log = value;
        }

        protected TextBlock m_tbCalcInfo;

        public System.Windows.Controls.TextBlock CalcInfo
        {
            set => m_tbCalcInfo = value;
        }

        public virtual void Start(PrimesBigInteger value, Dictionary<PrimesBigInteger, long> factors = null)
        {
            Stop();
            m_Value = value;
            m_Factors = factors;
            m_Log.Clear();
            m_Log.Columns = 1;
            m_Thread = new Thread(new ThreadStart(DoExecute))
            {
                CurrentCulture = Thread.CurrentThread.CurrentCulture,
                CurrentUICulture = Thread.CurrentThread.CurrentUICulture
            };
            m_Thread.Start();
        }

        public virtual void Stop()
        {
            CancelThread();
        }

        protected void CancelThread()
        {
            if (m_Thread != null)
            {
                m_Thread.Abort();
                m_Thread = null;
            }
        }

        protected abstract void DoExecute();

        public event VoidDelegate OnStart;

        public event VoidDelegate OnStop;

        protected void FireOnStart()
        {
            if (OnStart != null)
            {
                OnStart();
            }
        }

        protected void FireOnStop()
        {
            if (OnStop != null)
            {
                OnStop();
            }
        }

        #endregion

        protected void SetCalcInfo(string message)
        {
            ControlHandler.SetPropertyValue(m_tbCalcInfo, "Text", message);
        }

        #region INTFunction Members

        public bool IsRunning => m_Thread != null && m_Thread.ThreadState == System.Threading.ThreadState.Running;

        #endregion
    }
}
