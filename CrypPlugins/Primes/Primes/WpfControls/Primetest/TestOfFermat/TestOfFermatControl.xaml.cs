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
using Primes.WpfControls.Components;
using Primes.WpfControls.Components.Arrows;
using Primes.WpfControls.Validation;
using Primes.WpfControls.Validation.Validator;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Primes.WpfControls.Primetest.TestOfFermat
{
    /// <summary>
    /// Interaction logic for TestOfFermatControl.xaml
    /// </summary>
    public partial class TestOfFermatControl : UserControl, IPrimeTest
    {
        #region initializing

        private Thread m_Thread;

        public TestOfFermatControl()
        {
            InitializeComponent();
            InputValidator<PrimesBigInteger> iv = new InputValidator<PrimesBigInteger>
            {
                Validator = new BigIntegerMinValueMaxValueValidator(null, PrimesBigInteger.Two, PrimesBigInteger.ValueOf(100))
            };
            iscA.AddInputValidator(
              Primes.WpfControls.Components.InputSingleControl.Free,
              iv);
            iscA.Execute += new Primes.WpfControls.Components.ExecuteSingleDelegate(iscA_Execute);
            iscA.FreeText = "2";

            ircSystematic.Execute += new ExecuteDelegate(ircSystematic_Execute);
            ircSystematic.SetText(InputRangeControl.FreeFrom, "2");
            ircSystematic.SetText(InputRangeControl.FreeTo, "100");
            m_Points = new Dictionary<int, Point>();
            m_Arrows = new Dictionary<PrimesBigInteger, ArrowLine>();
            m_RunningLockObject = new object();
        }

        private void ircSystematic_Execute(PrimesBigInteger from, PrimesBigInteger to, PrimesBigInteger second)
        {
            if (ForceGetValue != null)
            {
                ForceGetValue(new GmpBigIntegerParameterDelegate(Execute));
            }
        }

        private void iscA_Execute(PrimesBigInteger value)
        {
            A = value;
            if (ForceGetValue != null)
            {
                ForceGetValue(new GmpBigIntegerParameterDelegate(Execute));
            }
        }

        public bool IsRunning()
        {
            return (m_Thread != null) && m_Thread.IsAlive;
        }

        #endregion

        #region events

        public event VoidDelegate ExecuteTest;
        public event VoidDelegate CancelTest;

        #endregion

        #region Properties

        private readonly object m_RunningLockObject;
        private bool m_Running;
        private PrimesBigInteger m_SystematicFrom;
        private PrimesBigInteger m_SystematicTo;
        private PrimesBigInteger m_A;

        private PrimesBigInteger A
        {
            set
            {
                if (value != null)
                {
                    iscA.SetText(Primes.WpfControls.Components.InputSingleControl.Free, value.ToString());
                    m_A = value;
                    if (lblA != null)
                    {
                        lblA.Content = value.ToString();
                    }
                }

                //if (this.PaintArea != null)
                //{
                //  iscA.SetText(Primes.WpfControls.Components.InputSingleControl.Free, value.ToString());
                //  ClearPoints();
                //  m_A = value;
                //  for (int i = 0; i < m_A; i++)
                //  {
                //    CreatePoint(i);
                //  }
                //}
            }
            get => m_A;
        }

        private readonly IDictionary<int, Point> m_Points;

        #endregion

        #region painting the points

        private void CreatePoint(int value)
        {
            double pointsize = 4.0;
            double angle = (360.0 / m_Value.IntValue) * value;
            double top = Radius + 25 + (Math.Sin((angle * 2 * Math.PI) / 360.0) * Radius - (pointsize / 2.0));
            double left = Radius + 25 + (Math.Cos((angle * 2 * Math.PI) / 360.0) * Radius - (pointsize / 2.0));

            Ellipse point = ControlHandler.CreateObject(typeof(Ellipse)) as Ellipse;
            ControlHandler.SetPropertyValue(point, "Width", pointsize);
            ControlHandler.SetPropertyValue(point, "Height", pointsize);
            ControlHandler.ExecuteMethod(PaintArea, "SetTop", new object[] { point, top });
            ControlHandler.ExecuteMethod(PaintArea, "SetLeft", new object[] { point, left });
            ControlHandler.SetPropertyValue(point, "Fill", Brushes.Black);
            ControlHandler.SetPropertyValue(point, "Stroke", Brushes.Black);
            ControlHandler.SetPropertyValue(point, "StrokeThickness", 1.0);
            ControlHandler.AddChild(point, PaintArea);

            Label lbl = ControlHandler.CreateLabel(value.ToString(), null);
            double _top = top;
            double _left = left - 5;
            _top += (top < Radius) ? -20 : 0;
            _left += (left < Radius) ? -7 + (-2 * (int)Math.Log(value)) : 2;
            ControlHandler.ExecuteMethod(PaintArea, "SetTop", new object[] { lbl, _top });
            ControlHandler.ExecuteMethod(PaintArea, "SetLeft", new object[] { lbl, _left });
            ControlHandler.AddChild(lbl, PaintArea);
            if (m_Points.ContainsKey(value))
            {
                m_Points[value] = new Point(left, top);
            }
            else
            {
                m_Points.Add(value, new Point(left, top));
            }
        }

        private PrimesBigInteger m_Value;

        #endregion

        #region Painting the Ellipse

        private void Paint()
        {
            Ellipse el = ControlHandler.CreateObject(typeof(Ellipse)) as Ellipse;
            ControlHandler.SetPropertyValue(el, "Width", Aperture);
            ControlHandler.SetPropertyValue(el, "Height", Aperture);
            ControlHandler.ExecuteMethod(CircleArea, "SetTop", new object[] { el, 25 });
            ControlHandler.ExecuteMethod(CircleArea, "SetLeft", new object[] { el, 25 });
            ControlHandler.SetPropertyValue(el, "StrokeThickness", 1.0);
            ControlHandler.SetPropertyValue(el, "Stroke", Brushes.Black);
            ControlHandler.SetPropertyValue(el, "Name", "dontremove");

            ControlHandler.AddChild(el, CircleArea);

            //Ellipse middle = ControlHandler.CreateObject(typeof(Ellipse)) as Ellipse;
            //ControlHandler.SetPropertyValue(middle, "Width", 2);
            //ControlHandler.SetPropertyValue(middle, "Height", 2);
            //ControlHandler.ExecuteMethod(PaintArea, "SetTop", new object[] { middle, 25 + (Aperture / 2.0) - 1.0 });
            //ControlHandler.ExecuteMethod(PaintArea, "SetLeft", new object[] { middle, 25 + (Aperture / 2.0) - 1.0 });
            //ControlHandler.SetPropertyValue(middle, "StrokeThickness", 1.0);
            //ControlHandler.SetPropertyValue(middle, "Stroke", Brushes.Black);
            //ControlHandler.AddChild(middle, PaintArea);
            //Line l = ControlHandler.CreateObject(typeof(Line)) as Line;
            //l.Y1 = l.Y2 = 25 + Radius;
            //l.X1 = 25 + Radius;
            //l.X2 = 25 + 2*Radius;

            //l.Stroke = Brushes.Black;
            //l.StrokeThickness = 1.0;

            //ControlHandler.AddChild(l, PaintArea);
        }

        private double Aperture => Math.Max(Math.Min(PaintArea.ActualWidth, PaintArea.ActualHeight) - 50, 0);

        private double Radius => Aperture / 2.0;

        private double Perimeter => (2 * Math.PI * Aperture);

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            log.RenderSize = sizeInfo.NewSize;
            //if (log.ActualWidth < 100)
            //{
            //  log.Width = this.ActualWidth - 5;
            //}
            if (sizeInfo.NewSize.Height != double.NaN && sizeInfo.NewSize.Height > 0 &&
              sizeInfo.NewSize.Width != double.NaN && sizeInfo.NewSize.Width > 0)
            {
                if (!m_Running)
                {
                    if (CircleArea.Children.Count != 0)
                    {
                        CircleArea.Children.Clear();
                    }

                    Paint();
                    if (m_Value != null)
                    {
                        IDictionary<ArrowLine, int> m_StartPoints = new Dictionary<ArrowLine, int>();
                        IDictionary<ArrowLine, int> m_EndPoints = new Dictionary<ArrowLine, int>();

                        foreach (ArrowLine al in m_Arrows.Values)
                        {
                            Point from = new Point(al.X1, al.Y1);
                            Point to = new Point(al.X2, al.Y2);
                            foreach (int key in m_Points.Keys)
                            {
                                Point p = m_Points[key];
                                if (from.X == p.X && from.Y == p.Y)
                                {
                                    m_StartPoints.Add(al, key);
                                }
                                else if (to.X == p.X && to.Y == p.Y)
                                {
                                    m_EndPoints.Add(al, key);
                                }
                            }
                        }
                        CreatePoints();
                        foreach (ArrowLine al in m_StartPoints.Keys)
                        {
                            al.X1 = m_Points[m_StartPoints[al]].X;
                            al.Y1 = m_Points[m_StartPoints[al]].Y;
                        }
                        foreach (ArrowLine al in m_EndPoints.Keys)
                        {
                            al.X2 = m_Points[m_EndPoints[al]].X;
                            al.Y2 = m_Points[m_EndPoints[al]].Y;
                        }
                    }
                }
            }
        }

        private void ClearPoints()
        {
            PaintArea.Children.Clear();
        }

        #endregion

        #region Execute

        private void CancelThread()
        {
            if (m_Thread != null)
            {
                lock (m_RunningLockObject)
                {
                    m_Running = false;
                }

                m_Thread.Abort();
                m_Thread = null;
            }
        }

        public void Execute(PrimesBigInteger value)
        {
            tabItemGraphic.IsEnabled = true;
            tcStats.SelectedIndex = 0;

            m_Value = value;
            m_Arrows.Clear();
            ArrowArea.Children.Clear();
            log.Columns = 1;
            log.Clear();
            if (value.CompareTo(PrimesBigInteger.ValueOf(150)) > 0)
            {
                tcStats.SelectedIndex = 1;
                tabItemGraphic.IsEnabled = false;
                log.Info("Ist die zu prüfende Zahl > 150 kann die Berechnung im Kreis nicht mehr sinnvoll dargestellt werden.");

                switch (KindOfTest)
                {
                    case KOD.Single:
                        ExecuteSingle_Log();
                        break;
                    case KOD.Systematic:
                        ExecuteSystematic_Log();
                        break;
                }
            }
            else
            {
                CreatePoints();
                switch (KindOfTest)
                {
                    case KOD.Single:
                        ExecuteSingle_Graphic();
                        break;
                    case KOD.Systematic:
                        ExecuteSystematic_Graphic();
                        break;
                }
            }
        }

        private void ExecuteSingle_Log()
        {
            A = iscA.GetValue();
            if (A != null)
            {
                ExecuteLog(A);
            }
        }

        private void ExecuteSystematic_Log()
        {
            if (ircSystematic.GetValue(ref m_SystematicFrom, ref m_SystematicTo))
            {
                m_Thread = new Thread(new ThreadStart(ExecuteTestSystematic_Log))
                {
                    CurrentCulture = Thread.CurrentThread.CurrentCulture,
                    CurrentUICulture = Thread.CurrentThread.CurrentUICulture
                };
                m_Thread.Start();
            }
        }

        private void ExecuteTestSystematic_Log()
        {
            FireEventStartTest();

            while (m_SystematicFrom.CompareTo(m_SystematicTo) <= 0)
            {
                if (!ExecuteLog(m_SystematicFrom))
                {
                    m_SystematicFrom = m_SystematicTo;
                }
                m_SystematicFrom = m_SystematicFrom.Add(PrimesBigInteger.One);
            }

            FireEventStopText();
        }

        private bool ExecuteLog(PrimesBigInteger a)
        {
            PrimesBigInteger result = a.ModPow(m_Value.Subtract(PrimesBigInteger.One), m_Value);
            log.Info(
              string.Format(
                "Berechne {0}^{1} mod {2} = {3}",
                new object[] { a.ToString(), m_Value.Subtract(PrimesBigInteger.One), m_Value.ToString(), result.ToString() }));
            ControlHandler.SetPropertyValue(lblA, "Content", a.ToString());
            ControlHandler.SetPropertyValue(lblExp, "Text", m_Value.ToString() + "-1");
            ControlHandler.SetPropertyValue(lblP, "Content", m_Value.ToString());
            ControlHandler.SetPropertyValue(lblCalc, "Text", result.ToString());

            if (result.Equals(PrimesBigInteger.One))
            {
                log.Info(string.Format("{0} hat den Fermattest bestanden und ist mir einer Wahrscheinlichkeit von 50% eine Primzahl", m_Value.ToString()));
            }
            else
            {
                log.Info(string.Format("{0} hat den Fermattest nicht bestanden und ist damit definitiv keine Primzahl. {1} ist Belastungszeuge gegen {2}", new object[] { m_Value.ToString(), a.ToString(), m_Value.ToString() }));
            }

            return result.Equals(PrimesBigInteger.One);
        }

        private void ExecuteSingle_Graphic()
        {
            A = iscA.GetValue();
            if (A != null)
            {

                m_Thread = new Thread(new ThreadStart(ExecuteTestSingle_Graphic))
                {
                    CurrentCulture = Thread.CurrentThread.CurrentCulture,
                    CurrentUICulture = Thread.CurrentThread.CurrentUICulture
                };
                m_Thread.Start();
            }
        }

        private void ExecuteTestSingle_Graphic()
        {
            FireEventStartTest();
            DoExecuteGraphic(A);
            FireEventStopText();
        }

        private void ExecuteSystematic_Graphic()
        {
            if (ircSystematic.GetValue(ref m_SystematicFrom, ref m_SystematicTo))
            {
                m_Thread = new Thread(new ThreadStart(ExecuteTestSystematic_Graphic))
                {
                    CurrentCulture = Thread.CurrentThread.CurrentCulture,
                    CurrentUICulture = Thread.CurrentThread.CurrentUICulture
                };
                m_Thread.Start();
            }
        }

        private void ExecuteTestSystematic_Graphic()
        {
            while (m_SystematicFrom.CompareTo(m_SystematicTo) <= 0)
            {
                FireEventStartTest();
                DoExecuteGraphic(m_SystematicFrom);
                m_SystematicFrom = m_SystematicFrom.Add(PrimesBigInteger.One);
            }

            FireEventStopText();
        }

        private void FireEventStopText()
        {
            if (CancelTest != null)
            {
                CancelTest();
            }
        }

        private void FireEventStartTest()
        {
            if (ExecuteTest != null)
            {
                ExecuteTest();
            }
        }

        private void CreatePoints()
        {
            ClearPoints();
            for (int i = 0; i < m_Value.IntValue; i++)
            {
                CreatePoint(i);
            }
        }

        private void DoExecuteGraphic(PrimesBigInteger a)
        {
            lock (m_RunningLockObject)
            {
                m_Running = true;
            }
            Point lastPoint = new Point(-1, -1);
            PrimesBigInteger factor = null;

            ControlHandler.SetPropertyValue(lblA, "Content", a.ToString());
            ControlHandler.SetPropertyValue(lblExp, "Text", m_Value.ToString() + "-1");
            ControlHandler.SetPropertyValue(lblP, "Content", m_Value.ToString());

            ControlHandler.SetPropertyValue(lblCalc, "Text", string.Empty);
            ControlHandler.SetPropertyValue(lblCalc, "Text", a.ModPow(m_Value.Subtract(PrimesBigInteger.One), m_Value).ToString());
            PrimesBigInteger i = PrimesBigInteger.Two;

            PrimesBigInteger result = null;
            PrimesBigInteger counter = PrimesBigInteger.Zero;
            while (i.CompareTo(m_Value.Subtract(PrimesBigInteger.One)) <= 0)
            {
                Thread.Sleep(100);
                result = a.ModPow(i, m_Value);
                log.Info(
                  string.Format(
                    "Berechne {0}^{1} mod {2} = {3}",
                    new object[] { a.ToString(), i.ToString(), m_Value.ToString(), result.ToString() }));

                if (factor == null)
                {
                    factor = result;
                }
                else
                {
                    factor = factor.Multiply(result);
                }
                //StringBuilder sbText = new StringBuilder();// new StringBuilder(ControlHandler.GetPropertyValue(lblCalc, "Text") as string);
                //sbText.Append(result.ToString());
                //if (i.CompareTo(m_Value.Subtract(PrimesBigInteger.One)) < 0)
                //  sbText.Append(" * ");
                //ControlHandler.SetPropertyValue(lblCalc, "Text", sbText.ToString());

                if (lastPoint.X == -1 && lastPoint.Y == -1)
                {
                    lastPoint = m_Points[result.IntValue];
                }
                else
                {
                    Point newPoint = m_Points[result.IntValue];
                    CreateArrow(counter, lastPoint, newPoint);
                    lastPoint = newPoint;
                }
                i = i.Add(PrimesBigInteger.One);
                counter = counter.Add(PrimesBigInteger.One);
            }

            if (result != null)
            {
                if (result.Equals(PrimesBigInteger.One))
                {
                    log.Info(
                      string.Format(
                        "Berechne {0}^{1} mod {2} = 1. {3} könnte eine Primzahl sein.",
                        new object[] { a.ToString(), m_Value.Subtract(PrimesBigInteger.One).ToString(), m_Value.ToString(), m_Value.ToString() }));
                }
                else
                {
                    log.Info(
                      string.Format(
                        "Berechne {0}^{1} mod {2} = {3}. {4} ist damit definitiv keine Primzahl.",
                        new object[] { a.ToString(), m_Value.Subtract(PrimesBigInteger.One).ToString(), m_Value.ToString(), result.ToString(), m_Value.ToString() }));
                }
            }

            if (CancelTest != null)
            {
                CancelTest();
            }

            lock (m_RunningLockObject)
            {
                m_Running = false;
            }
        }

        private readonly IDictionary<PrimesBigInteger, ArrowLine> m_Arrows;

        private void CreateArrow(PrimesBigInteger i, Point from, Point to)
        {
            ArrowLine l = ControlHandler.CreateObject(typeof(ArrowLine)) as ArrowLine;
            ControlHandler.SetPropertyValue(l, "Stroke", Brushes.Black);
            ControlHandler.SetPropertyValue(l, "StrokeThickness", 1.0);
            ControlHandler.SetPropertyValue(l, "X1", from.X);
            ControlHandler.SetPropertyValue(l, "Y1", from.Y);
            ControlHandler.SetPropertyValue(l, "X2", to.X);
            ControlHandler.SetPropertyValue(l, "Y2", to.Y);
            if (!ContainsLine(l))
            {
                ControlHandler.AddChild(l, ArrowArea);
                m_Arrows.Add(i, l);
            }
            else
            {
                ResetLine(l);
            }
        }

        private void ResetLine(ArrowLine l)
        {
            ArrowLine ltmp = GetLine(l);
            if (ltmp != null)
            {
                UIElementCollection children = ControlHandler.GetPropertyValue(ArrowArea, "Children") as UIElementCollection;
                ControlHandler.ExecuteMethod(children, "Remove", new object[] { ltmp });
                Thread.Sleep(100);
                ControlHandler.ExecuteMethod(children, "Add", new object[] { ltmp });
            }
        }

        private ArrowLine GetLine(ArrowLine l)
        {
            foreach (ArrowLine line in m_Arrows.Values)
            {
                double srcx1 = (double)ControlHandler.GetPropertyValue(line, "X1");
                double srcx2 = (double)ControlHandler.GetPropertyValue(line, "X2");
                double srcy1 = (double)ControlHandler.GetPropertyValue(line, "Y1");
                double srcy2 = (double)ControlHandler.GetPropertyValue(line, "Y2");
                double destx1 = (double)ControlHandler.GetPropertyValue(l, "X1");
                double destx2 = (double)ControlHandler.GetPropertyValue(l, "X2");
                double desty1 = (double)ControlHandler.GetPropertyValue(l, "Y1");
                double desty2 = (double)ControlHandler.GetPropertyValue(l, "Y2");

                if (srcx1 == destx1 && srcx2 == destx2 && srcy1 == desty1 && srcy2 == desty2)
                {
                    return line;
                }
            }

            return null;
        }

        private bool ContainsLine(ArrowLine l)
        {
            return GetLine(l) != null;
        }

        public void Cancel()
        {
            CancelThread();
            if (CancelTest != null)
            {
                CancelTest();
            }
        }

        #endregion

        public void CleanUp()
        {
            CancelThread();
        }

        #region IPrimeTest Members

        public IValidator<PrimesBigInteger> Validator => new BigIntegerMinValueValidator(null, PrimesBigInteger.Two);

        #endregion

        private void rbTest_Click(object sender, RoutedEventArgs e)
        {
            pnlSingle.Visibility = (rbSingleTest.IsChecked.Value) ? Visibility.Visible : Visibility.Collapsed;
            pnlSystematic.Visibility = (rbSystmaticTest.IsChecked.Value) ? Visibility.Visible : Visibility.Collapsed;
        }

        private KOD KindOfTest
        {
            get
            {
                if (rbSingleTest.IsChecked.Value)
                {
                    return KOD.Single;
                }
                else
                {
                    return KOD.Systematic;
                }
            }
        }

        private enum KOD { Single, Systematic }

        #region IPrimeTest Members

        public event CallBackDelegate ForceGetValue;

        #endregion

        #region IPrimeVisualization Members

        public event VoidDelegate Start;

        private void FireStartEvent()
        {
            if (Start != null)
            {
                Start();
            }
        }

        public event VoidDelegate Stop;

        private void FireStopEvent()
        {
            if (Stop != null)
            {
                Stop();
            }
        }

        event VoidDelegate IPrimeVisualization.Cancel
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        public event CallbackDelegateGetInteger ForceGetInteger;

        private void FireForceGetInteger()
        {
            ForceGetInteger(null);
        }

        public event CallbackDelegateGetInteger ForceGetIntegerInterval;

        private void FireForceGetIntegerInterval()
        {
            ForceGetIntegerInterval(null);
        }

        public void CancelExecute()
        {
        }

        public void Execute(PrimesBigInteger from, PrimesBigInteger to)
        {
        }

        #endregion
    }
}
