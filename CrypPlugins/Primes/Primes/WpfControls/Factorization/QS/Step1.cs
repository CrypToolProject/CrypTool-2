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
using System;
using System.Windows;
using System.Windows.Controls;

namespace Primes.WpfControls.Factorization.QS
{
    public class Step1 : BaseStep, IQSStep
    {
        private readonly TextBlock m_lblInfo;

        public Step1(Grid grid, TextBlock lblInfo)
            : base(grid)
        {
            m_lblInfo = lblInfo;
        }

        //public override bool Execute(ref QSData data)
        //{
        //  base.Execute(ref data);

        //  long sqrt = (long)Math.Floor(Math.Sqrt(m_QSData.N)) + 1;
        //  ControlHandler.SetPropertyValue(
        //    m_lblInfo,
        //    "Text",
        //    "Die Quadratwurzel aus " + m_QSData.N.ToString("D") + " ist " + Math.Sqrt(m_QSData.N).ToString("N") + " ≈ " + sqrt);
        //  int counter = 0;

        //  for (long i = data.From; i <= data.To; i++)
        //  {
        //    ControlHandler.AddRowDefintion(Grid, 1, GridUnitType.Auto);
        //    string a = StringFormat.FormatDoubleToIntString((sqrt + i)) + "²";
        //    string aminus1 = StringFormat.FormatDoubleToIntString((Math.Pow((sqrt + i), 2) - m_QSData.N));

        //    data.Add(new QuadraticPair(sqrt + i, ((long)Math.Pow((sqrt + i), 2) - m_QSData.N)));
        //    ControlHandler.ExecuteMethod(
        //      this,
        //      "AddToGrid",
        //      new object[] { Grid, a, counter + 1, 0, 0, 0 });
        //    ControlHandler.ExecuteMethod(
        //      this,
        //      "AddToGrid",
        //      new object[] { Grid, aminus1, counter + 1, 1, 0, 0 });

        //    counter++;
        //    Thread.Sleep(m_Delay);

        //  }

        //  return true;
        //}

        public override QSResult Execute(ref QSData data)
        {
            base.Execute(ref data);

            long sqrt = (long)Math.Ceiling(Math.Sqrt(m_QSData.N));

            string msg = string.Format(Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step1_squareroot, m_QSData.N, Math.Sqrt(m_QSData.N).ToString("N"), sqrt, data.From, data.To);
            ControlHandler.SetPropertyValue(m_lblInfo, "Text", msg);

            int counter = 2;

            for (long i = data.From; i <= data.To; i++)
            {
                int a = (int)i;
                int b = (int)(a * a - m_QSData.N);
                data.Add(new QuadraticPair(a, b));

                ControlHandler.AddColumnDefintion(Grid, 1, GridUnitType.Auto);
                base.AddToGrid(Grid, a.ToString() + "²", 0, counter);
                base.AddToGrid(Grid, b.ToString(), 1, counter);

                counter++;
                //Thread.Sleep(m_Delay);
            }

            return QSResult.Ok;
        }

        #region IQSStep Members

        public override void PreStep()
        {
            ControlHandler.ExecuteMethod(this, "_PreStep");
        }

        //public void _PreStep()
        //{
        //  Grid.RowDefinitions.Clear();
        //  Grid.Children.Clear();
        //  RowDefinition rd = new RowDefinition();
        //  rd.Height = new GridLength(1, GridUnitType.Auto);
        //  Grid.RowDefinitions.Add(rd);

        //  TextBlock tbA = new TextBlock();
        //  tbA.Text = "a²";
        //  tbA.Margin = new Thickness(5);

        //  TextBlock tbAMinusN = new TextBlock();
        //  tbAMinusN.Text = "a²-n";
        //  tbAMinusN.Margin = new Thickness(5);

        //  Grid.SetColumn(tbA, 0);
        //  Grid.SetRow(tbA, 0);
        //  Grid.Children.Add(tbA);
        //  Grid.SetColumn(tbAMinusN, 1);
        //  Grid.SetRow(tbAMinusN, 0);
        //  Grid.Children.Add(tbAMinusN);
        //}

        public void _PreStep()
        {
            Grid.ColumnDefinitions.Clear();
            Grid.Children.Clear();

            ColumnDefinition rd1 = new ColumnDefinition
            {
                Width = new GridLength(1, GridUnitType.Auto)
            };
            Grid.ColumnDefinitions.Add(rd1);

            ColumnDefinition rd2 = new ColumnDefinition
            {
                Width = new GridLength(1, GridUnitType.Auto)
            };
            Grid.ColumnDefinitions.Add(rd2);

            base.AddToGrid(Grid, "F1", 0, 0);
            base.AddToGrid(Grid, "F2", 1, 0);
            base.AddToGrid(Grid, "a²", 0, 1);
            base.AddToGrid(Grid, "a²-n", 1, 1);
        }

        public override void PostStep()
        {
        }

        #endregion
    }
}
