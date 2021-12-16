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

namespace Primes.WpfControls.Validation
{
    public class BigIntegerValidator : IValidator<PrimesBigInteger>
    {
        private object m_Value;
        protected SevenZ.Calculator.Calculator calculator;

        public BigIntegerValidator(object value)
            : this()
        {
            m_Value = value;
        }

        public BigIntegerValidator()
        {
            m_Value = string.Empty;
            calculator = new SevenZ.Calculator.Calculator();
        }

        #region IValidator Members

        public virtual ValidationResult Validate(ref PrimesBigInteger bi)
        {
            if (m_Value != null)
            {
                if (m_Value.GetType() == typeof(string)
                && !string.IsNullOrEmpty(m_Value.ToString().Trim()))
                {
                    try
                    {
                        bi = calculator.Evaluate(m_Value.ToString());
                        return ValidationResult.OK;
                    }
                    catch
                    {
                        if (string.IsNullOrEmpty(m_Message))
                        {
                            m_Message = Primes.Resources.lang.Validation.Validation.BigIntegerValidator;
                        }
                    }
                }
                else
                {
                    if (m_Value.GetType() == typeof(PrimesBigInteger))
                    {
                        bi = m_Value as PrimesBigInteger;
                        return ValidationResult.OK;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(m_Message))
                        {
                            m_Message = Primes.Resources.lang.Validation.Validation.BigIntegerValidator;
                        }
                    }
                }
            }

            return ValidationResult.WARNING;
        }

        private string m_Message = string.Empty;

        public virtual string Message
        {
            get => m_Message;
            set => m_Message = value;
        }

        #endregion

        #region IValidator<BigInteger> Members

        public object Value
        {
            get => m_Value;
            set => m_Value = value;
        }

        #endregion

        #region IValidator<PrimesBigInteger> Members

        public OnlineHelp.OnlineHelpActions HelpLink => OnlineHelp.OnlineHelpActions.None;

        #endregion
    }
}
