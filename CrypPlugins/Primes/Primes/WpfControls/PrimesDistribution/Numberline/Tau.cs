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
using System.Windows;
using System.Windows.Controls;

namespace Primes.WpfControls.PrimesDistribution.Numberline
{
    public class Tau : BaseNTFunction
    {
        public Tau(LogControl2 lc, TextBlock tb)
            : base(lc, tb)
        {
        }

        protected override void DoExecute()
        {
            FireOnStart();

            List<PrimesBigInteger> divisors = (m_Factors != null) ? PrimesBigInteger.Divisors(m_Factors) : m_Value.Divisors();
            divisors.Sort(PrimesBigInteger.Compare);

            ControlHandler.SetPropertyValue(m_tbCalcInfo, "Visibility", Visibility.Visible);
            SetCalcInfo(string.Format(Primes.Resources.lang.WpfControls.Distribution.Distribution.numberline_tauinfo, divisors.Count, m_Value));

            foreach (PrimesBigInteger d in divisors)
            {
                m_Log.Info(d + "   ");
            }

            FireOnStop();
        }
    }
}