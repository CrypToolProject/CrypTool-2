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

namespace Primes.WpfControls.Validation.Validator
{
    public class BigIntegerMinValueValidator : BigIntegerValidator
    {
        public BigIntegerMinValueValidator(object value, PrimesBigInteger minValue)
            : base(value)
        {
            m_MinValue = minValue;
        }

        public BigIntegerMinValueValidator() : base() { }

        private PrimesBigInteger m_MinValue;

        public PrimesBigInteger MinValue
        {
            get => m_MinValue;
            set => m_MinValue = value;
        }

        public override ValidationResult Validate(ref PrimesBigInteger bi)
        {
            ValidationResult result = base.Validate(ref bi);
            if (result == ValidationResult.OK)
            {
                if (bi.CompareTo(MinValue) < 0)
                {
                    result = ValidationResult.WARNING;
                }
            }

            return result;
        }

        private string m_Message;

        public override string Message
        {
            get
            {
                try
                {
                    if (!string.IsNullOrEmpty(m_Message))
                    {
                        return string.Format(m_Message, MinValue.ToString("N0"));
                    }
                    else
                    {
                        return string.Format(Primes.Resources.lang.Validation.Validation.BigIntegerMinValueValidator, MinValue.ToString("N0"));
                    }
                }
                catch
                {
                    return string.Format(Primes.Resources.lang.Validation.Validation.BigIntegerMinValueValidator, MinValue.ToString("N0"));
                }
            }
            set => m_Message = value;
        }
    }
}
