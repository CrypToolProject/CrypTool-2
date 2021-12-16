using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HexBox
{
    public class HexText : Control
    {

        private double charwidth;
        public Brush brush = Brushes.Orange;
        public int[] mark = { 0, 0 };

        public bool removemarks;


        public double CharWidth => charwidth;

        public HexText()
        {
            FontFamily = new FontFamily("Consolas");
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            string tempString = "";
            for (int i = 0; i < ByteContent.Count(); i++)
            {
                tempString += string.Format("{0:X2}", ByteContent[i]);
                tempString += " ";
            }



            //Console.WriteLine(""+tempString.Count());

            FormattedText formattedText = new FormattedText(
                tempString,
                CultureInfo.GetCultureInfo("en-us"),
                FlowDirection.LeftToRight,
                new Typeface(FontFamily, FontStyle, FontWeight, FontStretches.Normal),
                13,
                Brushes.Black)
            {
                MaxTextWidth = 340,
                LineHeight = 20,

                Trimming = TextTrimming.None
            };

            Point p = new Point();

            p = new Point(0, -4);

            //Console.WriteLine("" + formattedText.WidthIncludingTrailingWhitespace);


            int f = ByteContent.Count() * 3;

            if (f > 48)
            {
                f = 48;
            }


            charwidth = formattedText.WidthIncludingTrailingWhitespace / f;

            bool outOfScreen = false;

            if (mark[0] < 0)
            {
                mark[0] = 0;
                outOfScreen = false;
            }

            if (mark[1] < 0)
            {
                mark[1] = 0;
                outOfScreen = true;
            }

            if (mark[0] > 1024)
            {
                mark[0] = 1024;
                outOfScreen = true;
            }

            if (mark[1] > 1024)
            {
                mark[1] = 1024;
                outOfScreen = false;
            }


            if (!removemarks && !outOfScreen)
            {
                double veroff = 2;
                if (mark[0] < mark[1])
                {

                    double y = mark[0] / 32 * 20 - veroff;
                    double x = mark[0] % 32 * charwidth * 3 / 2;
                    double z = mark[1] % 32 * charwidth * 3 / 2 - x;

                    double z2 = 48 * charwidth - x - charwidth;

                    if (z < 0)
                    {
                        z = 0;
                    }

                    double y1 = mark[1] / 32 * 20 - veroff;
                    double x1 = 0;
                    double z1 = mark[1] % 32 * charwidth * 3 / 2;

                    if (z1 < 0)
                    {
                        z1 = 0;
                    }

                    if (z2 < 0)
                    {
                        z2 = 0;
                    }

                    if (mark[0] % 32 > mark[1] % 32 || mark[1] - mark[0] >= 32)
                    {
                        drawingContext.DrawRectangle(brush, new Pen(brush, 1.0),
                                                     new Rect(x, y, z2, 20));
                        if (z1 > 2)
                        {
                            drawingContext.DrawRectangle(brush, new Pen(brush, 1.0),
                                                         new Rect(x1, y1, z1, 20));
                        }
                        int v = mark[1] / 32 - mark[0] / 32;

                        for (int i = 1; i < v; i++)
                        {
                            double y3 = y + i * 20;
                            drawingContext.DrawRectangle(brush, new Pen(brush, 1.0),
                                                         new Rect(0, y3, 47 * charwidth, 20));
                        }

                    }
                    else
                    {
                        drawingContext.DrawRectangle(brush, new Pen(brush, 1.0),
                                                     new Rect(x, y, z, 20));
                    }

                }

                else
                {
                    double y = mark[0] / 32 * 20 - veroff;
                    double x = mark[1] % 32 * charwidth * 3 / 2;
                    double z = mark[0] % 32 * charwidth * 3 / 2 - x;

                    double z2 = mark[0] % 32 * charwidth * 3 / 2;

                    if (z < 0)
                    {
                        z = 0;
                    }

                    double y1 = mark[1] / 32 * 20 - veroff;

                    double z1 = mark[1] % 32 * charwidth * 3 / 2;
                    double x1 = 47 * charwidth - z1;


                    if (z1 < 0)
                    {
                        z1 = 0;
                    }

                    if (z2 < 0)
                    {
                        z2 = 0;
                    }




                    if (mark[0] % 32 < mark[1] % 32 || mark[0] - mark[1] >= 32)
                    {
                        drawingContext.DrawRectangle(brush, new Pen(brush, 1.0),
                                                     new Rect(0, y, z2, 20));
                        if (z1 > 2)
                        {
                            drawingContext.DrawRectangle(brush, new Pen(brush, 1.0),
                                                         new Rect(z1, y1, x1, 20));
                        }
                        int v = mark[0] / 32 - mark[1] / 32;

                        for (int i = 1; i < v; i++)
                        {
                            double y3 = y1 + i * 20;
                            drawingContext.DrawRectangle(brush, new Pen(brush, 1.0),
                                                         new Rect(0, y3, 47 * charwidth, 20));
                        }

                    }
                    else
                    {
                        if (mark[0] != mark[1])
                        {
                            drawingContext.DrawRectangle(brush, new Pen(brush, 1.0),
                                                     new Rect(x, y, z, 20));
                        }
                    }
                }
            }
            drawingContext.DrawText(formattedText, p);





            //Console.WriteLine(this.RenderSize);
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public void appendText(string text)
        {
            Text += text;
        }

        public byte[] ByteContent
        {
            get => (byte[])GetValue(ByteProperty);
            set => SetValue(ByteProperty, value);
        }

        private static readonly byte[] b = { };

        public static readonly DependencyProperty ByteProperty =
            DependencyProperty.Register("ByteContent",
            typeof(byte[]),
            typeof(HexText),
            new FrameworkPropertyMetadata(b, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text",
            typeof(string),
            typeof(HexText),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender));
    }
}
