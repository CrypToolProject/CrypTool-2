using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RAPPOR.Helper.ArrayDrawer
{
    /// <summary>
    /// Provides the methods for programatically drawing every part of the RAPPOR views Overview
    /// view and Bloomfilter view which is not beeing handled in the xaml view.
    /// </summary>
    public class ArrayDrawer : FrameworkElement
    {
        /// <summary>
        /// Initializes a new instance of the Array Drawer class.
        /// </summary>
        public ArrayDrawer()
        {
        }

        /// <summary>
        /// This method is used to create the static light gray parts of the Bloom filter canvas.
        /// These parts do not change in the animation.
        /// </summary>
        /// <returns>Path with all static light gray components</returns>
        public Path CreateBloomFilterStaticPathLightGray()
        {
            //Rectangles
            RectangleGeometry leftRectangleGeometry = new RectangleGeometry
            {
                Rect = new Rect(100, 50, 150, 150),
                RadiusX = 10,
                RadiusY = 10
            };
            RectangleGeometry rightRectangleGeometry = new RectangleGeometry
            {
                Rect = new Rect(550, 50, 150, 150),
                RadiusX = 10,
                RadiusY = 10
            };

            //Arrow
            StreamGeometry arrowGeometry = new StreamGeometry
            {
                FillRule = FillRule.EvenOdd
            };
            using (StreamGeometryContext sgc = arrowGeometry.Open())
            {
                sgc.BeginFigure(new Point(455, 125), true, true);
                //475,125
                sgc.LineTo(new Point(415, 80), true, false);
                //425, 75
                sgc.LineTo(new Point(415, 105), true, false);
                //425,95
                sgc.LineTo(new Point(325, 105), true, false);
                //325,95
                sgc.LineTo(new Point(325, 145), true, false);
                //325,155
                sgc.LineTo(new Point(415, 145), true, false);
                //425,155
                sgc.LineTo(new Point(415, 170), true, false);
                //425,175
            }
            arrowGeometry.Freeze();

            PathFigure myPathFigure = new PathFigure
            {
                StartPoint = new Point(645, 200)
            };
            //Half circle at the bottom ofthe hash function box
            myPathFigure.Segments.Add(new ArcSegment(new Point(605, 200), new Size(10, 10), 45, true, SweepDirection.Clockwise, true));

            /// Create a PathGeometry to contain the figure.
            PathGeometry halfCircle = new PathGeometry();
            halfCircle.Figures.Add(myPathFigure);

            GeometryGroup geometryGroup = new GeometryGroup();
            geometryGroup.Children.Add(leftRectangleGeometry);
            geometryGroup.Children.Add(rightRectangleGeometry);
            geometryGroup.Children.Add(arrowGeometry);
            geometryGroup.Children.Add(halfCircle);

            Path path = new Path
            {
                StrokeThickness = 1,
                Stroke = Brushes.Black,
                Data = geometryGroup
            };
            path.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#f2f2f2");

            return path;
        }

        /// <summary>
        /// This method is used to create the static black parts of the Bloom filter canvas.
        /// These parts do not change in the animation.
        /// </summary>
        /// <param name="boolArray">An instance of the boolean array. containing
        /// the current Bloom filter.</param>
        /// <param name="bloomFilter">An instance of the Bloom filter which is currently in
        /// use.</param>
        /// <param name="i">Current iteration of the input string which is being used.</param>
        /// <param name="input">Input string of the component.</param>
        /// <param name="roundVar">The round which in which this method is called.</param>
        /// <returns>Path with all static black components.</returns>
        public Path CreateBloomFilterPathBlack(bool[] boolArray, Model.BloomFilter bloomFilter, string input,
            int roundVar)
        {
            GeometryGroup geometryGroup = new GeometryGroup();

            //Creating the corners for the array of the bloom filter.
            StreamGeometry topLeftCorner = new StreamGeometry
            {
                FillRule = FillRule.EvenOdd
            };
            using (StreamGeometryContext sgc = topLeftCorner.Open())
            {
                sgc.BeginFigure(new Point(90, 310), true, true);
                sgc.LineTo(new Point(90, 320), true, false);
                sgc.LineTo(new Point(92, 318), true, false);
                sgc.LineTo(new Point(92, 312), true, false);
                sgc.LineTo(new Point(98, 312), true, false);
                sgc.LineTo(new Point(100, 310), true, false);
            }

            topLeftCorner.Freeze();
            StreamGeometry bottomLeftCorner = new StreamGeometry
            {
                FillRule = FillRule.EvenOdd
            };
            using (StreamGeometryContext sgc = bottomLeftCorner.Open())
            {
                sgc.BeginFigure(new Point(90, 370), true, true);
                sgc.LineTo(new Point(100, 370), true, false);
                sgc.LineTo(new Point(98, 368), true, false);
                sgc.LineTo(new Point(92, 368), true, false);
                sgc.LineTo(new Point(92, 362), true, false);
                sgc.LineTo(new Point(90, 360), true, false);
            }

            bottomLeftCorner.Freeze();
            StreamGeometry bottomRightCorner = new StreamGeometry
            {
                FillRule = FillRule.EvenOdd
            };
            using (StreamGeometryContext sgc = bottomRightCorner.Open())
            {
                sgc.BeginFigure(new Point(710, 370), true, true);
                sgc.LineTo(new Point(710, 360), true, false);
                sgc.LineTo(new Point(708, 362), true, false);
                sgc.LineTo(new Point(708, 368), true, false);
                sgc.LineTo(new Point(702, 368), true, false);
                sgc.LineTo(new Point(700, 370), true, false);
            }

            bottomRightCorner.Freeze();
            StreamGeometry topRightCorner = new StreamGeometry
            {
                FillRule = FillRule.EvenOdd
            };
            using (StreamGeometryContext ctx = topRightCorner.Open())
            {
                ctx.BeginFigure(new Point(710, 310), true /* is filled */, true /* is closed */);
                ctx.LineTo(new Point(710, 320), true /* is stroked */, false /* is smooth join */);
                ctx.LineTo(new Point(708, 318), true /* is stroked */, false /* is smooth join */);
                ctx.LineTo(new Point(708, 312), true, false); //140
                ctx.LineTo(new Point(702, 312), true, false);
                ctx.LineTo(new Point(700, 310), true, false);
            }

            topRightCorner.Freeze();

            //Creating the title text of the data set box, the calculation arrow structure and the hash function box.
            FormattedText formattedTextDataset = new FormattedText(CrypTool.Plugins.RAPPOR.Properties.Resources.DataSet,
                Thread.CurrentThread.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Times New Roman"), 16,
                Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip);
            Geometry formattedTextDatasetGeometry = formattedTextDataset.BuildGeometry(new Point(102, 32));
            FormattedText formattedTextInput = new FormattedText(CrypTool.Plugins.RAPPOR.Properties.Resources.Input + " (" + (roundVar + 1) + "/" + input.Split(',').Length + ")",
                Thread.CurrentThread.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Times New Roman"), 16,
                Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip);
            Geometry formattedTextInputGeometry = formattedTextInput.BuildGeometry(new Point(320, 77));
            FormattedText formattedTextHashFunctions = new FormattedText(
                CrypTool.Plugins.RAPPOR.Properties.Resources.Hashfunctions, Thread.CurrentThread.CurrentCulture,
                FlowDirection.LeftToRight, new Typeface("Times New Roman"), 16, Brushes.Black,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);
            Geometry formattedTextHashFunctionsGeometry = formattedTextHashFunctions.BuildGeometry(new Point(552, 32));

            //Creating the hash functions with the calculated values.
            string hashString = "";
            string infoString = "";
            for (int i = 0; i < bloomFilter.GetHistory()[0].Length; i++)
            {
                hashString += CrypTool.Plugins.RAPPOR.Properties.Resources.h;
                StringBuilder tempStringBuilder = new StringBuilder();
                tempStringBuilder.Append(i + 1);
                for (int j = 0; j < tempStringBuilder.Length; j++)
                {
                    string x = "";
                    switch (tempStringBuilder[j])
                    {
                        case '0':
                            x = "\u2080";
                            break;
                        case '1':
                            x = "\u2081";
                            break;
                        case '2':
                            x = "\u2082";
                            break;
                        case '3':
                            x = "\u2083";
                            break;
                        case '4':
                            x = "\u2084";
                            break;
                        case '5':
                            x = "\u2085";
                            break;
                        case '6':
                            x = "\u2086";
                            break;
                        case '7':
                            x = "\u2087";
                            break;
                        case '8':
                            x = "\u2088";
                            break;
                        case '9':
                            x = "\u2089";
                            break;
                        default:
                            break;
                    }

                    hashString += x;

                }

                hashString += ": ";
                if (roundVar >= 0 && input != "")
                {
                    hashString += bloomFilter.GetHistory()[roundVar][i].ToString();
                    //The infoString is used to display relevant information for the current step later
                    infoString += bloomFilter.GetHistory()[roundVar][i].ToString();
                }

                hashString += "\n";
                infoString += ", ";
            }
            infoString = string.Concat(infoString.Reverse().Skip(2).Reverse());
            infoString += " ";

            FormattedText formattedTextHashfunctionsContent = new FormattedText(hashString,
                Thread.CurrentThread.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Times New Roman"), 16,
                Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip)
            {
                MaxTextWidth = 140,
                MaxTextHeight = 140
            };

            Geometry formattedTextHashfunctionsContentGeometry =
                formattedTextHashfunctionsContent.BuildGeometry(new Point(555, 55));

            //Creating the data which is displayed in the data set box.
            string[] inputArray = input.Split(',');
            string reducedString = "";
            for (int j = 0; j < inputArray.Length; j++)
            {
                if (j >= roundVar && input != "")
                {
                    if (j != 0 || j == inputArray.Length + 1)// alles nach || != 0 hinzugefügt
                    {
                        if (reducedString != "")
                        {
                            reducedString += ", ";
                        }

                    }
                    reducedString += inputArray[j];
                }
            }

            if (reducedString.Length > 0)
            {
                reducedString.Remove(reducedString.Length - 1);
            }

            //Remove last piece of data
            if (reducedString.Length <= 2)
            {
                reducedString = "";
            }


            FormattedText formattedTextInputContent = new FormattedText(reducedString,
                Thread.CurrentThread.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Times New Roman"), 16,
                Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip)
            {
                MaxTextWidth = 140,
                MaxTextHeight = 140
            };
            Geometry formattedTextInputContentGeometry = formattedTextInputContent.BuildGeometry(new Point(105, 55));

            if (roundVar >= 0)
            {
                FormattedText formattedTextArrowContent = new FormattedText(input.Split(',')[roundVar],
                    Thread.CurrentThread.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Times New Roman"), 16,
                    Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip)
                {
                    MaxTextWidth = 95
                };
                Geometry formattedTextArrowContentGeometry =
                    formattedTextArrowContent.BuildGeometry(new Point(330, 117));
                geometryGroup.Children.Add(formattedTextArrowContentGeometry);
            }

            if (roundVar >= 0)
            {
                //Determining the word end of the ordinal number in the following sentence.
                string ordinalWordEnd;

                switch(roundVar + 1)
                {
                    case 1:
                        ordinalWordEnd = CrypTool.Plugins.RAPPOR.Properties.Resources.ordinalOne;
                        break;
                    case 2:
                        ordinalWordEnd = CrypTool.Plugins.RAPPOR.Properties.Resources.ordinalTwo;
                        break;
                    case 3:
                        ordinalWordEnd = CrypTool.Plugins.RAPPOR.Properties.Resources.ordinalThree;
                        break;
                    default:
                        ordinalWordEnd = CrypTool.Plugins.RAPPOR.Properties.Resources.ordinalRest;
                        break;
                }

                //Bottom line describing what is happening rn:
                //Old Structure: //string infoTextCombination = CrypTool.Plugins.RAPPOR.Properties.Resources.BloomFilterInformation1 + input.Split(',')[(roundVar)] + CrypTool.Plugins.RAPPOR.Properties.Resources.BloomFilterInformation2 + infoString + CrypTool.Plugins.RAPPOR.Properties.Resources.BloomFilterInformation3 + (roundVar + 1) + CrypTool.Plugins.RAPPOR.Properties.Resources.BloomFilterInformation4;
                string infoTextCombination = CrypTool.Plugins.RAPPOR.Properties.Resources.BloomFilterInformation1 + input.Split(',')[(roundVar)] + CrypTool.Plugins.RAPPOR.Properties.Resources.BloomFilterInformation2 + input.Split(',')[(roundVar)] + ordinalWordEnd + CrypTool.Plugins.RAPPOR.Properties.Resources.BloomFilterInformation3 + infoString + CrypTool.Plugins.RAPPOR.Properties.Resources.BloomFilterInformation4;
                FormattedText formattedTextInformation = new FormattedText(infoTextCombination,
        Thread.CurrentThread.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Times New Roman"), 16,
        Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip)
                {
                    //The Input [string(roundvar)] sets the cells [infostring] in step [roundvar+2]
                    MaxTextHeight = 100,
                    MaxTextWidth = 600
                };
                Geometry formattedTextInformationGeometry = formattedTextInformation.BuildGeometry(new Point(100, 400));
                geometryGroup.Children.Add(formattedTextInformationGeometry);
            }


            geometryGroup.Children.Add(topLeftCorner);
            geometryGroup.Children.Add(bottomLeftCorner);
            geometryGroup.Children.Add(bottomRightCorner);
            geometryGroup.Children.Add(topRightCorner);
            geometryGroup.Children.Add(formattedTextDatasetGeometry);
            geometryGroup.Children.Add(formattedTextInputGeometry);
            geometryGroup.Children.Add(formattedTextHashFunctionsGeometry);
            geometryGroup.Children.Add(formattedTextHashfunctionsContentGeometry);
            geometryGroup.Children.Add(formattedTextInputContentGeometry);




            Path path = new Path
            {
                StrokeThickness = 0.1,//Changed 1 to 0.1 here
                Stroke = Brushes.Black,
                Data = geometryGroup
            };
            path.Fill = Brushes.Black;

            return path;
        }
        /// <summary>
        /// Creates multiple arrays of boolean arrays on a canvas. Then it returns the canvas.
        /// </summary>
        /// <param name="boolArray">The Bloom filter, permanent randomized response and a user hoosesn amount of instantaneous randomized responses.</param>
        /// <returns>A canvas with the drawn arrays placed on it.</returns>
        public Canvas createArrayCanvas(bool[][] boolArray)
        {
            Canvas myCanvas = new Canvas
            {
                Width = 650,//512
                Height = 35,//35
                Background = Brushes.Transparent
            };
            Path path = new Path();
            int j;

            //Drawing the dynamic linear
            GeometryGroup geom = new GeometryGroup();
            geom.Children.Add(new LineGeometry(new Point(0, 0), new Point(myCanvas.Width / boolArray[0].Length * (boolArray[0].Length - 1), 0)));
            geom.Children.Add(new LineGeometry(new Point(0, 10), new Point(0, 0)));
            FormattedText linearNumberTextMax = new FormattedText((boolArray[0].Length - 1).ToString(), Thread.CurrentThread.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Times New Roman"), 10, Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip);
            Geometry linearNumberGeometryMax = linearNumberTextMax.BuildGeometry(new Point(-3, -12));
            geom.Children.Add(linearNumberGeometryMax);
            geom.Children.Add(new LineGeometry(new Point(myCanvas.Width / boolArray[0].Length * (boolArray[0].Length - 1), 10), new Point(myCanvas.Width / boolArray[0].Length * (boolArray[0].Length - 1), 0)));
            FormattedText linearNumberTextZero = new FormattedText("0", Thread.CurrentThread.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Times New Roman"), 10, Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip);
            Geometry linearNumberGeometryZero = linearNumberTextZero.BuildGeometry(new Point(myCanvas.Width / boolArray[0].Length * (boolArray[0].Length - 1) - 3, -12));
            geom.Children.Add(linearNumberGeometryZero);
            if (boolArray[0].Length <= 32)
            {
                //for loop from 0 to booleanarray length.
                for (int i = 0; i < boolArray[0].Length; i++)
                {
                    geom.Children.Add(new LineGeometry(new Point(myCanvas.Width / boolArray[0].Length * i, 5), new Point(myCanvas.Width / boolArray[0].Length * i, 0)));
                    FormattedText linearNumberText = new FormattedText((boolArray[0].Length - i - 1).ToString(), Thread.CurrentThread.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Times New Roman"), 10, Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip);
                    Geometry linearNumberGeometry = linearNumberText.BuildGeometry(new Point(myCanvas.Width / boolArray[0].Length * i - 3, -12));
                    geom.Children.Add(linearNumberGeometry);
                }
            }
            else if (boolArray[0].Length < 129)
            {
                int tempVar = 1;
                for (int i = 0; i < boolArray[0].Length; i++)
                {
                    if (myCanvas.Width / boolArray[0].Length * i > myCanvas.Width / 32 * tempVar)
                    {
                        geom.Children.Add(new LineGeometry(new Point(myCanvas.Width / boolArray[0].Length * i, 5), new Point(myCanvas.Width / boolArray[0].Length * i, 0)));
                        FormattedText linearNumberText = new FormattedText((boolArray[0].Length - i - 1).ToString(), Thread.CurrentThread.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Times New Roman"), 10, Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip);
                        Geometry linearNumberGeometry = linearNumberText.BuildGeometry(new Point(myCanvas.Width / boolArray[0].Length * i - 3, -12));
                        geom.Children.Add(linearNumberGeometry);
                        tempVar++;
                    }
                }
            }
            else
            {
                int tempVar = 1;
                for (int i = 0; i < boolArray[0].Length; i++)
                {
                    //15 because 0 is extra
                    if (myCanvas.Width / boolArray[0].Length * i > myCanvas.Width / 15 * tempVar)
                    {
                        geom.Children.Add(new LineGeometry(new Point(myCanvas.Width / boolArray[0].Length * i, 5), new Point(myCanvas.Width / boolArray[0].Length * i, 0)));
                        FormattedText linearNumberText = new FormattedText((boolArray[0].Length - i - 1).ToString(), Thread.CurrentThread.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Times New Roman"), 10, Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip);
                        Geometry linearNumberGeometry = linearNumberText.BuildGeometry(new Point(myCanvas.Width / boolArray[0].Length * i - 3, -12));
                        geom.Children.Add(linearNumberGeometry);
                        tempVar++;
                    }
                }
            }
            path = new Path
            {
                StrokeThickness = 1,
                Stroke = Brushes.Black,
                Data = geom
            };
            myCanvas.Children.Add(path);

            //Drawing the lineas where the bloom filter had a correct entry
            geom = new GeometryGroup();
            j = 0;
            for (double x = 0; x < myCanvas.Width; x += myCanvas.Width / boolArray[1].Length)
            {
                if (boolArray[0][j])
                {
                    for (int i = (int)myCanvas.Height; i < myCanvas.Height * boolArray.Length; i += 4)
                    {
                        geom.Children.Add(new LineGeometry(new Point(x, i), new Point(x, i + 2)));
                    }

                }
                j++;
                if (j >= boolArray[0].Length)
                {
                    break;
                }

            }
            path = new Path
            {
                StrokeThickness = 1,
                Stroke = Brushes.LightPink,
                Data = geom
            };
            myCanvas.Children.Add(path);

            //Drawing the false entrys of the boolean array
            for (int i = 0; i < boolArray.Length; i++)
            {
                //Drawing the false entrys of the boolean array
                double margin = 5; //Black lines are shorter for clarity
                                   //double xmin = margin;
                double xmax = myCanvas.Width - margin;
                double ymax = myCanvas.Height * (i + 1) - margin;
                geom = new GeometryGroup();
                j = 0;
                for (double x = 0; x < myCanvas.Width; x += myCanvas.Width / boolArray[i].Length)
                {
                    if (!boolArray[i][j])
                    {
                        geom.Children.Add(new LineGeometry(new Point(x, ymax - margin / 2), new Point(x, ymax + margin / 2)));
                    }
                    j++;
                    if (j >= boolArray[0].Length)
                    {
                        break;
                    }
                }

                path = new Path
                {
                    StrokeThickness = 1,
                    Stroke = Brushes.Black,
                    Data = geom
                };
                myCanvas.Children.Add(path);
                //Drawing the correct entrys of the boolean entrys
                margin = 10;
                geom = new GeometryGroup();
                j = 0;
                for (double x = 0; x < myCanvas.Width; x += myCanvas.Width / boolArray[i].Length)
                {
                    if (boolArray[i][j])
                    {
                        geom.Children.Add(new LineGeometry(new Point(x, ymax - margin / 2), new Point(x, ymax + margin / 2)));
                    }
                    j++;
                    if (j >= boolArray[0].Length)
                    {
                        break;
                    }

                }

                path = new Path
                {
                    StrokeThickness = 1,
                    Stroke = (SolidColorBrush)new BrushConverter().ConvertFrom("#F01F2B"),
                    Data = geom
                };

                myCanvas.Children.Add(path);
            }


            return myCanvas;
        }

        /// <summary>
        /// This method creates a single boolean array on a canvas and returns that canvas.
        /// </summary>
        /// <param name="boolArray">The boolean array which is supposed to be turned into  a canvas.</param>
        /// <returns>Returns a canvas with  a single boolean array.</returns>
        public Canvas createArrayCanvas(bool[] boolArray)
        {
            Canvas myCanvas = new Canvas
            {
                Name = "canGraph",
                Width = 475,
                Height = 100
            };


            // Draw a simple graph. 
            double margin = 5; //Black lines are shorter for an easier overview
            double ymax = myCanvas.Height - margin;
            GeometryGroup geom = new GeometryGroup();
            int i = 0;
            for (double x = 0; x < myCanvas.Width; x += myCanvas.Width / boolArray.Length)
            {
                if (boolArray[i] == false)
                {
                    geom.Children.Add(new LineGeometry(new Point(x, ymax - margin / 2), new Point(x, ymax + margin / 2)));
                }
                i++;
            }

            Path path = new Path
            {
                StrokeThickness = 1,
                Stroke = Brushes.Black,
                Data = geom
            };
            myCanvas.Children.Add(path);

            margin = 10;
            geom = new GeometryGroup();
            i = 0;
            for (double x = 0; x < myCanvas.Width; x += myCanvas.Width / boolArray.Length)
            {
                if (boolArray[i] == true)
                {
                    geom.Children.Add(new LineGeometry(new Point(x, ymax - margin / 2), new Point(x, ymax + margin / 2)));
                }
                i++;

            }

            path = new Path
            {
                StrokeThickness = 1,
                Stroke = (SolidColorBrush)new BrushConverter().ConvertFrom("#F01F2B"),
                Data = geom
            };

            myCanvas.Children.Add(path);
            myCanvas.Background = Brushes.Transparent;
            return myCanvas;

        }
        /// <summary>
        /// This method creates a three armed cross which is used to differentiated the different
        /// arrays in the overview view from each other.
        /// </summary>
        /// <param name="x">The x placement of the cross.</param>
        /// <param name="y">The y placement of the cross.</param>
        /// <param name="xLength">The x length of the cross.</param>
        /// <param name="yLength">The y length of the cross.</param>
        /// <param name="hexcolor">The color of the cross as a hexcolor string.</param>
        /// <param name="rightLeft">True for third arm to the right, false for third arm to
        /// the left.</param>
        /// <returns>The path of the line.</returns>
        public Path CreateCross(int x, int y, int xLength, int yLength, string hexcolor, bool rightLeft)
        {
            GeometryGroup geometryGroup = new GeometryGroup();

            LineGeometry line1 = new LineGeometry(new Point(x, y), new Point(x, y - yLength));
            LineGeometry line2 = new LineGeometry(new Point(x, y), new Point(x, y + yLength));
            LineGeometry line3;
            if (rightLeft)
            {
                line3 = new LineGeometry(new Point(x, y), new Point(x + xLength, y));
            }
            else
            {
                line3 = new LineGeometry(new Point(x, y), new Point(x - xLength, y));
            }

            geometryGroup.Children.Add(line1);
            geometryGroup.Children.Add(line2);
            geometryGroup.Children.Add(line3);

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
        /// This method is used to create a stroked line.
        /// </summary>
        /// <param name="x1">x variable of the start of the line.</param>
        /// <param name="x2">x variable of the end of the line.</param>
        /// <param name="y1">y variable of the start of the line.</param>
        /// <param name="y2">y variable of the end of the line.</param>
        /// <param name="stroking"> The distance of the stroking of the line.</param>
        /// <param name="hexcolor">The color of the line as a hexcolor string.</param>
        /// <returns>The path of the line</returns>
        public Path CreateLine(int x1, int x2, int y1, int y2, int stroking, string hexcolor)
        {
            GeometryGroup geometryGroup = new GeometryGroup();

            for (int i = 0; i + stroking < x2; i += stroking * 2)
            {
                LineGeometry line = new LineGeometry(new Point(i, y1), new Point(i + stroking, y1));
                geometryGroup.Children.Add(line);
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
        /// <summary>
        /// This method creates the true or false entries Boolean array of the bloom filter method.
        /// </summary>
        /// <param name="array">The array which is being drawn</param>
        /// <param name="b">The boolean array to decide if the true, or the false entries of the
        /// array are being drawn.</param>
        /// <returns>The path of the drawn boolean array.</returns>
        public Path CreateArray(bool[] array, bool b)
        {
            GeometryGroup geometryGroup = new GeometryGroup();
            double x1 = 120;
            double x2 = 680;
            double y = 340;

            double x = 120;
            int m = b ? 20 : 15;//the Margin
            double j = 16;
            if (array.Length < 32)
            {
                j = array.Length;
            }
            else if (array.Length < 128)
            {
                j = 32;
            }
            //Linear for Array
            if (!b)
            {
                double l = 0;
                for (int i = 0; i < array.Length; i++)
                {
                    x = x2 - ((x2 - x1) / array.Length * i);
                    if (x <= x2 - ((x2 - x1) / j * l))
                    {
                        geometryGroup.Children.Add(new LineGeometry(new Point(x, 385), new Point(x, 385 - 5)));
                        FormattedText formattedText = new FormattedText(i.ToString(), Thread.CurrentThread.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Times New Roman"), 10, Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip);
                        Geometry formattedTextGeometry = formattedText.BuildGeometry(new Point(x - 3, 385 + 2));
                        geometryGroup.Children.Add(formattedTextGeometry);
                        l++;
                    }

                }
                LineGeometry lG1 = new LineGeometry(new Point(x, 385), new Point(x2, 385));
                LineGeometry lG2 = new LineGeometry(new Point(x, 385), new Point(x, 375));
                LineGeometry lG3 = new LineGeometry(new Point(x2, 385), new Point(x2, 375));
                geometryGroup.Children.Add(lG1);
                geometryGroup.Children.Add(lG2);
                geometryGroup.Children.Add(lG3);
            }

            //boolean array
            for (int i = 0; i < array.Length; i++)
            {
                x = x2 - ((x2 - x1) / array.Length * i);

                if (array[i] == b)
                {
                    geometryGroup.Children.Add(new LineGeometry(new Point(x, y + m), new Point(x, y - m)));
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
        /// Creates a dynamic array from the hashfunction box to one entry of the Bloom filter
        /// which has been set in this round.
        /// </summary>
        /// <param name="horizontal">Horizontal height of the entry which has been set in the 
        /// Bloom filter.</param>
        /// <param name="array">Internal representation of the Bloom filter.</param>
        /// <returns>The path of an array from the hashfunction box to one entry which has been
        /// set by the hashfunction in this round.</returns>
        public Path CreateBloomFilterDynamicArrow(int horizontal, bool[] array)
        {
            double x1 = 120;
            double x2 = 680;
            double x = x2 - ((x2 - x1) / array.Length * horizontal);
            StreamGeometry hashArrow = new StreamGeometry
            {
                FillRule = FillRule.EvenOdd
            };

            using (StreamGeometryContext ctx = hashArrow.Open())
            {
                ctx.BeginFigure(new Point(625, 220), false /* is filled */, false /* is closed */);
                ctx.LineTo(new Point(625, 250), true /* is stroked */, false /* is smooth join */);
                ctx.LineTo(new Point(x, 250), true /* is stroked */, false /* is smooth join */);
                ctx.LineTo(new Point(x, 318), true, false);//140
                ctx.LineTo(new Point(x - 2, 316), true, false);
                ctx.LineTo(new Point(x, 318), true, false);
                ctx.LineTo(new Point(x + 3, 314), true, false);
            }
            hashArrow.Freeze();
            LineGeometry lG = new LineGeometry(new Point(x, 318), new Point(x, 314));
            GeometryGroup geometryGroup = new GeometryGroup();
            geometryGroup.Children.Add(lG);
            geometryGroup.Children.Add(hashArrow);

            Path path = new Path
            {
                StrokeThickness = 1,
                Stroke = Brushes.Black,
                Data = geometryGroup
            };
            path.Fill = Brushes.Black;
            return path;

        }

    }
}
