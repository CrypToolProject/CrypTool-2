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

namespace Primes.Library.Function
{
    public enum FunctionState { Stopped, Running }

    public interface IFunction
    {
        double FormerValue { get; }
        double Execute(double input);
        void Reset();
        bool CanEstimate { get; }
        FunctionState FunctionState { get; set; }
        double MaxValue { get; set; }
        event ObjectParameterDelegate Executed;
        double DrawTo { get; }
    }
}
