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
using Primes.WpfControls.Components;
using System.Collections.Generic;

namespace Primes.WpfControls.Primegeneration.Function
{
    public class EulerPolynom : SecondDegreePolynom
    {
        public EulerPolynom()
            : base()
        {
            m_StrImageUri = "pack://application:,,,/Primes;Component/Resources/icons/eulerpolynom.jpg";
            base.SetParameter(A, new PolynomFactor(A, PrimesBigInteger.One, true));
            base.SetParameter(B, new PolynomFactor(B, PrimesBigInteger.NegativeOne, true));
            base.SetParameter(C, new PolynomFactor(C, PrimesBigInteger.ValueOf(41), true));
        }

        public override PrimesBigInteger Execute(PrimesBigInteger input)
        {
            return (input.Pow(2).Subtract(input)).Add(PrimesBigInteger.ValueOf(41));
        }

        public override ICollection<PolynomFactor> Factors => new Dictionary<string, PolynomFactor>().Values;

        public override string Name => Resources.lang.WpfControls.Generation.PrimesGeneration.polynomname_polynomeuler;
    }
}
