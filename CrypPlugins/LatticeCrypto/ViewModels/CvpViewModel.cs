using LatticeCrypto.Properties;
using LatticeCrypto.Utilities;
using LatticeCrypto.Utilities.Arrows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LatticeCrypto.ViewModels
{
    public class CvpViewModel : SvpGaussViewModel
    {
        public void ChangeTargetPointToSelectedPoint(Point point)
        {
            TargetVectorX = new BigInteger(Math.Round((point.X - y_line.X1) / PixelsPerPoint / scalingFactorX));
            TargetVectorY = new BigInteger(Math.Round((x_line.Y1 - point.Y) / PixelsPerPoint / scalingFactorY));
        }

        public void FindClosestVector(bool writeHistory)
        {
            //Anmerkung: Man könnte für das Finden eines Closest Vectors die Methode von Babai benutzen.
            //Hierbei wird der Punkt als Linearkombination mit den reduzierten Vektoren ausgedrückt und am
            //Ende müssen die Skalare auf ganze Zahlen gerundet werden. Da aber dadurch zwischenzeitlich
            //mit Dezimalzahlen gearbeitet werden muss, ist dies hier schwierig, da mit BigInteger gearbeitet
            //wird. Daher wird der einfachere, pragmatischere Weg eingeschlagen: Es wird über die Parallelo-
            //gramme iteriert und schließlich der Eckpunkt gesucht, der dem Punkt am nähsten liegt.
            //Da die Anzahl der Parallelogramme begrenzt ist (z.B. 1.000) geht dies recht schnell.

            if (TargetVectorX == null || TargetVectorY == null)
            {
                closestVector = null;
                closestVectorArrow = null;
                return;
            }

            double targetX = y_line.X1 + (double)TargetVectorX * PixelsPerPoint * scalingFactorX;
            double targetY = x_line.Y1 + (double)TargetVectorY * PixelsPerPoint * scalingFactorY;

            closestVector = new Ellipse
            {
                Width = 10 + PixelsPerPoint / 5,
                Height = 10 + PixelsPerPoint / 5,
                Fill = Brushes.DarkOrange,
                ToolTip = "X = " + TargetVectorX + ", Y = " + TargetVectorY
            };
            Canvas.SetLeft(closestVector, targetX - closestVector.Width / 2);
            Canvas.SetBottom(closestVector, targetY - closestVector.Height / 2);

            //Vorauswahl
            List<Polygon> listPolygonsSmaller = listPolygons.FindAll(x => (new List<Point>(x.Points)).Exists(y => y.X >= targetX)).FindAll(x => (new List<Point>(x.Points)).Exists(y => y.X <= targetX)).FindAll(x => (new List<Point>(x.Points)).Exists(y => y.Y >= 2 * x_line.Y1 - targetY)).FindAll(x => (new List<Point>(x.Points)).Exists(y => y.Y <= 2 * x_line.Y1 - targetY));
            Point closestVectorPoint = new Point();
            double closestDistance = double.MaxValue;
            bool vectorFound = false;

            //Suche in den übrig gebliebenen Polygonen
            foreach (Polygon polygon in listPolygonsSmaller.Where(polygon => IsPointInPolygon(polygon.Points, new Point(targetX, canvas.ActualHeight - targetY)) || listPolygonsSmaller.Count <= 1))
            {
                foreach (Point point in polygon.Points)
                {
                    double distance = Math.Sqrt(Math.Pow(point.X - targetX, 2) + Math.Pow(point.Y - (canvas.ActualHeight - targetY), 2));
                    if (distance >= closestDistance)
                    {
                        continue;
                    }

                    closestDistance = distance;
                    closestVectorPoint = point;
                    vectorFound = true;
                }
                break;
            }

            if (vectorFound)
            {
                closestVectorArrow = new ArrowLine { X1 = closestVectorPoint.X, X2 = targetX, Y1 = closestVectorPoint.Y, Y2 = (canvas.ActualHeight - targetY), Stroke = Brushes.DarkOrange, StrokeThickness = 7, ArrowAngle = 65, ArrowLength = 25 + PixelsPerPoint / 10 };
            }
            else
            {
                closestVectorArrow = null;
            }

            BigInteger closestVectorPointX = new BigInteger(Math.Round((closestVectorPoint.X - y_line.X1) / pixelsPerPoint / scalingFactorX));
            BigInteger closestVectorPointY = new BigInteger(Math.Round((x_line.Y1 - closestVectorPoint.Y) / pixelsPerPoint / scalingFactorY));

            double shortestVectorDistance = Math.Sqrt(Math.Pow((double)closestVectorPointX - (double)TargetVectorX, 2) + Math.Pow((double)closestVectorPointY - (double)TargetVectorY, 2));

            if (!writeHistory)
            {
                return;
            }

            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add(new Bold(new Underline(new Run("** " + Languages.buttonFindClosestVector + " **\r\n"))));
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelTargetPoint + ":")));
            paragraph.Inlines.Add(" {" + TargetVectorX + ", " + TargetVectorY + "}\r\n");
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelClosestVector)));
            paragraph.Inlines.Add(" {" + closestVectorPointX + ", " + closestVectorPointY + "}\r\n");
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelShortestDistance)));
            paragraph.Inlines.Add(" " + Util.FormatDoubleLog(shortestVectorDistance) + "\r\n");

            if (History.Document.Blocks.FirstBlock != null)
            {
                History.Document.Blocks.InsertBefore(History.Document.Blocks.FirstBlock, paragraph);
            }
            else
            {
                History.Document.Blocks.Add(paragraph);
            }
        }

        public bool IsPointInPolygon(PointCollection points, Point point)
        {
            int j = points.Count - 1;
            bool oddNodes = false;

            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].Y < point.Y && points[j].Y >= point.Y || points[j].Y < point.Y && points[i].Y >= point.Y)
                {
                    if (points[i].X + (point.Y - points[i].Y) / (points[j].Y - points[i].Y) * (points[j].X - points[i].X) < point.X)
                    {
                        oddNodes = !oddNodes;
                    }
                }
                j = i;
            }

            return oddNodes;
        }
    }
}
