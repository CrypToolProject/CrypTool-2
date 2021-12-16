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
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Primes.WpfControls.Factorization.QS
{
    public class Step3 : BaseStep, IQSStep
    {
        private readonly TextBlock m_lblInfo;

        public Step3(Grid grid, TextBlock lblInfo)
            : base(grid)
        {
            m_lblInfo = lblInfo;
        }

        #region IQSStep Members

        public override QSResult Execute(ref QSData data)
        {
            int counter = 0;
            IList<QuadraticPair> pairs = data.BSmooth;
            bool foundquadratic = false;

            ControlHandler.SetPropertyValue(
                m_lblInfo,
                "Text",
                string.Format(Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step3_smooth, pairs.Count));

            //
            // Suche nach glatten Werten, die selbst schon Quadrat sind
            //

            foreach (QuadraticPair pair in pairs)
            {
                string msg;

                if (data.IsIgnored(PrimesBigInteger.ValueOf(pair.B)))
                {
                    pair.QuadraticStatus = QuadraticStatus.Ignore;
                }

                if (pair.QuadraticStatus == QuadraticStatus.Ignore)
                {
                    msg = string.Format(Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step3_ignored, pair.B);
                }
                else
                {
                    int sqrt = (int)Math.Sqrt(pair.B);
                    if (sqrt * sqrt == pair.B)
                    {
                        foundquadratic = true;
                        pair.QuadraticStatus = QuadraticStatus.Quadratic;
                        msg = string.Format(Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step3_issquare, pair.B, pair.B, sqrt);
                    }
                    else
                    {
                        msg = string.Format(Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step3_isnotsquare, pair.B);
                    }
                }

                ControlHandler.AddRowDefintion(Grid, 1, GridUnitType.Auto);
                ControlHandler.ExecuteMethod(this, "AddToGrid", new object[] { Grid, msg.ToString(), counter++, 0, 0, 0 });

                if (foundquadratic)
                {
                    return QSResult.Ok;
                }
            }

            //
            // Suche nach Produkten von glatten Werten, die ein Quadrat ergeben
            //

            int pslen = (int)Math.Pow(2, pairs.Count);

            for (int i = 1; i < pslen; i++)
            {
                MyInteger mi = new MyInteger(i);
                if (mi.BitCount <= 1)
                {
                    continue;
                }

                StringBuilder msg = new StringBuilder();
                int[] indices = mi.GetIndices();
                long erg = 1;

                foreach (int j in indices)
                {
                    if (msg.Length > 0)
                    {
                        msg.Append(" * ");
                    }

                    msg.Append(pairs[j].B);
                    erg *= pairs[j].B;
                }

                if (erg != 1)
                {
                    if (data.IsIgnored(PrimesBigInteger.ValueOf(erg)))
                    {
                        ControlHandler.AddRowDefintion(Grid, 1, GridUnitType.Auto);
                        TextBlock tb = AddTextBlock(counter++, 0);
                        ControlHandler.SetPropertyValue(tb, "Text", Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step3_testcombi + msg.ToString());
                        ControlHandler.SetPropertyValue(tb, "Text", string.Format(Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step3_testcombiignore, msg, erg));
                    }
                    else
                    {
                        int sqrt = (int)Math.Sqrt(erg);
                        if (sqrt * sqrt == erg)
                        {
                            ControlHandler.AddRowDefintion(Grid, 1, GridUnitType.Auto);
                            TextBlock tb = AddTextBlock(counter++, 0);
                            ControlHandler.SetPropertyValue(tb, "Text", Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step3_testcombi + msg.ToString());
                            ControlHandler.SetPropertyValue(tb, "Text", string.Format(Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step3_testcombiisquare, msg, erg, erg, sqrt));
                            foreach (int j in indices)
                            {
                                pairs[j].QuadraticStatus = QuadraticStatus.Part;
                            }

                            return QSResult.Ok;
                        }
                        else
                        {
                            ControlHandler.AddRowDefintion(Grid, 1, GridUnitType.Auto);
                            TextBlock tb = AddTextBlock(counter++, 0);
                            ControlHandler.SetPropertyValue(tb, "Text", Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step3_testcombi + msg.ToString());
                            ControlHandler.SetPropertyValue(tb, "Text", string.Format(Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step3_testcombiisnotsquare, msg, erg));
                        }
                    }
                }

                //Thread.Sleep(m_Delay);
            }

            return QSResult.Failed; // keine Quadrate gefunden
        }

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
                Text = "",
                Margin = new Thickness(5)
            };

            Grid.SetColumn(tbA, 0);
            Grid.SetRow(tbA, 0);
            Grid.Children.Add(tbA);
        }

        public override void PostStep()
        {
        }

        #endregion
    }
}
