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
using Primes.WpfControls.Validation;
using System;

namespace Primes.WpfControls.Factorization
{
    public delegate void FoundFactor(object o);

    public interface IFactorizer : IPrimeVisualization
    {
        //void Factorize(PrimesBigInteger value);
        //void CancelFactorization();
        //event FoundFactor FoundFactor;
        //event VoidDelegate Start;
        //event VoidDelegate Stop;
        //event VoidDelegate Cancel;

        //void Clean();
        void CancelFactorization();
        event FoundFactor FoundFactor;
        TimeSpan Needs { get; }
        IValidator<PrimesBigInteger> Validator { get; }
        bool isRunning { get; }
    }
}