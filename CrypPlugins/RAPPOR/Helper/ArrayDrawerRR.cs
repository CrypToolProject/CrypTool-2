using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RAPPOR.Helper
{
    /// <summary>
    /// This class is used to draw the parts of the canvas for the randomized response view of the
    /// component.
    /// </summary>
    internal class ArrayDrawerRR
    {
        public ArrayDrawerRR()
        {

        }
        /// <summary>
        /// This method draws a rectangle of a specific color and size.
        /// </summary>
        /// <param name="x">The top left x location of the rectangle.</param>
        /// <param name="y">The top left y location of the rectangle.</param>
        /// <param name="xLength">The x length of the rectanlge.</param>
        /// <param name="yLength">The y length of the rectangle</param>
        /// <param name="hexcolor">The color of the rectange as a hexcolor string</param>
        /// <returns>returns the path of the drawn rectangle.</returns>
        public Path AddRectangle(int x, int y, double xLength, double yLength, string hexcolor)
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
        /// This method draws a line for a canvas.
        /// </summary>
        /// <param name="x1">The x start of the line</param>
        /// <param name="y1">The y start of the line</param>
        /// <param name="x2">The x end of the line</param>
        /// <param name="y2">The y end of the line</param>
        /// <returns>Returns the path of the drawn line</returns>
        public Path AddLine(int x1, int y1, int x2, int y2)
        {
            LineGeometry line = new LineGeometry(new Point(x1, y1), new Point(x2, y2));
            GeometryGroup geometryGroup = new GeometryGroup();
            geometryGroup.Children.Add(line);
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
        /// This method creates a circle to be drawn on a canvas.
        /// </summary>
        /// <param name="x">The x position of the center of the circle</param>
        /// <param name="y">The y position of the center of the circle</param>
        /// <param name="radius">The radius of the circle</param>
        /// <param name="hexcolor">The color of the circle as a hexcolor string</param>
        /// <returns></returns>
        public Path AddCircle(int x, int y, int radius, string hexcolor)
        {
            GeometryGroup geometryGroup = new GeometryGroup();

            geometryGroup.Children.Add(new EllipseGeometry(new Point(x, y), radius, radius));

            Path path = new Path
            {
                StrokeThickness = 1,
                Stroke = (SolidColorBrush)new BrushConverter().ConvertFrom(hexcolor),
                Data = geometryGroup
            };
            path.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom(hexcolor);
            return path;

        }
        /// <summary>
        /// This is method is used to draw a stroked line on a canvas.
        /// </summary>
        /// <param name="x1">The x start of the line</param>
        /// <param name="y1">The y start of the line</param>
        /// <param name="x2">The x end of the line</param>
        /// <param name="y2">The y end of the line</param>
        /// <param name="stroke">The stoke of the line</param>
        /// <param name="hexcolor">The color of the line a hexcolor string</param>
        /// <returns>Returns the drawn line.</returns>
        public Path AddStrokedLine(int x1, int y1, int x2, int y2, int stroke, string hexcolor)
        {
            GeometryGroup geometryGroup = new GeometryGroup();
            int x = x1;
            while (x <= x2)
            {
                LineGeometry line = new LineGeometry(new Point(x, y1), new Point(x + stroke, y2));
                geometryGroup.Children.Add(line);
                x += 2 * stroke;
            }

            Path path = new Path
            {
                StrokeThickness = 1,
                Stroke = (SolidColorBrush)new BrushConverter().ConvertFrom(hexcolor),
                Data = geometryGroup
            };
            path.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom(hexcolor);
            return path;
        }
    }
}
