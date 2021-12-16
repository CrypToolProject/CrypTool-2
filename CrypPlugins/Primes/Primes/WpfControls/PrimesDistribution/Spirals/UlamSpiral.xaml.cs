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
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Primes.WpfControls.PrimesDistribution.Spirals
{
    /// <summary>
    /// Interaction logic for UlamSpiral.xaml
    /// </summary>
    public partial class UlamSpiral : UserControl, IPrimeSpiral
    {
        private Thread m_DrawThread;
        private double m_UnitWidth;

        private PrimesBigInteger m_From;
        private PrimesBigInteger m_To;

        private enum Direction { Right, Up, Left, Down }

        public UlamSpiral()
        {
            InitializeComponent();
        }

        public void Draw(PrimesBigInteger from, PrimesBigInteger to)
        {
            m_From = from;
            m_To = to;

            m_DrawThread = new Thread(new ThreadStart(DrawThread))
            {
                CurrentCulture = Thread.CurrentThread.CurrentCulture,
                CurrentUICulture = Thread.CurrentThread.CurrentUICulture
            };
            m_DrawThread.Start();
        }

        #region Events

        public event VoidDelegate StartDrawing;
        public event VoidDelegate StopDrawing;

        #endregion

        private void DrawThread()
        {
            FireEvent_StartDrawing();

            double size = CalculateSize() * m_UnitWidth;
            ControlHandler.SetPropertyValue(PaintArea, "Width", size);
            ControlHandler.SetPropertyValue(PaintArea, "Height", size);
            double sawidth = (double)ControlHandler.GetPropertyValue(PaintArea, "Width");
            double saheight = (double)ControlHandler.GetPropertyValue(PaintArea, "Height");
            ControlHandler.ExecuteMethod(sv, "ScrollToVerticalOffset", new object[] { sawidth / 2.0 });
            ControlHandler.ExecuteMethod(sv, "ScrollToHorizontalOffset", new object[] { saheight / 2.0 });
            int direction = 0;
            double x1 = size / 2.0;
            double y1 = size / 2.0;
            int lenghtFactor = 0;
            long value = m_From.LongValue;

            while (value <= m_To.LongValue)
            {
                if (direction % 2 == 0)
                {
                    lenghtFactor++;
                }

                double x2 = x1, y2 = y1;
                Calculate(x1, y1, (Direction)direction, lenghtFactor, ref x2, ref y2);
                DrawLine(x1, x2, y1, y2);

                for (int j = 0; j < lenghtFactor; j++)
                {
                    if (value > m_To)
                    {
                        break;
                    }

                    x2 = x1;
                    y2 = y1;
                    Calculate(x1, y1, (Direction)direction, 1, ref x2, ref y2);

                    if (PrimeNumbers.isprime.Contains(value))
                    {
                        DrawNumberButton(value, x1, y1);
                    }

                    value = value + 1;
                    x1 = x2;
                    y1 = y2;
                }

                direction = (direction + 1) % 4;
            }

            FireEvent_StopDrawing();
        }

        private double CalculateSize()
        {
            int limit = (m_To - m_From).IntValue;
            int result = 1;
            int i = 0;

            for (int k = 0; k < limit; k += result, i++)
            {
                if (i % 2 == 0)
                {
                    result++;
                }
            }

            return result + 5;
        }

        private void Calculate(double x1, double y1, Direction direction, int factor, ref double x2, ref double y2)
        {
            switch (direction)
            {
                case Direction.Up:
                    x2 = x1;
                    y2 = y1 - m_UnitWidth * factor;
                    break;
                case Direction.Right:
                    x2 = x1 + m_UnitWidth * factor;
                    y2 = y1;
                    break;
                case Direction.Down:
                    x2 = x1;
                    y2 = y1 + m_UnitWidth * factor;
                    break;
                case Direction.Left:
                    x2 = x1 - m_UnitWidth * factor;
                    y2 = y1;
                    break;
            }
        }

        private void DrawLine(double x1, double x2, double y1, double y2)
        {
            Line result = ControlHandler.CreateObject(typeof(Line)) as Line;
            ControlHandler.SetPropertyValue(result, "X1", x1);
            ControlHandler.SetPropertyValue(result, "X2", x2);
            ControlHandler.SetPropertyValue(result, "Y1", y1);
            ControlHandler.SetPropertyValue(result, "Y2", y2);
            ControlHandler.SetPropertyValue(result, "Stroke", Brushes.Black);
            ControlHandler.SetPropertyValue(result, "StrokeThickness", 0.5);

            ControlHandler.AddChild(result, PaintArea);
        }

        private delegate void SetCanvasDelegate(MethodInfo mi, UIElement element, double value);

        private void SetCanvas(MethodInfo mi, UIElement element, double value)
        {
            mi.Invoke(PaintArea, new object[] { element, value });
        }

        private void DrawNumberButton(PrimesBigInteger value, double x, double y)
        {
            Ellipse nb = ControlHandler.CreateObject(typeof(Ellipse)) as Ellipse;
            //NumberButton nb = ControlHandler.CreateObject(typeof(NumberButton)) as NumberButton;
            //ControlHandler.SetPropertyValue(nb, "NumberButtonStyle", NumberButtonStyle.Ellipse.ToString());
            //ControlHandler.SetPropertyValue(nb, "BINumber", value);
            ControlHandler.SetPropertyValue(nb, "Width", 6);
            ControlHandler.SetPropertyValue(nb, "Height", 6);
            ToolTip tt = ControlHandler.CreateObject(typeof(ToolTip)) as ToolTip;
            ControlHandler.SetPropertyValue(tt, "Content", value.ToString());
            ControlHandler.SetPropertyValue(nb, "ToolTip", tt);

            //if (value.IsPrime(10))
            //  ControlHandler.SetPropertyValue(nb, "Background", Brushes.Blue);
            //else
            //  ControlHandler.SetPropertyValue(nb, "Background", Brushes.Gray);
            if (value.IsPrime(10))
            {
                ControlHandler.SetPropertyValue(nb, "Fill", Brushes.Blue);
            }
            else
            {
                ControlHandler.SetPropertyValue(nb, "Fill", Brushes.Gray);
            }

            ControlHandler.ExecuteMethod(PaintArea, "SetTop", new object[] { nb, y - 3 });
            ControlHandler.ExecuteMethod(PaintArea, "SetLeft", new object[] { nb, x - 3 });
            ControlHandler.AddChild(nb, PaintArea);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            m_UnitWidth = sizeInfo.NewSize.Width / 150;
        }

        public void CancelDrawThread()
        {
            if (m_DrawThread != null)
            {
                m_DrawThread.Abort();
                m_DrawThread = null;
            }

            FireEvent_StopDrawing();
        }

        #region IPrimeSpiral Members

        public void Close()
        {
            CancelDrawThread();
        }

        public void Clear()
        {
            CancelDrawThread();
            PaintArea.Children.Clear();
        }

        public void Cancel()
        {
            CancelDrawThread();
        }

        #endregion

        private void FireEvent_StartDrawing()
        {
            if (StartDrawing != null)
            {
                StartDrawing();
            }
        }

        private void FireEvent_StopDrawing()
        {
            if (StopDrawing != null)
            {
                StopDrawing();
            }
        }

        private void silderRotate_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lblRotation.Text = $"{e.NewValue:N2}°";
        }
    }
}
