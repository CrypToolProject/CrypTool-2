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
using System.Windows;

namespace Primes.Library
{
    public delegate void GmpBigIntegerParameterDelegate(PrimesBigInteger value);
    public delegate void MessageDelegate(string message);
    public delegate void ObjectParameterDelegate(object obj);
    public delegate void DoubleParameterDelegate(Size size);
    public delegate void CallBackDelegate(GmpBigIntegerParameterDelegate del);
    public delegate void Navigate(NavigationCommandType type);
    public delegate void ExecuteIntegerDelegate(PrimesBigInteger value);
    public delegate void ExecuteIntegerIntervalDelegate(PrimesBigInteger from, PrimesBigInteger to);
    public delegate void CallbackDelegateGetInteger(ExecuteIntegerDelegate ExecuteDelegate);
    public delegate void CallbackDelegateGetIntegerInterval(ExecuteIntegerIntervalDelegate ExecuteIntervalDelegate);
    public delegate void VoidDelegate();
}