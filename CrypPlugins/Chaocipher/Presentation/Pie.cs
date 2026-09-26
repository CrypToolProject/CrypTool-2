/*
   Copyright (C) CrypTool 2 Team

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
                StreamGeometry geometry = new StreamGeometry { FillRule = FillRule.EvenOdd };

                using (StreamGeometryContext context = geometry.Open())
                {
                    DrawGeometry(context);
                }

                geometry.Freeze();
                return geometry;
            }
        }

        private void DrawGeometry(StreamGeometryContext context)
        {
            double startAngle = Rotation + 90;
            double endAngle = Rotation + Angle + 90;

            double maxWidth = Math.Max(0.0, Radius - StrokeThickness / 2);
            double maxHeight = Math.Max(0.0, Radius - StrokeThickness / 2);

            double xEnd = maxWidth * Math.Cos(startAngle * Math.PI / 180.0);
            double yEnd = maxHeight * Math.Sin(startAngle * Math.PI / 180.0);

            double xStart = maxWidth * Math.Cos(endAngle * Math.PI / 180.0);
            double yStart = maxHeight * Math.Sin(endAngle * Math.PI / 180.0);

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