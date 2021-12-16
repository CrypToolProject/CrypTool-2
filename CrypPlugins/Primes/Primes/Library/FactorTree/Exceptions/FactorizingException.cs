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
using System;

namespace Primes.Library.FactorTree.Exceptions
{
    public class FactorizingException : Exception
    {
        private readonly string m_Message;

        public FactorizingException()
            : base()
        {
        }

        public FactorizingException(PrimesBigInteger value, PrimesBigInteger maxValue)
            : base()
        {
            m_Message = string.Format("{0} is bigger than MaxValue {1}", new object[] { value.ToString(), maxValue.ToString() });
        }

        public override string Message => m_Message;
    }
}
