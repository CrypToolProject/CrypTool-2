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
using Primes.WpfControls.Primetest;
using Primes.WpfControls.Validation;
using Primes.WpfControls.Validation.Validator;
using System.Windows.Controls;

namespace Primes.WpfControls.Primegeneration.SieveOfAtkin
{
    /// <summary>
    /// Interaction logic for SieveOfAtkinControl.xaml
    /// </summary>
    public partial class SieveOfAtkinControl : UserControl, IPrimeTest
    {
        public SieveOfAtkinControl()
        {
            InitializeComponent();
            InitInputSingleControl();
            soa.Cancel += new Primes.Library.VoidDelegate(FireCancelEvent);
            soa.Start += new Primes.Library.VoidDelegate(FireStartEvent);
            soa.Stop += new Primes.Library.VoidDelegate(FireStopEvent);
        }

        private void InitInputSingleControl()
        {
            //InputValidator<PrimesBigInteger> iv = new InputValidator<PrimesBigInteger>();
            //iv.Validator = new BigIntegerMinValueMaxValueValidator(null,PrimesBigInteger.Seven,PrimesBigInteger.ValueOf(10000));
            //isc.AddInputValidator(InputSingleControl.Free, iv);
        }

        #region IPrimeMethodDivision Members

        public void Dispose()
        {
            CancelExecute();
        }

        public void Init()
        {
        }

        public void SetTab(int i)
        {
        }

        #endregion

        #region IPrimeTest Members

        public IValidator<PrimesBigInteger> Validator => new BigIntegerMinValueMaxValueValidator(null, PrimesBigInteger.Seven, PrimesBigInteger.ValueOf(10000));

        public bool IsRunning()
        {
            return soa.IsRunning();
        }

        #endregion

        #region IPrimeVisualization Members

        public event Primes.Library.VoidDelegate Start;

        private void FireStartEvent()
        {
            if (Start != null)
            {
                Start();
            }
        }

        public event Primes.Library.VoidDelegate Stop;

        private void FireStopEvent()
        {
            if (Stop != null)
            {
                Stop();
            }
        }

        public event Primes.Library.VoidDelegate Cancel;

        private void FireCancelEvent()
        {
            if (Cancel != null)
            {
                Cancel();
            }
        }

        public event Primes.Library.CallbackDelegateGetInteger ForceGetInteger;

        private void FireForceGetInteger()
        {
            if (ForceGetInteger != null)
            {
                ForceGetInteger(new Primes.Library.ExecuteIntegerDelegate(Execute));
            }
        }

        public event Primes.Library.CallbackDelegateGetInteger ForceGetIntegerInterval;

        private void FireForceGetIntegerInterval()
        {
            if (ForceGetIntegerInterval != null)
            {
                ForceGetIntegerInterval(new Primes.Library.ExecuteIntegerDelegate(Execute));
            }
        }

        public void Execute(PrimesBigInteger value)
        {
            soa.Execute(value);
        }

        public void CancelExecute()
        {
            soa.CancelSieve();
        }

        public void Execute(PrimesBigInteger from, PrimesBigInteger to)
        {
        }

        #endregion
    }
}
