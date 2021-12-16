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
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Primes.WpfControls.PrimesDistribution.Numberline
{
    public class EulerPhiSum : BaseNTFunction
    {
        public EulerPhiSum(LogControl2 lc, TextBlock tb)
            : base(lc, tb)
        {
            m_Log.OverrideText = true;
        }

        protected override void DoExecute()
        {
            FireOnStart();

            ControlHandler.SetPropertyValue(m_tbCalcInfo, "Visibility", Visibility.Visible);
            StringBuilder sb = new StringBuilder();

            Dictionary<PrimesBigInteger, long> factors = (m_Factors != null) ? m_Factors : m_Value.Factorize();

            Dictionary<PrimesBigInteger, long> f = new Dictionary<PrimesBigInteger, long>();
            List<PrimesBigInteger> keys = factors.Keys.ToList();
            foreach (PrimesBigInteger key in keys)
            {
                f[key] = 0;
            }

            Dictionary<PrimesBigInteger, PrimesBigInteger> result = new Dictionary<PrimesBigInteger, PrimesBigInteger>();
            PrimesBigInteger sum = PrimesBigInteger.Zero;

            int i;
            do
            {
                PrimesBigInteger phi = PrimesBigInteger.Phi(f);
                result[PrimesBigInteger.Refactor(f)] = phi;
                sum = sum.Add(phi);
                for (i = keys.Count - 1; i >= 0; i--)
                {
                    f[keys[i]]++;
                    if (f[keys[i]] <= factors[keys[i]])
                    {
                        break;
                    }
                }
                for (int j = i + 1; j < keys.Count; j++)
                {
                    f[keys[j]] = 0;
                }
            }
            while (i >= 0);

            SetCalcInfo(string.Format(Primes.Resources.lang.WpfControls.Distribution.Distribution.numberline_eulerphisuminfo, m_Value, sum));

            List<PrimesBigInteger> philist = result.Keys.Select(k => k).ToList();
            philist.Sort(PrimesBigInteger.Compare);
            foreach (PrimesBigInteger k in philist)
            {
                m_Log.Info(string.Format(Primes.Resources.lang.WpfControls.Distribution.Distribution.numberline_eulerphisumlog, k, result[k]));
            }

            string s = string.Join(" + ", philist.Select(k => string.Format("φ({0})", k)));
            m_Log.Info(s + " = " + sum);

            FireOnStop();
        }
    }
}
