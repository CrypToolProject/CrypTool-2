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

namespace Primes.WpfControls.Validation.Advisers
{
    public class LargeNumberAdvisor : IValidator<PrimesBigInteger>
    {
        #region Properties

        private readonly PrimesBigInteger m_Limit;
        private readonly OnlineHelp.OnlineHelpActions m_HelpLink;
        private PrimesBigInteger m_Value;

        #endregion

        public LargeNumberAdvisor(PrimesBigInteger limit, OnlineHelp.OnlineHelpActions helplink)
        {
            m_Limit = limit;
            m_HelpLink = helplink;
        }

        #region IValidator<PrimesBigInteger> Members

        public object Value
        {
            get => m_Value;
            set => m_Value = value as PrimesBigInteger;
        }

        public ValidationResult Validate(ref PrimesBigInteger t)
        {
            m_Value = t;
            if (m_Limit.CompareTo(t) < 0)
            {
                return ValidationResult.INFO;
            }
            return ValidationResult.OK;
        }

        public string Message
        {
            get => Primes.Resources.lang.Validation.Validation.largenumberadvisor;
            set { }
        }

        public OnlineHelp.OnlineHelpActions HelpLink => m_HelpLink;

        #endregion
    }
}