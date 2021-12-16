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
using Primes.Library.Function;
using Primes.WpfControls.PrimesDistribution.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Threading;

namespace Primes.WpfControls.Threads
{
    public class FunctionThread : SuspendableThread
    {
        private readonly FunctionExecute m_FunctionExecute;
        private readonly DrawLine m_DrawLine;
        private readonly double m_XStart;
        public event FunctionEvent OnFunctionStart;
        public event FunctionEvent OnFunctionStop;
        private readonly Dispatcher m_Dispatcher;
        public PrimesBigInteger m_From;

        public FunctionThread(FunctionExecute functionExecute, DrawLine dl, double xStart, Dispatcher dispatcher)
        {
            m_FunctionExecute = functionExecute;
            m_DrawLine = dl;
            m_XStart = xStart;
            m_Dispatcher = dispatcher;
            m_From = m_FunctionExecute.Range.From;
        }

        /// <summary>
        /// This Method is gonna executed when then thread is startet
        /// </summary>
        protected override void OnDoWork()
        {
            if (OnFunctionStart != null)
            {
                OnFunctionStart(m_FunctionExecute.Function);
            }

            if (m_FunctionExecute != null && m_FunctionExecute.GetType() == typeof(FunctionExecute))
            {
                FunctionExecute fe = m_FunctionExecute;
                fe.Function.FunctionState = FunctionState.Running;
                if (fe.FunctionType == FunctionType.STAIR)
                {
                    ExecuteStair();
                }
                else
                {
                    double x1 = m_XStart;
                    PrimesBigInteger incX = PrimesBigInteger.One;
                    PrimesBigInteger inci = PrimesBigInteger.One;
                    PrimesBigInteger div = (fe.Range.RangeAmount.CompareTo(PrimesBigInteger.ValueOf(10000)) > 0) ? PrimesBigInteger.Ten : PrimesBigInteger.OneHundred;
                    if (fe.Range.RangeAmount.CompareTo(PrimesBigInteger.ValueOf(1000)) > 0 && fe.Function.CanEstimate)
                    {
                        inci = fe.Range.RangeAmount.Divide(div);
                        incX = inci;
                    }

                    PrimesBigInteger i = m_From;
                    while (inci > 0 && i.CompareTo(fe.Range.To) <= 0 && !HasTerminateRequest())
                    {
                        bool awokenByTerminate = SuspendIfNeeded();

                        if (awokenByTerminate)
                        {
                            m_From = i;
                            return;
                        }

                        double param = double.Parse(i.ToString());
                        if (fe.Function.FormerValue.Equals(double.NaN))
                        {
                            try
                            {
                                fe.Function.Execute(param);
                            }
                            catch (ResultNotDefinedException) { x1 += double.Parse(incX.ToString()); continue; }
                        }
                        else
                        {
                            double formerY = fe.Function.FormerValue;
                            double y = fe.Function.Execute(param);

                            double x2 = x1 + double.Parse(incX.ToString());

                            if (fe.Function.DrawTo.Equals(double.PositiveInfinity) || x2 <= fe.Function.DrawTo)
                            {
                                if (!DrawLine(x1, x2, formerY, y, fe.Color, fe.Function))
                                {
                                    break;
                                }
                            }

                            x1 = x2;
                        }

                        if (i.Add(inci).CompareTo(fe.Range.To) > 0)
                        {
                            inci = fe.Range.To.Subtract(i);
                            incX = inci;
                        }

                        i = i.Add(inci);
                    }
                }
                fe.Function.Reset();
                fe.Function.FunctionState = FunctionState.Stopped;
                if (OnFunctionStop != null)
                {
                    OnFunctionStop(m_FunctionExecute.Function);
                }
            }
        }

        private void ExecuteStair()
        {
            FunctionExecute fe = m_FunctionExecute;
            double x1 = m_XStart;
            PrimesBigInteger incX = PrimesBigInteger.One;
            PrimesBigInteger inci = PrimesBigInteger.One;
            PrimesBigInteger div = (fe.Range.RangeAmount.CompareTo(PrimesBigInteger.ValueOf(10000)) > 0) ? PrimesBigInteger.Ten : PrimesBigInteger.OneHundred;
            bool approximate = (fe.Range.RangeAmount.CompareTo(PrimesBigInteger.ValueOf(10000)) > 0);
            if (fe.Range.RangeAmount.CompareTo(PrimesBigInteger.ValueOf(1000)) > 0 && fe.Function.CanEstimate)
            {
                inci = fe.Range.RangeAmount.Divide(div);
                incX = inci;
            }

            List<LineParameters> lineSegments = new List<LineParameters>();

            PrimesBigInteger i = m_From;
            while (inci > 0 && i.CompareTo(fe.Range.To) <= 0 && !HasTerminateRequest())
            {
                bool awokenByTerminate = SuspendIfNeeded();

                if (awokenByTerminate)
                {
                    m_From = i;
                    return;
                }

                double param = i.DoubleValue;
                if (fe.Function.FormerValue.Equals(double.NaN))
                {
                    try
                    {
                        fe.Function.Execute(param);
                    }
                    catch (ResultNotDefinedException) { x1 += double.Parse(incX.ToString()); continue; }
                }
                else
                {
                    double formerY = fe.Function.FormerValue;
                    double y = fe.Function.Execute(param);

                    bool drawstair = !formerY.Equals(y) || formerY.Equals(double.NaN);
                    if (formerY.Equals(double.NaN))
                    {
                        formerY = y;
                    }

                    double x2 = x1 + double.Parse(incX.ToString());
                    if (fe.Function.DrawTo.Equals(double.PositiveInfinity) || (x2 <= fe.Function.DrawTo && x2 <= fe.Range.To.DoubleValue))
                    {
                        if (drawstair)
                        {
                            x2 -= double.Parse(incX.ToString());
                        }
                        lineSegments.Add(new LineParameters(x1, x2, formerY, y, fe.Color, fe.Function));
                        //if (!DrawLine(x1, x2, formerY, y, fe.Color, fe.Function)) break;
                        if (drawstair)
                        {
                            x2 += double.Parse(incX.ToString());
                            lineSegments.Add(new LineParameters(x1, x2, y, y, fe.Color, fe.Function));
                            //if (!DrawLine(x1, x2, y, y, fe.Color, fe.Function)) break;
                        }
                    }

                    x1 = x2;
                }

                if (i.Add(inci).CompareTo(fe.Range.To) > 0)
                {
                    inci = fe.Range.To.Subtract(i);
                    incX = inci;
                }
                i = i.Add(inci);

                if (lineSegments.Count > 10 * div)
                {
                    DrawLines(lineSegments, approximate);
                    lineSegments.Clear();
                }
            }

            DrawLines(lineSegments, approximate);
        }

        private bool DrawLines_Internal(IEnumerable<LineParameters> lineParameters)
        {
            foreach (LineParameters lineParameter in lineParameters)
            {
                if (!m_DrawLine(lineParameter).BoolValue)
                {
                    return false;
                }
            }
            return true;
        }

        private bool DrawLines(IEnumerable<LineParameters> lineParameters, bool approximate)
        {
            if (approximate)
            {
                LineParameters first = lineParameters.First();
                LineParameters last = lineParameters.Last();
                return DrawLine(first.X1, last.X2, first.Y1, last.Y2, first.Color, first.Function);
            }
            else
            {
                return (bool)m_Dispatcher.Invoke(
                    DispatcherPriority.Send,
                    new Func<IEnumerable<LineParameters>, bool>(DrawLines_Internal),
                    lineParameters);
            }
        }

        private bool DrawLine(double x1, double x2, double formerY, double y, Brush color, IFunction function)
        {
            LineParameters lineparams = new LineParameters(x1, x2, formerY, y, color, function);
            BoolWrapper result = m_Dispatcher.Invoke(DispatcherPriority.Send, m_DrawLine, lineparams) as BoolWrapper;
            return (result != null) ? result.BoolValue : false;
        }
    }
}
