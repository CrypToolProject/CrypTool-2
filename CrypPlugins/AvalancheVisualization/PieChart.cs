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
