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
    public class FunctionLiN : BaseFunction, IFunction
    {
        private bool usePari;

        public FunctionLiN()
            : base()
        {
            if (paribridge == null && Options.OptionsAccess.UsePari)
            {
                Pari.PariBridge.Initialize(Options.OptionsAccess.GpExe);
                if (Pari.PariBridge.IsInitialized)
                {
                    paribridge = new Primes.Library.Pari.PariBridge();
                }
            }
        }

        private Pari.PariBridge paribridge;

        #region IFunction Members

        public double Execute(double input)
        {
            double value = 0.0;

            if (usePari)
            {
                value = paribridge.LiX(input);
            }
            else
            {
                if ((input > 100000 || m_FormerValue == double.NaN) || MaxValue > 100000)
                {
                    double a = 512.0;

                    double c1 = 16 * Math.Log10(input);
                    double c2 = c1 * 2;

                    double h1 = (input - a) / c1;
                    double h2 = (input - a) / c2;

                    double sum1 = 0.0, sum2 = 0.0, sum3 = 0.0;
                    for (int i = 0; i < c1; i++)
                    {
                        double xk = a + i * h1;
                        if (i % c1 == 0)
                        {
                            sum1 += 1 / Math.Log(xk);
                        }
                        else if (i % 2 == 1)
                        {
                            sum2 += 1 / Math.Log(xk);
                        }
                        else
                        {
                            sum3 += 1 / Math.Log(xk);
                        }
                    }
                    sum2 *= 4;
                    sum3 *= 2;
                    value = (sum1 + sum2 + sum3) * (h1 / 3);
                    sum1 = sum2 = sum3 = 0.0;
                    double value2 = 0.0;
                    for (int i = 0; i < c2; i++)
                    {
                        double xk = a + i * h2;
                        if (i % c2 == 0)
                        {
                            sum1 += 1 / Math.Log(xk);
                        }
                        else if (i % 2 == 1)
                        {
                            sum2 += 1 / Math.Log(xk);
                        }
                        else
                        {
                            sum3 += 1 / Math.Log(xk);
                        }
                    }
                    value2 = (sum1 + sum2 + sum3) * (h2 / 3);
                    double error = (value2 - value) * (1 / 15);
                    value -= error;
                    if (value == 0.0)
                    {
                        value = 0.1;
                    }
                }
                else
                {
                    double factor = 1;
                    double counter = 1.0 / (1.0 * factor);
                    double divider = 2;
                    for (int i = 2; i <= input * factor; i++)
                    {
                        value += (counter) / Math.Log(divider);
                        divider += counter;
                    }
                }
            }
            m_FormerValue = value;
            if (Executed != null)
            {
                Executed(value);
            }

            return value;
        }

        #endregion

        #region IFunction Members

        public void Reset()
        {
            m_FormerValue = double.NaN;
            if (Options.OptionsAccess.UsePari)
            {
                if (paribridge == null)
                {
                    Pari.PariBridge.Initialize(Options.OptionsAccess.GpExe);
                    if (Pari.PariBridge.IsInitialized)
                    {
                        paribridge = new Primes.Library.Pari.PariBridge();
                    }
                }
                usePari = Pari.PariBridge.IsInitialized;
            }
            else
            {
                usePari = false;
            }
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

        private double m_MaxValue;

        public double MaxValue
        {
            get => m_MaxValue;
            set => m_MaxValue = value;
        }

        #endregion

        #region IFunction Members

        public event ObjectParameterDelegate Executed;

        #endregion

        #region IFunction Members

        public double DrawTo => double.PositiveInfinity;

        #endregion
    }
}
