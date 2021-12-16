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
using System.Numerics;
using System.Windows;
using System.Windows.Controls;

namespace Primes.WpfControls.PrimesDistribution.Numberline
{
    public class EulerPhi : BaseNTFunction
    {
        public EulerPhi(LogControl2 lc, TextBlock tb)
            : base(lc, tb)
        {
        }

        protected override void DoExecute()
        {
            FireOnStart();

            ControlHandler.SetPropertyValue(m_tbCalcInfo, "Visibility", Visibility.Visible);

            if (m_Value.IsPrime(20))
            {
                string info = string.Format(Primes.Resources.lang.WpfControls.Distribution.Distribution.numberline_eulerphiisprime, m_Value, m_Value, m_Value, m_Value.Subtract(PrimesBigInteger.One));
                m_Log.Info(info);
                SetCalcInfo(info);
            }
            else
            {
                PrimesBigInteger phi = (m_Factors != null) ? PrimesBigInteger.Phi(m_Factors) : m_Value.Phi();
                SetCalcInfo(string.Format(Primes.Resources.lang.WpfControls.Distribution.Distribution.numberline_eulerphifoundresult, phi, m_Value));

                BigInteger x = BigInteger.Parse(m_Value.ToString());
                int counter = 0;
                int maxlines = 1000;

                for (BigInteger d = 1; d < x; d++)
                {
                    if (BigInteger.GreatestCommonDivisor(d, x) == 1)
                    {
                        m_Log.Info(d + "   ");
                        if (++counter >= maxlines)
                        {
                            break;
                        }
                    }
                }

                if (counter >= maxlines)
                {
                    m_Log.Info(string.Format(Primes.Resources.lang.WpfControls.Distribution.Distribution.numberline_eulerphimaxlines, maxlines, phi));
                }
            }

            FireOnStop();
        }
    }
}