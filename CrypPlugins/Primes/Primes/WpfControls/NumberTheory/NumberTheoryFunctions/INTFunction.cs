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
using System.Collections.ObjectModel;

namespace Primes.WpfControls.NumberTheory.NumberTheoryFunctions
{
    public delegate void NumberTheoryMessageDelegate(INTFunction function, PrimesBigInteger value, string message);

    public interface INTFunction
    {
        void Start(PrimesBigInteger from, PrimesBigInteger to, PrimesBigInteger second);
        void Stop();
        event VoidDelegate OnStart;
        event VoidDelegate OnStop;
        event NumberTheoryMessageDelegate Message;
        string Description { get; }
        bool IsRunnung { get; }
        bool NeedsSecondParameter { get; }
    }

    public class NTFunctions : ObservableCollection<INTFunction> { }
}