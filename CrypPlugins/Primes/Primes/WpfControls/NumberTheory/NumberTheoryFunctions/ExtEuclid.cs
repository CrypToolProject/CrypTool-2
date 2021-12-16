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

using CrypTool.PluginBase.Miscellaneous;
using System;
using System.Numerics;

namespace Primes.WpfControls.NumberTheory.NumberTheoryFunctions
{
    public class ExtEuclid : BaseNTFunction
    {
        private readonly object lockobj = new object();
        public ExtEuclid() : base() { }

        #region Calculating

        protected override void DoExecute()
        {
            FireOnStart();

            for (BigInteger x = m_From; x <= m_To; x++)
            {
                string msg;

                try
                {
                    BigInteger gcd = BigIntegerHelper.ExtEuclid(x, m_SecondParameter, out BigInteger a, out BigInteger b);
                    string sa = a.ToString(); if (a < 0)
                    {
                        sa = "(" + sa + ")";
                    }

                    string sb = b.ToString(); if (b < 0)
                    {
                        sb = "(" + sb + ")";
                    }

                    msg = string.Format("{0}*{1} + {2}*{3} = {4}", sa, x, sb, m_SecondParameter, gcd);
                }
                catch (Exception)
                {
                    msg = "-";
                }

                FireOnMessage(this, x, msg);
            }

            FireOnStop();
        }

        public override string Description => m_ResourceManager.GetString(BaseNTFunction.exteuclid);

        public override bool NeedsSecondParameter => true;

        #endregion
    }
}
