/*                              
   Copyright 2025 Nils Kopal, CrypTool Project

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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CrypTool.Plugins.EllipticCurveCryptography.Views
{
    /// <summary>
    /// Visualises points of an elliptic curve y² = x³ + ax + b (or other curve via ICurve) over F_p
    /// and displays them in a fixed plot with labeled axes.
    /// </summary>
    public partial class EccCurveVisualizer : UserControl
    {
        private EllipticCurve _curve;

        public int MaxRange { get; set; } = 50;
        public int Ticks { get; set; } = 10;

        private WriteableBitmap _bmp;
        private const int Margin = 30;
        private const int PointSize = 3;

        public EccCurveVisualizer()
        {
            InitializeComponent();
            InitBitmap();
            IsVisibleChanged += OnVisibleChangedRender;
        }

        /// <summary>
        /// Forces to redraw when visibility changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnVisibleChangedRender(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                Redraw();
            }
        }

        private void InitBitmap()
        {
            int size = 400;
            _bmp = new WriteableBitmap(size, size, 96, 96, PixelFormats.Pbgra32, null);
            curveImage.Source = _bmp;
        }

        public void UpdateCurve(EllipticCurve curve)
        {
            _curve = curve;
            if (IsVisible)
            {
                Redraw();
            }
        }

        private void Redraw()
        {
            if (_curve == null || _bmp == null)
            {
                return;
            }

            int size = _bmp.PixelWidth;
            double plotSize = size - 2 * Margin;
            double scale = plotSize / MaxRange;

            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                // Background
                dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, size, size));

                // Axes
                Pen axisPen = new Pen(Brushes.Black, 1);
                dc.DrawLine(axisPen, new System.Windows.Point(Margin, size - Margin), new System.Windows.Point(size - Margin, size - Margin)); // X
                dc.DrawLine(axisPen, new System.Windows.Point(Margin, Margin), new System.Windows.Point(Margin, size - Margin)); // Y

                // Grid + labels
                Pen gridPen = new Pen(Brushes.LightGray, 0.5);
                Typeface font = new Typeface("Segoe UI");
                Brush textBrush = Brushes.Gray;
                double fontSize = 10;
                double step = plotSize / Ticks;

                for (int i = 0; i <= Ticks; i++)
                {
                    double x = Margin + i * step;
                    double y = size - Margin - i * step;

                    if (i > 0)
                    {
                        dc.DrawLine(gridPen, new System.Windows.Point(x, Margin), new System.Windows.Point(x, size - Margin));
                        dc.DrawLine(gridPen, new System.Windows.Point(Margin, y), new System.Windows.Point(size - Margin, y));
                    }

                    // X labels
                    var xLabel = new FormattedText(
                        ((int)(i * MaxRange / Ticks)).ToString(),
                        System.Globalization.CultureInfo.InvariantCulture,
                        FlowDirection.LeftToRight,
                        font,
                        fontSize,
                        textBrush,
                        1.25);
                    dc.DrawText(xLabel, new System.Windows.Point(x - xLabel.Width / 2, size - Margin + 2));

                    // Y labels
                    var yLabel = new FormattedText(
                        ((int)(i * MaxRange / Ticks)).ToString(),
                        System.Globalization.CultureInfo.InvariantCulture,
                        FlowDirection.LeftToRight,
                        font,
                        fontSize,
                        textBrush,
                        1.25);
                    dc.DrawText(yLabel, new System.Windows.Point(Margin - yLabel.Width - 4, y - yLabel.Height / 2));
                }

                // Curve points
                SolidColorBrush ptBrush = Brushes.Red;

                for (int xVal = 0; xVal <= MaxRange; xVal++)
                {
                    for (int yVal = 0; yVal <= MaxRange; yVal++)
                    {
                        Point pt = new Point
                        {
                            Curve = _curve,
                            IsInfinity = false,
                            X = xVal,
                            Y = yVal
                        };

                        if (!_curve.IsPointOnCurve(pt)) continue;

                        double px = Margin + xVal * scale;
                        double py = size - Margin - yVal * scale;

                        dc.DrawEllipse(ptBrush, null, new System.Windows.Point(px, py), PointSize, PointSize);
                    }
                }
            }

            // Render to bitmap
            RenderTargetBitmap rtb = new RenderTargetBitmap(_bmp.PixelWidth, _bmp.PixelHeight, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(dv);

            _bmp.Lock();
            rtb.CopyPixels(new Int32Rect(0, 0, _bmp.PixelWidth, _bmp.PixelHeight), _bmp.BackBuffer, _bmp.BackBufferStride * _bmp.PixelHeight, _bmp.BackBufferStride);
            _bmp.AddDirtyRect(new Int32Rect(0, 0, _bmp.PixelWidth, _bmp.PixelHeight));
            _bmp.Unlock();
        }
    }
}