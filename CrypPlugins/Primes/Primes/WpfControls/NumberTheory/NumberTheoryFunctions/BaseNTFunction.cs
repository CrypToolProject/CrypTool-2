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
using System.Numerics;
using System.Resources;
using System.Threading;

namespace Primes.WpfControls.NumberTheory.NumberTheoryFunctions
{
    public abstract class BaseNTFunction : INTFunction
    {
        #region Resources

        protected const string groupbox_choosefunctions = "groupbox_choosefunctions";
        protected const string eulerphi = "eulerphi";
        protected const string eulerphisum = "eulerphisum";
        protected const string eulerphivalues = "eulerphivalues";
        protected const string pix = "pix";
        protected const string rho = "sigma";
        protected const string tau = "tau";
        protected const string tauvalues = "tauvalues";
        protected const string gcd = "gcd";
        protected const string lcm = "lcm";
        protected const string modinv = "modinv";
        protected const string exteuclid = "exteuclid";

        protected static ResourceManager m_ResourceManager;

        static BaseNTFunction()
        {
            m_ResourceManager = new ResourceManager("Primes.Resources.lang.Numbertheory.Numbertheory", typeof(Primes.Resources.lang.Numbertheory.Numbertheory).Assembly);
        }

        #endregion

        public BaseNTFunction()
        {
        }

        #region Properties

        protected Thread m_Thread;
        protected BigInteger m_From;
        protected BigInteger m_To;
        protected BigInteger m_SecondParameter;

        #endregion

        #region INTFunction Members

        public virtual void Start(PrimesBigInteger from, PrimesBigInteger to, PrimesBigInteger second)
        {
            Stop();
            if (from != null)
            {
                m_From = from;
            }

            if (to != null)
            {
                m_To = to;
            }

            if (second != null)
            {
                m_SecondParameter = second;
            }

            m_Thread = new Thread(new ThreadStart(DoExecute))
            {
                CurrentCulture = Thread.CurrentThread.CurrentCulture,
                CurrentUICulture = Thread.CurrentThread.CurrentUICulture
            };
            m_Thread.Start();
            m_IsRunning = true;
        }

        public virtual void Stop()
        {
            m_IsRunning = false;
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
                m_IsRunning = false;
                OnStop();
            }
        }

        public event NumberTheoryMessageDelegate Message;

        protected void FireOnMessage(INTFunction function, PrimesBigInteger value, string message)
        {
            if (Message != null)
            {
                Message(function, value, message);
            }
        }

        #endregion

        #region INTFunction Members

        public virtual string Description => "Base";

        #endregion

        #region INTFunction Members

        private bool m_IsRunning;

        public bool IsRunnung => m_IsRunning;

        public virtual bool NeedsSecondParameter => false;

        #endregion
    }
}