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
    public class FunctionX2 : IFunction
    {
        private double m_FormerValue = double.NaN;

        public double FormerValue
        {
            get => m_FormerValue;
            set => m_FormerValue = value;
        }

        public double Execute(double input)
        {
            double result = (input * input);
            m_FormerValue = result;
            return result;
        }

        #region IFunction Members

        public void Reset()
        {
            m_FormerValue = double.NaN;
        }

        #endregion

        #region IFunction Members

        public bool CanEstimate => true;

        #endregion

        #region IFunction Members

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

        private void FireExecutedEvent()
        {
            if (Executed != null)
            {
                Executed(null);
            }
        }

        public double DrawTo => double.PositiveInfinity;

        #endregion
    }
}
