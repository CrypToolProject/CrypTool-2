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
                (double posX, double posY) = GetPosition();
                FormattedText formattedText = new FormattedText(Text, CultureInfo.GetCultureInfo("de"),
                    FlowDirection.LeftToRight, new Typeface("Arial"), 11, Brushes.Black,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip);
                Geometry geometry = formattedText.BuildGeometry(new Point(posX, posY));

                geometry.Freeze();
                return geometry;
            }
        }


        private (double PosX, double PosY) GetPosition()
        {
            double endAngle = Rotation + 90;
            double maxWidth = Math.Max(0.0, Radius - Pie.StrokeSize / 2);
            double maxHeight = Math.Max(0.0, Radius - Pie.StrokeSize / 2);
            double xStart = maxWidth * Math.Cos(endAngle * Math.PI / 180.0);
            double yStart = maxHeight * Math.Sin(endAngle * Math.PI / 180.0);

            double posX = CentreX + xStart;
            double posY = CentreY - yStart;
            return (posX, posY);
        }
    }
}