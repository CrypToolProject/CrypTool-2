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

using System;
using Primes.Bignum;

namespace Primes.Library
{
    public class Range
    {
        private PrimesBigInteger m_From;

        public PrimesBigInteger From
        {
            get { return m_From; }
            set { m_From = value; }
        }

        private PrimesBigInteger m_To;

        public PrimesBigInteger To
        {
            get { return m_To; }
            set { m_To = value; }
        }

        public Range(PrimesBigInteger from, PrimesBigInteger to)
        {
            this.From = from;
            this.To = to;
        }

        public Range(int from, int to)
            : this(PrimesBigInteger.ValueOf(from), PrimesBigInteger.ValueOf(to))
        {
        }

        public PrimesBigInteger RangeAmount
        {
            get
            {
                return this.To.Add((this.From.Multiply(PrimesBigInteger.ValueOf(-1))));
            }
        }

        public virtual PrimesBigInteger GetZeroPosition()
        {
            throw new NotImplementedException("GetZeroPosition is only implmented in Primes.Library.RangeX and Primes.Library.RangeY");
        }
    }
}
