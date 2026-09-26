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


namespace AvalancheVisualization
{
    internal class PieChart : Shape

    {
        public double angle;
        public double pieceRotation;
        public Point startingPointOfArc;
        public Point endPointOfArc;




        protected override Geometry DefiningGeometry => drawPiece();


        //creates a piece of pie diagram 
        private StreamGeometry drawPiece()
        {
            StreamGeometry geometry = new StreamGeometry();

            using (StreamGeometryContext ctx = geometry.Open())
            {
                geometry.FillRule = FillRule.EvenOdd;
                bool largeArc = angle > 180;

                startingPointOfArc = coordinates(pieceRotation);
                endPointOfArc = coordinates(pieceRotation + angle);

                startingPointOfArc.Offset(60, 60);
                endPointOfArc.Offset(60, 60);

                ctx.BeginFigure(new Point(60, 60), true, true);
                ctx.LineTo(startingPointOfArc, false, true);
                ctx.ArcTo(endPointOfArc, new Size(50, 50), 0, largeArc, SweepDirection.Clockwise, false, true);

            }


            return geometry;

        }

        //calculates share of the pie chart to be occupied
        public double calculateAngle(int bits, Tuple<string, string> strTuple)
        {

            double angleDegree = ((double)bits / strTuple.Item1.Length) * 360;

            double roundUpAngle = Math.Round(angleDegree, 0, MidpointRounding.AwayFromZero);

            return roundUpAngle;
        }


        public double calculateAngleClassic(int bytes, byte[] cipher)
        {

            double angleDegree = ((double)bytes / cipher.Length) * 360;



            double roundUpAngle = Math.Round(angleDegree, 0, MidpointRounding.AwayFromZero);

            return roundUpAngle;
        }


        public Point coordinates(double angle)
        {
            // conversion from  angle in degrees to radians
            double pointX = Math.Cos((Math.PI / 180.0) * angle) * 50;
            double pointY = Math.Sin((Math.PI / 180.0) * angle) * 50;

            return new Point(pointX, pointY);
        }



    }
}
