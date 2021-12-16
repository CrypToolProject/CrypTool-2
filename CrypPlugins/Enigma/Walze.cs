using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace CrypTool.Enigma
{
    internal class Walze : Canvas
    {
        #region Variables

        private readonly int model = 3;
        private readonly TextBlock[] tebo = new TextBlock[26];
        private readonly Line[,] larray = new Line[26, 3];
        private readonly List<Line> linesToAnimat = new List<Line>();
        private readonly List<Line> trashList = new List<Line>();
        public double fast = 400;
        private readonly TextBlock[] teboToAnimat = new TextBlock[2];
        public readonly int typ;
        public TextBlock iAm = new TextBlock();
        public bool stop = false;
        private double timecounter = 0.0;
        private bool wrong;
        public int[] umkehrlist;

        public static int[] getWalzeAsInt(int model, int walze)
        {
            int[] value = new int[26];
            for (int i = 0; i < EnigmaCore.rotors[model, walze].Length; i++)
            {
                if (EnigmaCore.reflectors[model, walze].Length > 0)
                {
                    value[i] = EnigmaCore.reflectors[model, walze][i] - 65;
                }
                else
                {
                    value[i] = EnigmaCore.reflectors[model, 0][i] - 65;
                }
            }
            return value;
        }

        #endregion

        #region Storyboard creating
        public Storyboard startanimation()
        {
            timecounter = 0.0;
            Storyboard sb = new Storyboard();

            sb.Children.Add(animateThisTebo(teboToAnimat[0], true));
            DoubleAnimation[] douret = animateThisLine(linesToAnimat[0]);
            sb.Children.Add(douret[0]);
            sb.Children.Add(douret[1]);
            DoubleAnimation[] douret1 = new DoubleAnimation[2];
            if (!wrong)
            {
                douret1 = animateThisLine(linesToAnimat[1]);
            }
            else
            {
                douret1 = animateThisLineReverse(linesToAnimat[1]);
            }

            sb.Children.Add(douret1[0]);
            sb.Children.Add(douret1[1]);
            DoubleAnimation[] douret2 = animateThisLineReverse(linesToAnimat[2]);
            sb.Children.Add(douret2[0]);
            sb.Children.Add(douret2[1]);
            sb.Children.Add(animateThisTebo(teboToAnimat[1], false));

            return sb;

        }

        public int umkehrlist0(int x, bool off)
        {
            resetColors();
            if (off)
            {
                tebo[x].Background = Brushes.Green;
                tebo[umkehrlist[x]].Background = Brushes.Red;
            }
            teboToAnimat[0] = tebo[x];
            teboToAnimat[1] = tebo[umkehrlist[x]];


            larray[x, 0].Stroke = Brushes.Red;
            larray[x, 1].Stroke = Brushes.Red;
            larray[x, 2].Stroke = Brushes.Red;

            larray[umkehrlist[x], 0].Stroke = Brushes.Green;
            larray[umkehrlist[x], 1].Stroke = Brushes.Green;
            larray[umkehrlist[x], 2].Stroke = Brushes.Green;


            if (umkehrlist[x] > x)
            {
                wrong = true;
                if (larray[x, 1].Parent == this)
                {
                    linesToAnimat.Add(larray[x, 0]);
                    linesToAnimat.Add(larray[x, 1]);
                    linesToAnimat.Add(larray[x, 2]);

                }

                else
                {
                    linesToAnimat.Add(larray[umkehrlist[x], 0]);
                    linesToAnimat.Add(larray[umkehrlist[x], 1]);
                    linesToAnimat.Add(larray[umkehrlist[x], 2]);
                }
            }

            else
            {
                wrong = false;
                if (larray[x, 1].Parent == this)
                {


                    linesToAnimat.Add(larray[x, 2]);
                    linesToAnimat.Add(larray[x, 1]);
                    linesToAnimat.Add(larray[x, 0]);
                }

                else
                {


                    linesToAnimat.Add(larray[umkehrlist[x], 2]);
                    linesToAnimat.Add(larray[umkehrlist[x], 1]);
                    linesToAnimat.Add(larray[umkehrlist[x], 0]);
                }
            }

            return umkehrlist[x];
        }

        private ColorAnimation animateThisTebo(TextBlock tebo, bool c)
        {

            ColorAnimation colorAni = new ColorAnimation
            {
                From = Colors.SkyBlue
            };
            if (tebo.Background == Brushes.LightSeaGreen)
            {
                colorAni.From = Colors.LightSeaGreen;
            }

            if (c)
            {
                colorAni.To = Colors.YellowGreen;
            }
            else
            {
                colorAni.To = Colors.Tomato;
            }

            colorAni.Duration = new Duration(TimeSpan.FromMilliseconds(1000));

            colorAni.BeginTime = TimeSpan.FromMilliseconds(timecounter);
            Storyboard.SetTarget(colorAni, tebo);
            Storyboard.SetTargetProperty(colorAni, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));

            timecounter += 1000;

            return colorAni;

        }

        private DoubleAnimation[] animateThisLineReverse(Line l)
        {

            //resetColors();

            Line l1 = new Line();

            Canvas.SetLeft(l, Canvas.GetLeft(l));

            Canvas.SetTop(l, Canvas.GetTop(l));

            l1.StrokeThickness = 5.0;
            l1.Stroke = Brushes.Tomato;


            l1.X1 = l.X2;
            l1.X2 = l.X2;
            DoubleAnimation mydouble1 = new DoubleAnimation();


            l1.Y1 = l.Y2;
            l1.Y2 = l.Y2;


            mydouble1.From = l.Y2;
            mydouble1.To = l.Y1;
            mydouble1.Duration = new Duration(TimeSpan.FromMilliseconds((1000)));
            mydouble1.BeginTime = TimeSpan.FromMilliseconds(timecounter);


            DoubleAnimation mydouble = new DoubleAnimation
            {
                From = l.X2,
                To = l.X1,
                Duration = new Duration(TimeSpan.FromMilliseconds((1000))),
                BeginTime = TimeSpan.FromMilliseconds(timecounter)
            };
            timecounter += 1000;

            Children.Add(l1);

            DoubleAnimation[] douret = new DoubleAnimation[2];
            douret[1] = mydouble1;
            douret[0] = mydouble;
            Storyboard.SetTarget(douret[0], l1);
            Storyboard.SetTarget(douret[1], l1);
            Storyboard.SetTargetProperty(douret[0], new PropertyPath("X2"));
            Storyboard.SetTargetProperty(douret[1], new PropertyPath("Y2"));

            trashList.Add(l1);

            return douret;



        }

        private DoubleAnimation[] animateThisLine(Line l)
        {

            //resetColors();

            Line l1 = new Line();

            Canvas.SetLeft(l, Canvas.GetLeft(l));

            Canvas.SetTop(l, Canvas.GetTop(l));

            l1.StrokeThickness = 5.0;
            l1.Stroke = Brushes.LawnGreen;


            l1.X1 = l.X1;
            l1.X2 = l.X1;
            DoubleAnimation mydouble1 = new DoubleAnimation();


            l1.Y1 = l.Y1;
            l1.Y2 = l.Y1;


            mydouble1.From = l.Y1;
            mydouble1.To = l.Y2;
            mydouble1.Duration = new Duration(TimeSpan.FromMilliseconds((1000)));
            mydouble1.BeginTime = TimeSpan.FromMilliseconds(timecounter);


            DoubleAnimation mydouble = new DoubleAnimation
            {
                From = l.X1,
                To = l.X2,
                Duration = new Duration(TimeSpan.FromMilliseconds((1000))),
                BeginTime = TimeSpan.FromMilliseconds(timecounter)
            };

            timecounter += 1000;
            Children.Add(l1);

            //l1.BeginAnimation(Line.X2Property, mydouble);
            //l1.BeginAnimation(Line.Y2Property, mydouble1);



            trashList.Add(l1);

            DoubleAnimation[] douret = new DoubleAnimation[2];
            douret[1] = mydouble1;
            douret[0] = mydouble;
            Storyboard.SetTarget(douret[0], l1);
            Storyboard.SetTarget(douret[1], l1);
            Storyboard.SetTargetProperty(douret[0], new PropertyPath("X2"));
            Storyboard.SetTargetProperty(douret[1], new PropertyPath("Y2"));



            return douret;
        }

        #endregion

        #region Reset
        public void resetColors()
        {
            foreach (Line l in trashList)
            {
                Children.Remove(l);
                //l.Opacity = 0.0;
            }
            trashList.Clear();
            linesToAnimat.Clear();
            for (int i = 0; i < tebo.GetLength(0); i++)
            {
                tebo[i].Background = Brushes.SkyBlue;

                if (i % 2 == 0)
                {
                    tebo[i].Background = Brushes.LightSeaGreen;
                }
            }
            foreach (Line l in larray)
            {
                l.Stroke = Brushes.Black;
            }
        }
        #endregion

        #region Constructor

        public Walze(int model, int umkehr, double width, double height)
        {
            this.model = model;
            typ = umkehr;
            Rectangle myRectangle = new Rectangle
            {
                Width = 260,
                Height = 764,

                RadiusX = 15,
                RadiusY = 15,

                Fill = Brushes.LightSteelBlue,
                Stroke = Brushes.Silver,
                StrokeThickness = 30
            };
            Children.Add(myRectangle);

            switch (umkehr)
            {
                case 1:
                    umkehrlist = getWalzeAsInt(model, 0);
                    iAm.Text = "A";
                    ; break;
                case 2: umkehrlist = getWalzeAsInt(model, 1); iAm.Text = "B"; break;
                case 3: umkehrlist = getWalzeAsInt(model, 2); iAm.Text = "C"; break;
            }


            double x = 29.39;
            int ix = 0;
            double distance = 15;
            StackPanel stack = new StackPanel
            {
                Orientation = Orientation.Vertical
            };
            for (int i = 0; i < 26; i++)
            {
                TextBlock t = new TextBlock
                {
                    Text = "" + Convert.ToChar(i + 65),
                    Width = 30.0,
                    Height = 29.39,


                    FontSize = 20,
                    //Canvas.SetLeft(t, 50.0 / 2000 * width + 1);
                    //Canvas.SetTop(t, 42.0 / 1000 * height * i + 60);
                    Background = Brushes.SkyBlue,
                    TextAlignment = TextAlignment.Center
                };
                if (i % 2 == 0)
                {
                    t.Background = Brushes.LightSeaGreen;
                }

                stack.Children.Add(t);
                tebo[i] = t;

                Line l2 = new Line
                {
                    Y1 = x / 2 + i * x,
                    X1 = 230,
                    Y2 = x / 2 + i * x,
                    X2 = 20 + (i - ix) * distance,

                    StrokeThickness = 1,

                    Stroke = Brushes.Black
                };



                Line l3 = new Line
                {
                    Y1 = x / 2 + umkehrlist[i] * x,
                    X1 = 20 + (i - ix) * distance,
                    Y2 = x / 2 + i * x,
                    X2 = 20 + (i - ix) * distance,

                    StrokeThickness = 1,

                    Stroke = Brushes.Black
                };


                Line l4 = new Line
                {
                    Y1 = x / 2 + umkehrlist[i] * x,
                    X1 = 230,
                    Y2 = x / 2 + umkehrlist[i] * x,
                    X2 = 20 + (i - ix) * distance,

                    StrokeThickness = 1,

                    Stroke = Brushes.Black
                };

                if (umkehrlist[i] > i)
                {
                    Children.Add(l4);
                    Children.Add(l2);
                    Children.Add(l3);

                }

                else
                {
                    ix++;
                }

                larray[i, 0] = l2;
                larray[i, 1] = l3;
                larray[i, 2] = l4;
            }
            Canvas.SetLeft(stack, 230);

            Children.Add(stack);
            iAm.Height = 50;
            iAm.Width = 50;
            iAm.FontSize = 30;
            iAm.TextAlignment = TextAlignment.Center;
            Canvas.SetLeft(iAm, 0);
            Canvas.SetTop(iAm, 0);
            iAm.Background = Brushes.Orange;

            iAm.Uid = "" + typ;
            Children.Add(iAm);
        }
        #endregion
    }
}
