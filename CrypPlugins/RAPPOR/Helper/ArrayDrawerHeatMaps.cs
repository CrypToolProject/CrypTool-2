using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RAPPOR.Helper
{
    /// <summary>
    /// This class is used to draw on the canvas of the Heat Maps view of the rappor component.
    /// </summary>
    public class ArrayDrawerHeatMaps : FrameworkElement
    {
        public ArrayDrawerHeatMaps()
        {

        }
        /// <summary>
        /// This method draws the legend of the heatmaps view. This legend is used to explain the
        /// created heatmaps.
        /// </summary>
        /// <param name="x">The x location of the top left corner of the heatmaps legend.</param>
        /// <param name="y">The y location of the top left corner of the heatmaps legend.</param>
        /// <param name="xLength">The x length of the heatmaps legend.</param>
        /// <param name="yLength">The y length of the heatmaps legend.</param>
        /// <returns>The path of the HeatMaps legend.</returns>
        public Path HeatMapLegend(int x, int y, int xLength, int yLength)
        {

            RectangleGeometry heatMapsRectangle = new RectangleGeometry
            {
                Rect = new System.Windows.Rect(x, y, xLength, yLength)
            };

            LinearGradientBrush myVerticalGradient = new LinearGradientBrush
            {
                StartPoint = new Point(0.5, 1),
                EndPoint = new Point(0.5, 0)
            };
            //The colors which are being used in the legend of the heatmaps view.
            myVerticalGradient.GradientStops.Add(new GradientStop(Colors.Black, 0.0));
            myVerticalGradient.GradientStops.Add(new GradientStop(Colors.Blue, .1666));
            myVerticalGradient.GradientStops.Add(new GradientStop(Colors.Green, .3333));
            myVerticalGradient.GradientStops.Add(new GradientStop(Colors.Yellow, .5));
            myVerticalGradient.GradientStops.Add(new GradientStop(Colors.Orange, .6666));
            myVerticalGradient.GradientStops.Add(new GradientStop(Colors.OrangeRed, .8333));
            myVerticalGradient.GradientStops.Add(new GradientStop(Colors.Red, 1));//This is very simular to (Color)new BrushConverter().ConvertFrom("#F01F2B")

            GeometryGroup geometryGroup = new GeometryGroup();
            geometryGroup.Children.Add(heatMapsRectangle);
            Path path = new Path
            {
                StrokeThickness = 1,
                Stroke = Brushes.Black,
                Data = geometryGroup
            };
            path.Fill = myVerticalGradient;
            return path;
        }
        /// <summary>
        /// This method provides the path for the text of the heat maps legend.
        /// </summary>
        /// <param name="x">The top left x value of the text.</param>
        /// <param name="y">The top left y value of the text.</param>
        /// <param name="yLength">The y length of the text values.</param>
        /// <param name="min">The minimal value of the parameters.</param>
        /// <param name="mid">The middle value of the parameters.</param>
        /// <param name="max">The maximal value of the parameters.</param>
        /// <returns>The path with the text of the heat maps legend.</returns>
        public Path HeatMapLegendText(int x, int y, int yLength, string min, string mid, string max)
        {
            VisualCollection Visuals = new VisualCollection(this);
            FormattedText formattedTextheatMapLegendTextTop = new FormattedText(max, Thread.CurrentThread.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Times New Roman"), 10, Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip);
            Geometry formattedTextheatMapLegendTextTopGeometry = formattedTextheatMapLegendTextTop.BuildGeometry(new Point(x, y));
            FormattedText formattedTextheatMapLegendTextMiddle = new FormattedText(mid, Thread.CurrentThread.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Times New Roman"), 10, Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip);
            Geometry formattedTextheatMapLegendTextMiddleGeometry = formattedTextheatMapLegendTextMiddle.BuildGeometry(new Point(x, y + yLength / 2));
            FormattedText formattedTextheatMapLegendTextBottom = new FormattedText(min, Thread.CurrentThread.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Times New Roman"), 10, Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip);

            //Removing 10 from thee y value to have it line up with the end of the bloom filter legend.
            Geometry formattedTextheatMapLegendTextBottomGeometry = formattedTextheatMapLegendTextBottom.BuildGeometry(new Point(x, y + yLength - 10));

            GeometryGroup geometryGroup = new GeometryGroup();
            geometryGroup.Children.Add(formattedTextheatMapLegendTextTopGeometry);
            geometryGroup.Children.Add(formattedTextheatMapLegendTextMiddleGeometry);
            geometryGroup.Children.Add(formattedTextheatMapLegendTextBottomGeometry);
            Path path = new Path
            {
                StrokeThickness = 1,
                Stroke = Brushes.Black,
                Data = geometryGroup
            };
            path.Fill = Brushes.Black;
            return path;
        }

        /// <summary>
        /// This method is used to create all rectangles in the heat maps view of the component.
        /// </summary>
        /// <param name="x">The x palcement of the top left of the rectangle</param>
        /// <param name="y">The y placement of the top left of the rectangle</param>
        /// <param name="xLength">The x length of the rectangle</param>
        /// <param name="yLength">The y length of the rectangle</param>
        /// <param name="hexcolor">The color of the rectangle as a hexcolor string</param>
        /// <returns>The path of the creaated rectangle</returns>
        public Path AddRectangle(int x, int y, int xLength, int yLength, string hexcolor)
        {
            RectangleGeometry rectangle = new RectangleGeometry
            {
                Rect = new System.Windows.Rect(x, y, xLength, yLength)
            };
            GeometryGroup geometryGroup = new GeometryGroup();
            geometryGroup.Children.Add(rectangle);
            Path path = new Path
            {
                StrokeThickness = 1,
                Stroke = Brushes.Black,
                Data = geometryGroup
            };
            path.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom(hexcolor);
            return path;
        }
        /// <summary>
        /// This method is used to add text to a canvas.
        /// </summary>
        /// <param name="x">The top left x placement of the text</param>
        /// <param name="y">The top left y placement of the text</param>
        /// <param name="size">The sizeof the text</param>
        /// <param name="text">The text to be displaced</param>
        /// <returns>The path of the created text.</returns>
        public Path AddText(int x, int y, int size, string text)
        {
            VisualCollection Visuals = new VisualCollection(this);
            FormattedText formattedText = new FormattedText(text, Thread.CurrentThread.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Times New Roman"), size, Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip);
            formattedText.SetFontWeight(FontWeights.Normal);
            Geometry formattedTextGeometry = formattedText.BuildGeometry(new Point(x, y));
            GeometryGroup geometryGroup = new GeometryGroup();
            geometryGroup.Children.Add(formattedTextGeometry);
            Path path = new Path
            {
                //Stroke thickness controls the thickness of the resulting text
                StrokeThickness = 0.1,
                Stroke = Brushes.Black,
                Data = geometryGroup
            };
            path.Fill = Brushes.Black;
            return path;
        }
        /// <summary>
        /// This method is used to colored add text to a canvas.
        /// </summary>
        /// <param name="x">The top left x placement of the text</param>
        /// <param name="y">The top left y placement of the text</param>
        /// <param name="size">The sizeof the text</param>
        /// <param name="text">The text to be displaced</param>
        /// <param name="hexColor">The hexcolor of the created text</param>
        /// <returns>The path of the created text.</returns>
        public Path AddText(int x, int y, int size, string text, string hexColor)
        {
            VisualCollection Visuals = new VisualCollection(this);
            FormattedText formattedText = new FormattedText(text, Thread.CurrentThread.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Times New Roman"), size, Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip);
            formattedText.SetFontWeight(FontWeights.Normal);
            Geometry formattedTextGeometry = formattedText.BuildGeometry(new Point(x, y));
            GeometryGroup geometryGroup = new GeometryGroup();
            geometryGroup.Children.Add(formattedTextGeometry);
            Path path = new Path
            {
                //Stroke thickness controls the thickness of the resulting text
                StrokeThickness = 0.1,
                Stroke = (SolidColorBrush)new BrushConverter().ConvertFrom(hexColor),
                Data = geometryGroup
            };
            path.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom(hexColor);
            return path;
        }
        /// <summary>
        /// Creates the path of the bloom filter boolean array. It either creates the true values
        /// or the false values. 
        /// </summary>
        /// <param name="x">The x placement of the top left corner of the boolean array of the 
        /// bloom filter</param>
        /// <param name="y">The y placement of the top left corner of the boolean array of the 
        /// bloom fiter</param>
        /// <param name="xLength">The x length of the boolean array</param>
        /// <param name="yLength">The y length of the boolean array</param>
        /// <param name="margin">The margin used in the Bloom filter Boolean array.</param>
        /// <param name="array">The array which is being visualized.</param>
        /// <param name="b">The parts of the boolean array which are being visualized</param>
        /// <returns>The given parts of the bloom filter boolean array.</returns>
        public Path AddBloomFilter(double x, double y, double xLength, double yLength, double margin, bool[] array, bool b)
        {
            GeometryGroup geometryGroup = new GeometryGroup();

            double xmargin = margin;

            if (!b)
            {
                margin *= 5;
            }
            double x0 = x;
            double x2 = x + xLength;

            for (int i = 0; i < array.Length; i++)
            {
                x0 = x2 - ((x2 - x) / array.Length * i);

                if (array[i] == b)
                {
                    geometryGroup.Children.Add(new LineGeometry(new Point(x0, y + margin * 2), new Point(x0, y - margin * 2 + yLength)));
                }
            }

            Path path = new Path
            {
                StrokeThickness = 1
            };
            if (b)
            {
                path.Stroke = (SolidColorBrush)new BrushConverter().ConvertFrom("#F01F2B");
            }
            else
            {
                path.Stroke = Brushes.Black;
            }

            path.Data = geometryGroup;

            return path;
        }
        /// <summary>
        /// This method adds one stroke of the boolean arary which is being created through the
        /// falsificaion of the orignal bloom filter boolean array. This line is drawn dynamically
        /// in accordance to the result of the randomized response for the length and the color
        /// of the line. 
        /// </summary>
        /// <param name="x">The x placement of the top left of the drawn line</param>
        /// <param name="y">The y placement of the top left of the drawn line</param>
        /// <param name="xLength">The x length of the drawn line</param>
        /// <param name="yLength">The y length of the drawn line</param>
        /// <param name="margin">The margin used for drawing the line</param>
        /// <param name="iterationMax">The maximum amount of iterations</param>
        /// <param name="iValue">The i value of this line</param>
        /// <param name="value">The value of this line</param>
        /// <param name="arrayLength">The length of the array</param>
        /// <param name="gradCol">The color of the line</param>
        /// <param name="max">The maximum possible value</param>
        /// <param name="min">The minimum possible value</param>
        /// <returns>The drawn line with a specific color and length</returns>
        public Path AddRandomizedResponseStroke(double x, double y, double xLength, double yLength, double margin, double iterationMax, double iValue, double value, double arrayLength, GradientStopCollection gradCol, double max, double min)
        {
            GeometryGroup geometryGroup = new GeometryGroup();
            double yLengthClear = yLength - (4 * margin);
            double valueMargin = ((double)yLengthClear * ((max - min) - (value - min)) / max);
            double height = ((value / max) * 40) + 10;//40 increased to 50
            if (height > 50)//increased to 60
            {
                height = 50;
            }
            else if (height < 10)
            {
                height = 10;
            }
            double x0 = x;
            double x2 = x + xLength;
            x0 = x2 - ((x2 - x) / arrayLength * iValue);
            geometryGroup.Children.Add(new LineGeometry(new Point(x0, y + height), new Point(x0, y - height)));

            double a = (double)value / (double)max;
            Path path = new Path
            {
                StrokeThickness = 1
            };
            if (value == iterationMax)
            {
                path.Stroke = Brushes.Red;
            }
            else if (value == 0)
            {
                path.Stroke = Brushes.Black;
            }
            else
            {
                path.Stroke = new SolidColorBrush(gradCol.GetRelativeColor(a));
            }
            path.Data = geometryGroup;
            return path;
        }


    }
}
/// <summary>
/// https://stackoverflow.com/questions/9650049/get-color-in-specific-location-on-gradient Gradient collector sourced from here.
/// This class is used to dynamically decide the color of the line in the arrays which represent the heatmaps.
/// </summary>
public static class GradientStopCollectionExtensions
{
    public static Color GetRelativeColor(this GradientStopCollection gStop, double offset)
    {
        GradientStop point = gStop.SingleOrDefault(f => f.Offset == offset);
        if (point != null)
        {
            return point.Color;
        }
        GradientStop before = gStop.Where(w => w.Offset == gStop.Min(m => m.Offset)).First();
        GradientStop after = gStop.Where(w => w.Offset == gStop.Max(m => m.Offset)).First();

        foreach (GradientStop gs in gStop)
        {
            if (gs.Offset < offset && gs.Offset > before.Offset)
            {
                before = gs;
            }
            if (gs.Offset > offset && gs.Offset < after.Offset)
            {
                after = gs;
            }
        }

        Color color = new Color
        {
            ScA = (float)((offset - before.Offset) * (after.Color.ScA - before.Color.ScA) / (after.Offset - before.Offset) + before.Color.ScA),
            ScR = (float)((offset - before.Offset) * (after.Color.ScR - before.Color.ScR) / (after.Offset - before.Offset) + before.Color.ScR),
            ScG = (float)((offset - before.Offset) * (after.Color.ScG - before.Color.ScG) / (after.Offset - before.Offset) + before.Color.ScG),
            ScB = (float)((offset - before.Offset) * (after.Color.ScB - before.Color.ScB) / (after.Offset - before.Offset) + before.Color.ScB)
        };

        return color;
    }
}