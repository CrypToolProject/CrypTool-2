using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CrypTool.Chaocipher.Presentation
{
    public class Pie : Shape
    {
        public double CentreX { get; set; }
        public double CentreY { get; set; }
        public double Radius { get; set; }
        public double Rotation { get; set; }
        public double Angle { get; set; }
        public string Text { get; set; }
        public static double StrokeSize = 50;

        public Pie()
        {
            StrokeThickness = StrokeSize;
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                var geometry = new StreamGeometry {FillRule = FillRule.EvenOdd};

                using (var context = geometry.Open())
                {
                    DrawGeometry(context);
                }

                geometry.Freeze();
                return geometry;
            }
        }

        private void DrawGeometry(StreamGeometryContext context)
        {
            var startAngle = Rotation + 90;
            var endAngle = Rotation + Angle + 90;

            var maxWidth = Math.Max(0.0, Radius - StrokeThickness / 2);
            var maxHeight = Math.Max(0.0, Radius - StrokeThickness / 2);

            var xEnd = maxWidth * Math.Cos(startAngle * Math.PI / 180.0);
            var yEnd = maxHeight * Math.Sin(startAngle * Math.PI / 180.0);

            var xStart = maxWidth * Math.Cos(endAngle * Math.PI / 180.0);
            var yStart = maxHeight * Math.Sin(endAngle * Math.PI / 180.0);

            context.BeginFigure(
                new Point(CentreX + xStart,
                    CentreY - yStart),
                true, // Filled
                false); // Closed
            context.ArcTo(
                new Point(CentreX + xEnd,
                    CentreY - yEnd),
                new Size(maxWidth, maxHeight),
                0.0, // rotationAngle
                startAngle - endAngle > 180, // greater than 180 deg?
                SweepDirection.Clockwise,
                true, // isStroked
                false);
        }
    }
}