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

namespace Primes.Library
{
    public class RangeY : Range
    {
        public RangeY(PrimesBigInteger from, PrimesBigInteger to)
            : base(from, to)
        {
        }

        public override PrimesBigInteger GetZeroPosition()
        {
            PrimesBigInteger result = RangeAmount;
            if (From.CompareTo(PrimesBigInteger.Zero) < 0)
            {
                result = RangeAmount.Add(From);
            }
            return result;
        }
    }
}
