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

using Primes.Library;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace Primes.WpfControls.Factorization.QS
{
    public class Step2 : BaseStep, IQSStep
    {
        private readonly TextBlock m_lblInfo;

        public Step2(Grid grid, TextBlock lblInfo)
            : base(grid)
        {
            m_lblInfo = lblInfo;
        }

        #region IQSStep Members

        public override void PreStep()
        {
            ControlHandler.ExecuteMethod(this, "_PreStep");
        }

        public void _PreStep()
        {
            Grid.RowDefinitions.Clear();
            Grid.Children.Clear();

            RowDefinition rd = new RowDefinition
            {
                Height = new GridLength(1, GridUnitType.Auto)
            };
            Grid.RowDefinitions.Add(rd);

            TextBlock tbA = new TextBlock
            {
                Text = "a",
                Margin = new Thickness(5)
            };

            TextBlock tbB = new TextBlock
            {
                Text = "b",
                Margin = new Thickness(5)
            };

            TextBlock tbFactors = new TextBlock
            {
                Text = Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step2_factorization,
                Margin = new Thickness(5)
            };

            TextBlock tbIsBSmooth = new TextBlock
            {
                Text = Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step2_bsmooth,
                Margin = new Thickness(5)
            };

            Grid.SetColumn(tbA, 0);
            Grid.SetRow(tbA, 0);
            Grid.Children.Add(tbA);

            Grid.SetColumn(tbB, 1);
            Grid.SetRow(tbB, 0);
            Grid.Children.Add(tbB);

            Grid.SetColumn(tbFactors, 2);
            Grid.SetRow(tbFactors, 0);
            Grid.Children.Add(tbFactors);

            Grid.SetColumn(tbIsBSmooth, 3);
            Grid.SetRow(tbIsBSmooth, 0);
            Grid.Children.Add(tbIsBSmooth);
        }

        public override void PostStep()
        {
        }

        public override QSResult Execute(ref QSData data)
        {
            IList<int> m_Factors = data.CalculateFactorBase();
            StringBuilder fb = new StringBuilder();

            foreach (int i in m_Factors)
            {
                if (fb.Length > 0)
                {
                    fb.Append(",");
                }

                fb.Append(i);
            }

            ControlHandler.SetPropertyValue(
                m_lblInfo,
                "Text",
                string.Format(Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step2_B, data.B, fb.ToString()));

            IList<long> list = new List<long>();
            int counter = 1;

            foreach (QuadraticPair pair in data)
            {
                ControlHandler.AddRowDefintion(Grid, 1, GridUnitType.Auto);

                AddToGrid(Grid, pair.A.ToString(), counter, 0, 0, 0);
                AddToGrid(Grid, pair.B.ToString(), counter, 1, 0, 0);
                TextBlock tb = AddTextBlock(counter, 2);
                StringBuilder sb = new StringBuilder();

                long b = pair.B;
                if (b < 0)
                {
                    b = -b;
                    pair.AddExponent(-1, 1);
                    sb.Append("-1");
                }

                if (b != 0)
                {
                    for (int i = 0; i < m_Factors.Count; i++)
                    {
                        int f = m_Factors[i];
                        int exp = 0;
                        while (b % f == 0)
                        {
                            b /= f;
                            exp++;
                        }
                        if (exp > 0)
                        {
                            if (sb.Length > 0)
                            {
                                sb.Append(" * ");
                            }

                            sb.Append(f);
                            if (exp > 1)
                            {
                                sb.Append("^" + exp);
                            }
                        }
                        pair.AddExponent(f, exp);
                    }
                }

                if (b > 1 && sb.Length > 0)
                {
                    sb.Append(" * " + b);
                }

                ControlHandler.SetPropertyValue(tb, "Text", sb.ToString());

                pair.IsBSmooth = (b == 1) || (b == 0);
                string s = pair.IsBSmooth ? Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step2_yes : Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step2_no;
                ControlHandler.ExecuteMethod(this, "AddToGrid", new object[] { Grid, s, counter++, 3, 0, 0 });

                Thread.Sleep(m_Delay);
            }

            return QSResult.Ok;
        }

        #endregion
    }
}
