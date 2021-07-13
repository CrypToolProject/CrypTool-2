using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CrypTool.Chaocipher.Presentation
{
    public class PieText : Shape
    {
        public double Angle { get; set; }
        public double Radius { get; set; }
        public double CentreX { get; set; }
        public double CentreY { get; set; }
        public string Text { get; set; }
        public double Rotation { get; set; }

        public PieText()
        {
            StrokeThickness = 1;
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                var (posX, posY) = GetPosition();
                var formattedText = new FormattedText(Text, CultureInfo.GetCultureInfo("de"),
                    FlowDirection.LeftToRight, new Typeface("Arial"), 11, Brushes.Black,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip);
                var geometry = formattedText.BuildGeometry(new Point(posX, posY));

                geometry.Freeze();
                return geometry;
            }
        }


        private (double PosX, double PosY) GetPosition()
        {
            var endAngle = Rotation + 90;
            var maxWidth = Math.Max(0.0, Radius- Pie.StrokeSize/ 2);
            var maxHeight = Math.Max(0.0, Radius - Pie.StrokeSize / 2);
            var xStart = maxWidth * Math.Cos(endAngle * Math.PI / 180.0);
            var yStart = maxHeight * Math.Sin(endAngle * Math.PI / 180.0);

            var posX = CentreX + xStart;
            var posY = CentreY - yStart;
            return (posX, posY);
        }
    }
}