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

namespace Primes.Library.Function
{
    public class FunctionPiGauss : BaseFunction, IFunction
    {
        public FunctionPiGauss() : base() { }

        #region IFunction Members

        public double Execute(double input)
        {
            if (input < 2)
            {
                throw new ResultNotDefinedException();
            }

            double result = input / Math.Log(input);
            if (double.IsInfinity(result))
            {
                result = m_FormerValue;
            }

            m_FormerValue = result;
            if (Executed != null)
            {
                Executed(result);
            }

            return result;
        }

        #endregion

        #region IFunction Members

        public void Reset()
        {
            m_FormerValue = double.NaN;
        }

        #endregion

        #region IFunction Members

        public bool CanEstimate => true;

        private FunctionState m_FunctionState;

        public FunctionState FunctionState
        {
            get => m_FunctionState;
            set => m_FunctionState = value;
        }

        #endregion

        #region IFunction Members

        public double MaxValue
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        #endregion

        #region IFunction Members

        public event ObjectParameterDelegate Executed;

        public double DrawTo => double.PositiveInfinity;

        #endregion
    }
}
