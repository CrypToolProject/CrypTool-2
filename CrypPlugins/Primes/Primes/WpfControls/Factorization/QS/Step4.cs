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
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Primes.WpfControls.Factorization.QS
{
    public class Step4 : BaseStep, IQSStep
    {
        private readonly TextBlock m_lblInfo;

        public Step4(Grid grid, TextBlock lblInfo)
            : base(grid)
        {
            m_lblInfo = lblInfo;
        }

        #region IQSStep Members

        public override QSResult Execute(ref QSData data)
        {
            int counter = 0;
            IList<QuadraticPair> smoothpair = data.BSmooth;
            long productA = 1;
            long productB = 1;
            string msg;

            foreach (QuadraticPair pair in smoothpair)
            {
                if (pair.QuadraticStatus == QuadraticStatus.Quadratic)
                {
                    productA = pair.A;
                    productB = pair.B;
                    break;
                }
            }

            if (productA == 1 && productB == 1)
            {
                foreach (QuadraticPair pair in smoothpair)
                {
                    if (pair.QuadraticStatus == QuadraticStatus.Part)
                    {
                        productA = (productA * pair.A) % data.N;
                        productB *= pair.B;
                        pair.QuadraticStatus = QuadraticStatus.Nope;
                    }
                }
            }

            if (productA == 1 || productB == 1)
            {
                ControlHandler.AddRowDefintion(Grid, 1, GridUnitType.Auto);
                ControlHandler.ExecuteMethod(this, "AddToGrid", new object[] { Grid, string.Format(Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step4_end, data.N), counter++, 0, 0, 0 });
                return QSResult.Ok;
            }

            long sqb = (long)Math.Sqrt(productB);

            StringBuilder sbInfo = new StringBuilder();
            sbInfo.Append(string.Format(Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step4_abcalculated, productA, sqb));
            sbInfo.Append("\n");
            sbInfo.Append(Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step4_checkcong);
            ControlHandler.SetPropertyValue(m_lblInfo, "Text", sbInfo.ToString());

            ControlHandler.AddRowDefintion(Grid, 1, GridUnitType.Auto);
            ControlHandler.ExecuteMethod(this, "AddToGrid", new object[] { Grid, BuildQuadMod(productA, sqb, data.N), counter++, 0, 0, 0 });
            ControlHandler.AddRowDefintion(Grid, 1, GridUnitType.Auto);
            ControlHandler.ExecuteMethod(this, "AddToGrid", new object[] { Grid, BuildMod(productA, sqb, data.N), counter++, 0, 0, 0 });

            if (!ModuloTest(productA, sqb, data.N))    // Modulotest nicht bestanden
            {
                data.AddIgnoreQuadrat(PrimesBigInteger.ValueOf(productB));
                msg = Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step4_notproofed;
                ControlHandler.AddRowDefintion(Grid, 1, GridUnitType.Auto);
                ControlHandler.ExecuteMethod(this, "AddToGrid", new object[] { Grid, msg, counter++, 0, 0, 0 });

                return QSResult.Failed;
            }

            msg = string.Format(Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step4_proofed, data.N);
            ControlHandler.AddRowDefintion(Grid, 1, GridUnitType.Auto);
            ControlHandler.ExecuteMethod(this, "AddToGrid", new object[] { Grid, msg, counter++, 0, 0, 0 });

            long factor1 = (long)BigInteger.GreatestCommonDivisor(productA + sqb, data.N);
            long factor2 = (long)BigInteger.GreatestCommonDivisor(productA - sqb, data.N);

            bool trivialfactor1 = (factor1 == 1 || factor1 == data.N);
            bool trivialfactor2 = (factor2 == 1 || factor2 == data.N);

            string sbfactor1 = string.Format(Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step4_factor1, productA, sqb, data.N, factor1);
            ControlHandler.AddRowDefintion(Grid, 1, GridUnitType.Auto);
            ControlHandler.ExecuteMethod(this, "AddToGrid", new object[] { Grid, sbfactor1, counter++, 0, 0, 0 });

            string sbfactor2 = string.Format(Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step4_factor2, productA, sqb, data.N, factor2);
            ControlHandler.AddRowDefintion(Grid, 1, GridUnitType.Auto);
            ControlHandler.ExecuteMethod(this, "AddToGrid", new object[] { Grid, sbfactor2, counter++, 0, 0, 0 });

            if (trivialfactor1 && trivialfactor2)
            {
                data.AddIgnoreQuadrat(PrimesBigInteger.ValueOf(productB));
                msg = string.Format(Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step4_reset, productA, sqb);
                ControlHandler.AddRowDefintion(Grid, 1, GridUnitType.Auto);
                ControlHandler.ExecuteMethod(this, "AddToGrid", new object[] { Grid, msg, counter++, 0, 0, 0 });

                return QSResult.Failed;
            }

            long nontrivialfactor = trivialfactor1 ? factor2 : factor1;

            FireFoundFactorEvent(PrimesBigInteger.ValueOf(nontrivialfactor));
            FireFoundFactorEvent(PrimesBigInteger.ValueOf(data.N / nontrivialfactor));

            //Boolean p1 = BigIntegerHelper.IsProbablePrime(factor1);
            //Boolean p2 = BigIntegerHelper.IsProbablePrime(factor2);

            //if (p1) FireFoundFactorEvent(PrimesBigInteger.ValueOf(factor1));
            //if (p2) FireFoundFactorEvent(PrimesBigInteger.ValueOf(factor2));

            //if( !(p1 && p2) ) // falls ein Faktor nicht prim ist
            //{
            //    long notPrime = p1 ? factor2 : factor1;
            //    msg = string.Format(Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step4_refactorize, notPrime, notPrime);
            //    ControlHandler.AddRowDefintion(Grid, 1, GridUnitType.Auto);
            //    ControlHandler.ExecuteMethod( this, "AddToGrid", new object[] { Grid, msg, counter++, 0, 0, 0 });

            //    data.N = notPrime;
            //    return QSResult.Restart;
            //}

            return QSResult.Ok;
        }

        private string BuildQuadMod(long producta, long productb, long mod)
        {
            string res = ((producta * producta - productb * productb) % mod == 0) ? Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step4_yes : Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step4_no;
            return producta + "² ≡ " + productb + "² (mod " + mod + "): " + res;
        }

        private string BuildMod(long producta, long productb, long mod)
        {
            string res = ((producta - productb) % mod != 0) ? Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step4_yes : Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step4_no;
            return producta + " ≢  " + productb + " (mod " + mod + "): " + res;
        }

        public override void PreStep()
        {
            ControlHandler.ExecuteMethod(this, "_PreStep");
        }

        public void _PreStep()
        {
            Grid.RowDefinitions.Clear();
            Grid.Children.Clear();
            m_lblInfo.Text = "";
        }

        public override void PostStep()
        {
        }

        #endregion
    }
}
