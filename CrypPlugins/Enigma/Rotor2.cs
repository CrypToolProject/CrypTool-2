using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;


namespace CrypTool.Enigma
{
    internal class Rotor2 : Canvas
    {
        #region Variables

        private readonly double width;
        private readonly double height;
        public readonly int map;
        private List<TextBlock> tebo = new List<TextBlock>();
        private List<TextBlock> tebo2 = new List<TextBlock>();
        private Line[] lines = new Line[26];
        public TextBlock custom = new TextBlock();
        public TextBlock custom2 = new TextBlock();
        private Canvas lineCanvas = new Canvas();
        private Line lineToAnimat = new Line();
        private Line lineToAnimat2 = new Line();
        private List<Line> lineTrash = new List<Line>();
        private readonly TextBlock[] textBlockToAnimat = new TextBlock[2];
        private readonly TextBlock[] textBlockToAnimat2 = new TextBlock[2];

        private readonly int version = new int();
        private StackPanel stack = new StackPanel();
        private StackPanel stack1 = new StackPanel();
        private StackPanel stack2 = new StackPanel();

        public Button up;
        public Button up1;

        public Button down;
        public Button down1;

        public TextBlock iAm = new TextBlock();

        public bool anomalie = false;

        private Canvas content;

        private readonly double fast = 100;

        public bool stop = false;

        public bool next = false;
        private bool rotated = false;
        private int[] nextint = { -2, -2 };

        public int[,] maparray = new int[26, 2];

        /*int einsnext = 16;
        int zweinext = 4;
        int dreinext = 21;
        int viernext = 9;
        int fuenfnext = 25;
        int[] sechsnext = { 25, 12 };
        int[] siebennext = { 25, 12 };
        int[] achtnext = { 25, 12 };*/

        /* int[] eins = new int[] { 4, 10, 12, 5, 11, 6, 3, 16, 21, 25, 13, 19, 14, 22, 24, 7, 23, 20, 18, 15, 0, 8, 1, 17, 2, 9 };
         int[] zwei = new int[] { 0, 9, 3, 10, 18, 8, 17, 20, 23, 1, 11, 7, 22, 19, 12, 2, 16, 6, 25, 13, 15, 24, 5, 21, 14, 4 };
         int[] drei = new int[] { 1, 3, 5, 7, 9, 11, 2, 15, 17, 19, 23, 21, 25, 13, 24, 4, 8, 22, 6, 0, 10, 12, 20, 18, 16, 14 };
         int[] vier = new int[] { 4, 18, 14, 21, 15, 25, 9, 0, 24, 16, 20, 8, 17, 7, 23, 11, 13, 5, 19, 6, 10, 3, 2, 12, 22, 1 };
         int[] fuenf = new int[] { 21, 25, 1, 17, 6, 8, 19, 24, 20, 15, 18, 3, 13, 7, 11, 23, 0, 22, 12, 9, 16, 14, 5, 4, 2, 10 };
         int[] sechs = new int[] { 9, 15, 6, 21, 14, 20, 12, 5, 24, 16, 1, 4, 13, 7, 25, 17, 3, 10, 0, 18, 23, 11, 8, 2, 19, 22 };
         int[] sieben = new int[] { 13, 25, 9, 7, 6, 17, 2, 23, 12, 24, 18, 22, 1, 14, 20, 5, 0, 8, 21, 11, 15, 4, 10, 16, 3, 19 };
         int[] acht = new int[] { 5, 10, 16, 7, 19, 11, 23, 14, 2, 1, 9, 18, 15, 3, 25, 17, 0, 12, 4, 22, 13, 8, 20, 24, 6, 21 };*/

        private readonly int[] eins = getRotorsAsInt(3, 0);
        private readonly int[] zwei = getRotorsAsInt(3, 1);
        private readonly int[] drei = getRotorsAsInt(3, 2);
        private readonly int[] vier = getRotorsAsInt(3, 3);
        private readonly int[] fuenf = getRotorsAsInt(3, 4);
        private readonly int[] sechs = getRotorsAsInt(3, 5);
        private readonly int[] sieben = getRotorsAsInt(3, 6);
        private readonly int[] acht = getRotorsAsInt(3, 7);
        private readonly int einsnext = getNotchesAsInt(3, 0)[0];
        private readonly int zweinext = getNotchesAsInt(3, 1)[0];
        private readonly int dreinext = getNotchesAsInt(3, 2)[0];
        private readonly int viernext = getNotchesAsInt(3, 3)[0];
        private readonly int fuenfnext = getNotchesAsInt(3, 4)[0];
        private readonly int[] sechsnext = getNotchesAsInt(3, 5);
        private readonly int[] siebennext = getNotchesAsInt(3, 6);
        private readonly int[] achtnext = getNotchesAsInt(3, 7);
        private double timecounter = 0.0;

        #endregion

        #region getter

        private static int[] getRotorsAsInt(int model, int rotor)
        {
            int[] value = new int[EnigmaCore.rotors[model, rotor].Length];

            for (int i = 0; i < EnigmaCore.rotors[model, rotor].Length; i++)
            {
                value[i] = EnigmaCore.rotors[model, rotor][i] - 65;
            }
            return value;
        }

        private static int[] getNotchesAsInt(int model, int rotor)
        {
            int[] value = new int[EnigmaCore.notches[model, rotor].Length];
            for (int i = 0; i < EnigmaCore.notches[model, rotor].Length; i++)
            {
                value[i] = EnigmaCore.notches[model, rotor][i] - 65;
            }
            return value;
        }

        #endregion

        #region mapping
        public int mapto(int x)
        {
            lineToAnimat = lines[maparray[x, 0]];
            textBlockToAnimat[0] = tebo[maparray[x, 0]];
            textBlockToAnimat[1] = tebo2[maparray[x, 1]];
            rotated = false;


            return maparray[x, 1];
        }
        public int returnMap()
        {
            return map;
        }
        public int maptoreverse(int y)
        {
            int help = 0;
            for (int x = 0; x < maparray.GetLength(0); x++)
            {
                if (maparray[x, 1] == y)
                {
                    help = x;
                }
            }
            textBlockToAnimat2[1] = tebo2[maparray[y, 0]];
            textBlockToAnimat2[0] = tebo[help];

            lineToAnimat2 = lines[help];
            return help;
        }
        #endregion

        #region storyboard creating

        public Storyboard startAnimation()
        {
            timecounter = 0.0;
            Storyboard sbreturn = new Storyboard();

            ColorAnimation col1 = animateThisTebo(textBlockToAnimat[0], true);
            sbreturn.Children.Add(col1);

            DoubleAnimation[] dou = animateThisLine(lineToAnimat);
            sbreturn.Children.Add(dou[0]);
            sbreturn.Children.Add(dou[1]);

            ColorAnimation col2 = animateThisTebo(textBlockToAnimat[1], true);
            sbreturn.Children.Add(col2);




            return sbreturn;
        }

        public Storyboard startAnimationReverse()
        {
            timecounter = 0.0;
            Storyboard sbreturn = new Storyboard();

            ColorAnimation col1 = animateThisTebo(textBlockToAnimat2[1], false);
            sbreturn.Children.Add(col1);

            DoubleAnimation[] dou = animateThisLine2(lineToAnimat2);
            sbreturn.Children.Add(dou[0]);
            sbreturn.Children.Add(dou[1]);


            ColorAnimation col2 = animateThisTebo(textBlockToAnimat2[0], false);
            sbreturn.Children.Add(col2);


            return sbreturn;
        }

        private ColorAnimation animateThisTebo(TextBlock tebo, bool b)
        {


            ColorAnimation colorAni = new ColorAnimation();
            string s = tebo.Text;
            colorAni.From = Colors.Gainsboro;
            if (tebo.Background == Brushes.Silver)
            {
                colorAni.From = Colors.Silver;
            }

            if (b)
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

        private DoubleAnimation[] animateThisLine2(Line l)
        {



            Line l1 = new Line();

            Canvas.SetLeft(l, Canvas.GetLeft(l));

            Canvas.SetTop(l, Canvas.GetTop(l));

            l1.StrokeThickness = 5.0;
            l1.Stroke = Brushes.Tomato;


            l1.X1 = l.X2;
            l1.X2 = l.X2;
            DoubleAnimation mydouble1 = new DoubleAnimation();
            if (rotated)
            {
                l1.Y1 = l.Y2 - 30;
                l1.Y2 = l.Y2 - 30;


                mydouble1.From = l.Y2 - 30;
                mydouble1.To = l.Y1 - 30;
                double abst = Math.Sqrt(Math.Pow(l.X2 - l.X1, 2) + Math.Pow(l.Y2 - l.Y1, 2));
                if (abst == 0)
                {
                    abst = 1;
                }

                mydouble1.Duration = new Duration(TimeSpan.FromMilliseconds((abst)));

            }
            else
            {
                l1.Y1 = l.Y2;
                l1.Y2 = l.Y2;


                mydouble1.From = l.Y2;
                mydouble1.To = l.Y1;
                mydouble1.Duration = new Duration(TimeSpan.FromMilliseconds((1000)));

            }

            DoubleAnimation mydouble = new DoubleAnimation
            {
                From = l.X2,
                To = l.X1,
                Duration = new Duration(TimeSpan.FromMilliseconds((1000))),
                BeginTime = TimeSpan.FromMilliseconds(timecounter)
            };
            mydouble1.BeginTime = TimeSpan.FromMilliseconds(timecounter);


            lineCanvas.Children.Add(l1);


            lineTrash.Add(l1);

            DoubleAnimation[] douret = new DoubleAnimation[2];

            douret[1] = mydouble1;
            douret[0] = mydouble;

            Storyboard.SetTarget(douret[0], l1);
            Storyboard.SetTarget(douret[1], l1);
            Storyboard.SetTargetProperty(douret[0], new PropertyPath("X2"));
            Storyboard.SetTargetProperty(douret[1], new PropertyPath("Y2"));

            timecounter += 1000;

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
            DoubleAnimation mydouble1 = new DoubleAnimation
            {
                BeginTime = TimeSpan.FromMilliseconds(timecounter)
            };
            if (rotated)
            {
                l1.Y1 = l.Y1 - 30;
                l1.Y2 = l.Y1 - 30;


                mydouble1.From = l.Y1 - 30;
                mydouble1.To = l.Y2 - 30;

                double abst = Math.Sqrt(Math.Pow(l.X2 - l.X1, 2) + Math.Pow(l.Y2 - l.Y1, 2));
                if (abst == 0)
                {
                    abst = 1;
                }

                mydouble1.Duration = new Duration(TimeSpan.FromMilliseconds((abst)));

            }
            else
            {
                l1.Y1 = l.Y1;
                l1.Y2 = l.Y1;


                mydouble1.From = l.Y1;
                mydouble1.To = l.Y2;
                mydouble1.Duration = new Duration(TimeSpan.FromMilliseconds((1000)));

            }

            DoubleAnimation mydouble = new DoubleAnimation
            {
                From = l.X1,
                To = l.X2,
                Duration = new Duration(TimeSpan.FromMilliseconds((1000))),
                BeginTime = TimeSpan.FromMilliseconds(timecounter)
            };
            //mydouble.Completed += helpNextAnimationMethod;


            lineCanvas.Children.Add(l1);

            DoubleAnimation[] douret = new DoubleAnimation[2];

            douret[1] = mydouble1;
            douret[0] = mydouble;

            Storyboard.SetTarget(douret[0], l1);
            Storyboard.SetTarget(douret[1], l1);
            Storyboard.SetTargetProperty(douret[0], new PropertyPath("X2"));
            Storyboard.SetTargetProperty(douret[1], new PropertyPath("Y2"));

            lineTrash.Add(l1);

            timecounter += 1000;
            return douret;

        }

        #endregion

        #region events

        public event EventHandler updone;
        public event EventHandler downdone;
        public event EventHandler up1done;
        public event EventHandler down1done;

        #endregion

        #region rotation
        private void helpdownererclick(object sender, EventArgs e)
        {

            DoubleAnimation mydouble = new DoubleAnimation
            {
                From = 30.0,
                To = 60.0,
                Duration = new Duration(TimeSpan.FromMilliseconds((0)))
            };
            //mydouble.RepeatBehavior = RepeatBehavior.Forever;

            DoubleAnimation mydouble1 = new DoubleAnimation
            {
                From = -30.0,
                To = 0.0,
                Duration = new Duration(TimeSpan.FromMilliseconds((0)))
            };
            //mydouble1.RepeatBehavior = RepeatBehavior.Forever;
            DoubleAnimation mydouble3 = new DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                Duration = new Duration(TimeSpan.FromMilliseconds((0)))
            };

            DoubleAnimation mydouble4 = new DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                Duration = new Duration(TimeSpan.FromMilliseconds((fast)))
            };

            custom.BeginAnimation(OpacityProperty, mydouble4);

            tebo[25].BeginAnimation(OpacityProperty, mydouble3);
            tebo2[25].BeginAnimation(OpacityProperty, mydouble3);

            stack.Children.RemoveAt(0);
            stack1.Children.RemoveAt(0);


            stack.BeginAnimation(Canvas.TopProperty, mydouble);
            stack1.BeginAnimation(Canvas.TopProperty, mydouble);
            lineCanvas.BeginAnimation(Canvas.TopProperty, mydouble1);


            char c = custom.Text[0];
            int ix = c - 65;
            if (ix > 0)
            {
                ix = ix - 1;
            }
            else
            {
                ix = 25;
            }

            custom.Text = Convert.ToChar(ix + 65) + "";

            TextBlock dummy = tebo[25];
            tebo.RemoveAt(25);
            tebo.Insert(0, dummy);

            TextBlock dummy2 = tebo2[25];
            tebo2.RemoveAt(25);
            tebo2.Insert(0, dummy2);

            stack.Children.RemoveAt(25);
            stack.Children.Insert(0, dummy);


            stack1.Children.RemoveAt(25);
            stack1.Children.Insert(0, dummy2);

            derotate();
            downdone(down, EventArgs.Empty);
            b = true;
        }

        private void downerclick(object sender, EventArgs e)
        {
            if (b)
            {
                resetColors();
                b = false;
                DoubleAnimation mydouble = new DoubleAnimation
                {
                    From = 31.0,
                    To = 60.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds((fast)))
                };
                //mydouble.RepeatBehavior = RepeatBehavior.Forever;

                DoubleAnimation mydouble1 = new DoubleAnimation
                {
                    From = 0.0,
                    To = 30.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds((fast)))
                };
                //mydouble1.RepeatBehavior = RepeatBehavior.Forever;

                DoubleAnimation mydouble2 = new DoubleAnimation
                {
                    From = 0.0,
                    To = 1.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds((fast)))
                };



                DoubleAnimation mydouble3 = new DoubleAnimation
                {
                    From = 1.0,
                    To = 0.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds((fast)))
                };

                DoubleAnimation mydouble4 = new DoubleAnimation
                {
                    From = 850.0,
                    To = 50.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds((fast)))
                };

                mydouble1.Completed += helpdownererclick;
                TextBlock dummy = cloneTextBlock(tebo[25]);

                TextBlock dummy2 = cloneTextBlock(tebo2[25]);


                Line l = new Line();
                l = cloneLine(lines[0]);

                //stack.Children.Add(dummy);
                stack.Children.Insert(0, dummy);
                //stack1.Children.Add(dummy2);
                stack1.Children.Insert(0, dummy2);
                tebo[25].BeginAnimation(OpacityProperty, mydouble3);
                tebo2[25].BeginAnimation(OpacityProperty, mydouble3);
                custom.BeginAnimation(OpacityProperty, mydouble3);

                dummy.BeginAnimation(OpacityProperty, mydouble2);
                dummy2.BeginAnimation(OpacityProperty, mydouble2);

                lines[25].BeginAnimation(Line.Y1Property, mydouble4);

                for (int i = 0; i < maparray.GetLength(0); i++)
                {
                    if (maparray[i, 1] == 25)
                    {
                        lines[i].BeginAnimation(Line.Y2Property, mydouble4);
                    }
                }

                stack.BeginAnimation(Canvas.TopProperty, mydouble);

                stack1.BeginAnimation(Canvas.TopProperty, mydouble);

                lineCanvas.BeginAnimation(Canvas.TopProperty, mydouble1);
            }
        }


        private void helpdownererclick1(object sender, EventArgs e)
        {

            DoubleAnimation mydouble = new DoubleAnimation
            {
                From = 30.0,
                To = 60.0,
                Duration = new Duration(TimeSpan.FromMilliseconds((0)))
            };
            //mydouble.RepeatBehavior = RepeatBehavior.Forever;

            DoubleAnimation mydouble1 = new DoubleAnimation
            {
                From = -30.0,
                To = 0.0,
                Duration = new Duration(TimeSpan.FromMilliseconds((0)))
            };
            //mydouble1.RepeatBehavior = RepeatBehavior.Forever;
            DoubleAnimation mydouble3 = new DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                Duration = new Duration(TimeSpan.FromMilliseconds((0)))
            };

            DoubleAnimation mydouble4 = new DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                Duration = new Duration(TimeSpan.FromMilliseconds((fast)))
            };

            custom2.BeginAnimation(OpacityProperty, mydouble4);

            tebo[25].BeginAnimation(OpacityProperty, mydouble3);
            //tebo2[25].BeginAnimation(OpacityProperty, mydouble3);

            stack.Children.RemoveAt(0);
            //stack1.Children.RemoveAt(0);


            stack.BeginAnimation(Canvas.TopProperty, mydouble);
            //stack1.BeginAnimation(Canvas.TopProperty, mydouble);
            lineCanvas.BeginAnimation(Canvas.TopProperty, mydouble1);

            TextBlock dummy = tebo[25];
            tebo.RemoveAt(25);
            tebo.Insert(0, dummy);

            stack.Children.RemoveAt(25);
            stack.Children.Insert(0, dummy);


            int x = int.Parse(custom2.Text);
            if (x != 26)
            {
                custom2.Text = "" + (x + 1);
            }
            else
            {
                custom2.Text = 1 + "";
            }

            derotate();
            for (int i = 0; i < tebo.Count; i++)
            {
                if (tebo[i].FontWeight == FontWeights.UltraBold)
                {
                    if (i == 0)
                    {
                        tebo[25].Foreground = Brushes.Red;
                        tebo[25].FontWeight = FontWeights.UltraBold;
                        tebo[0].FontWeight = FontWeights.Normal;
                        tebo[0].Foreground = Brushes.Black;
                        break;
                    }
                    else if (i <= 25 && i > 0)
                    {
                        tebo[i - 1].Foreground = Brushes.Red;
                        tebo[i - 1].FontWeight = FontWeights.UltraBold;
                        tebo[i].FontWeight = FontWeights.Normal;
                        tebo[i].Foreground = Brushes.Black;
                    }

                }
            }
            down1done(down1, EventArgs.Empty);
            b = true;
        }

        public void downerclick1(object sender, EventArgs e)
        {
            if (b)
            {
                resetColors();
                b = false;
                DoubleAnimation mydouble = new DoubleAnimation
                {
                    From = 31.0,
                    To = 60.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds((fast)))
                };
                //mydouble.RepeatBehavior = RepeatBehavior.Forever;

                DoubleAnimation mydouble1 = new DoubleAnimation
                {
                    From = 0.0,
                    To = 30.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds((fast)))
                };
                //mydouble1.RepeatBehavior = RepeatBehavior.Forever;

                DoubleAnimation mydouble2 = new DoubleAnimation
                {
                    From = 0.0,
                    To = 1.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds((fast)))
                };



                DoubleAnimation mydouble3 = new DoubleAnimation
                {
                    From = 1.0,
                    To = 0.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds((fast)))
                };

                DoubleAnimation mydouble4 = new DoubleAnimation
                {
                    From = 850.0,
                    To = 50.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds((fast)))
                };

                mydouble1.Completed += helpdownererclick1;
                TextBlock dummy = cloneTextBlock(tebo[25]);

                TextBlock dummy2 = cloneTextBlock(tebo2[25]);



                Line l = new Line();
                l = cloneLine(lines[0]);

                //stack.Children.Add(dummy);
                stack.Children.Insert(0, dummy);
                //stack1.Children.Add(dummy2);
                //stack1.Children.Insert(0, dummy2);
                tebo[25].BeginAnimation(OpacityProperty, mydouble3);
                //tebo2[25].BeginAnimation(OpacityProperty, mydouble3);
                custom2.BeginAnimation(OpacityProperty, mydouble3);

                dummy.BeginAnimation(OpacityProperty, mydouble2);
                //dummy2.BeginAnimation(OpacityProperty, mydouble2);

                lines[25].BeginAnimation(Line.Y1Property, mydouble4);

                for (int i = 0; i < maparray.GetLength(0); i++)
                {
                    if (maparray[i, 1] == 25)
                    {
                        lines[i].BeginAnimation(Line.Y2Property, mydouble4);
                    }
                }

                stack.BeginAnimation(Canvas.TopProperty, mydouble);

                //stack1.BeginAnimation(Canvas.TopProperty, mydouble);

                lineCanvas.BeginAnimation(Canvas.TopProperty, mydouble1);
            }

        }


        public void derotate()
        {



            next = false;
            if (tebo2[0].Text[0] == Convert.ToChar(nextint[0] + 65) || tebo2[0].Text[0] == Convert.ToChar(nextint[1] + 65))
            {
                next = true;
            }
            //int dummy = maparray[0,0];
            int dummy1 = maparray[25, 1];

            for (int i = 25; i > 0; i--)
            {
                //  maparray[i,0] = maparray[i + 1, 0];
                if (maparray[i - 1, 1] == 25)
                {
                    maparray[i, 1] = 0;
                }
                else
                {
                    maparray[i, 1] = maparray[i - 1, 1] + 1;
                }
            }

            //maparray[maparray.GetLength(0) - 1, 0] = dummy;
            if (dummy1 == 25)
            {
                maparray[0, 1] = 0;
            }
            else
            {
                maparray[0, 1] = dummy1 + 1;
            }

            foreach (Line l in lines)
            {
                lineCanvas.Children.Remove(l);
            }
            for (int i = 0; i < 26; i++)
            {
                Line t1 = new Line
                {
                    X1 = 170,
                    Y1 = 29.4 * i + 75,
                    Opacity = 0.5,
                    X2 = 30,
                    Y2 = 0,

                    Stroke = Brushes.Black
                };

                t1.Y2 = 29.4 * maparray[i, 1] + 75;

                lines[i] = t1;
                lineCanvas.Children.Add(t1);
            }
        }

        public void helpupperclick(object sender, EventArgs e)
        {
            helpupperclicksb(true);
        }
        public Storyboard helpupperclicksb(bool go)
        {
            Storyboard sb = new Storyboard();

            DoubleAnimation mydouble = new DoubleAnimation
            {
                From = 30.0,
                To = 60.0,
                Duration = new Duration(TimeSpan.FromMilliseconds((0)))
            };
            //mydouble.RepeatBehavior = RepeatBehavior.Forever;

            DoubleAnimation mydouble1 = new DoubleAnimation
            {
                From = -30.0,
                To = 0.0,
                Duration = new Duration(TimeSpan.FromMilliseconds((0)))
            };
            //mydouble1.RepeatBehavior = RepeatBehavior.Forever;

            DoubleAnimation mydouble3 = new DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                Duration = new Duration(TimeSpan.FromMilliseconds((0)))
            };


            DoubleAnimation mydouble4 = new DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                Duration = new Duration(TimeSpan.FromMilliseconds((fast)))
            };



            //stack.Children.RemoveAt(26);
            //stack1.Children.RemoveAt(26);

            if (go)
            {
                tebo[0].BeginAnimation(OpacityProperty, mydouble3);
                tebo2[0].BeginAnimation(OpacityProperty, mydouble3);
                custom.BeginAnimation(OpacityProperty, mydouble4);
                stack.BeginAnimation(Canvas.TopProperty, mydouble);
                stack1.BeginAnimation(Canvas.TopProperty, mydouble);
                lineCanvas.BeginAnimation(Canvas.TopProperty, mydouble1);
            }
            else
            {


                Storyboard.SetTarget(mydouble3, tebo[0]);
                DoubleAnimation mydouble31 = new DoubleAnimation
                {
                    From = 0.0,
                    To = 1.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds((0)))
                };
                Storyboard.SetTargetProperty(mydouble31, new PropertyPath("(Opacity)"));
                Storyboard.SetTarget(mydouble31, tebo2[0]);

                Storyboard.SetTarget(mydouble4, custom);
                Storyboard.SetTarget(mydouble, stack);

                DoubleAnimation mydouble01 = new DoubleAnimation
                {
                    From = 30.0,
                    To = 60.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds((0)))
                };
                Storyboard.SetTargetProperty(mydouble01, new PropertyPath("(Canvas.Top)"));
                Storyboard.SetTarget(mydouble01, stack1);
                Storyboard.SetTarget(mydouble1, lineCanvas);
                Storyboard.SetTargetProperty(mydouble, new PropertyPath("(Canvas.Top)"));
                Storyboard.SetTargetProperty(mydouble1, new PropertyPath("(Canvas.Top)"));
                Storyboard.SetTargetProperty(mydouble3, new PropertyPath("(Opacity)"));
                Storyboard.SetTargetProperty(mydouble4, new PropertyPath("(Opacity)"));


                sb.SpeedRatio = 0.1;

                sb.Children.Add(mydouble);
                sb.Children.Add(mydouble1);
                sb.Children.Add(mydouble3);
                sb.Children.Add(mydouble4);
                sb.Children.Add(mydouble01);
                sb.Children.Add(mydouble31);
            }

            char c = custom.Text[0];
            int ix = c - 65;
            ix = (ix + 1) % 26;

            custom.Text = Convert.ToChar(ix + 65) + "";

            TextBlock dummy = cloneTextBlock(tebo[0]);
            tebo.RemoveAt(0);
            tebo.Add((TextBlock)stack.Children[26]);
            //tebo.Insert(25, dummy);

            TextBlock dummy2 = cloneTextBlock(tebo2[0]);
            tebo2.RemoveAt(0);
            tebo2.Add((TextBlock)stack1.Children[26]);
            //tebo2.Insert(25,dummy2);

            stack.Children.RemoveAt(0);
            //stack.Children.Add(dummy);

            stack1.Children.RemoveAt(0);
            //stack1.Children.Add(dummy2);



            rotate();
            EventArgs test = new EventArgs();

            updone(up, EventArgs.Empty);
            b = true;
            return sb;
        }

        private bool b = true;
        public void upperclick(object sender, EventArgs e)
        {
            upperclicksb(true);
        }


        public Storyboard upperclicksb(bool go)
        {
            //int dummy = maparray[0,0];
            Storyboard sb = new Storyboard();

            if (b)
            {
                int dummy1 = maparray[0, 1];


                for (int i = 0; i < maparray.GetLength(0) - 1; i++)
                {
                    //  maparray[i,0] = maparray[i + 1, 0];
                    if (maparray[i + 1, 1] == 0)
                    {
                        maparray[i, 1] = 25;
                    }
                    else
                    {
                        maparray[i, 1] = maparray[i + 1, 1] - 1;
                    }
                }

                //maparray[maparray.GetLength(0) - 1, 0] = dummy;
                if (dummy1 == 0)
                {
                    maparray[maparray.GetLength(0) - 1, 1] = 25;
                }
                else
                {
                    maparray[maparray.GetLength(0) - 1, 1] = dummy1 - 1;
                }

                resetColors();
                b = false;
                DoubleAnimation mydouble = new DoubleAnimation
                {
                    From = 60.0,
                    To = 31.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds((fast)))
                };
                //mydouble.RepeatBehavior = RepeatBehavior.Forever;

                DoubleAnimation mydouble1 = new DoubleAnimation
                {
                    From = 0.0,
                    To = -30.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds((fast)))
                };
                //mydouble1.RepeatBehavior = RepeatBehavior.Forever;

                DoubleAnimation mydouble2 = new DoubleAnimation
                {
                    From = 0.0,
                    To = 1.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds((fast)))
                };

                DoubleAnimation mydouble3 = new DoubleAnimation
                {
                    From = 1.0,
                    To = 0.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds((fast)))
                };

                DoubleAnimation mydouble4 = new DoubleAnimation
                {
                    From = 75,
                    To = 850.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds((fast)))
                };
                TextBlock dummy = cloneTextBlock(tebo[0]);

                TextBlock dummy2 = cloneTextBlock(tebo2[0]);

                dummy.Opacity = 0.0;
                dummy2.Opacity = 0.0;


                Line l = new Line();
                l = cloneLine(lines[0]);

                stack.Children.Add(dummy);
                stack1.Children.Add(dummy2);
                if (go)
                {
                    mydouble1.Completed += helpupperclick;

                    tebo[0].BeginAnimation(OpacityProperty, mydouble3);
                    tebo2[0].BeginAnimation(OpacityProperty, mydouble3);
                    custom.BeginAnimation(OpacityProperty, mydouble3);

                    dummy.BeginAnimation(OpacityProperty, mydouble2);
                    dummy2.BeginAnimation(OpacityProperty, mydouble2);

                    lines[0].BeginAnimation(Line.Y1Property, mydouble4);

                    for (int i = 0; i < maparray.GetLength(0); i++)
                    {
                        if (maparray[i, 1] == 25)
                        {
                            if (i + 1 != 26)
                            {
                                lines[i + 1].BeginAnimation(Line.Y2Property, mydouble4);
                            }
                            else
                            {
                                lines[0].BeginAnimation(Line.Y2Property, mydouble4);
                            }
                        }
                    }

                    stack.BeginAnimation(Canvas.TopProperty, mydouble);

                    stack1.BeginAnimation(Canvas.TopProperty, mydouble);

                    lineCanvas.BeginAnimation(Canvas.TopProperty, mydouble1);
                }
                else
                {

                    sb.SpeedRatio = 0.1;
                    Storyboard.SetTarget(mydouble3, tebo[0]);
                    Storyboard.SetTargetProperty(mydouble3, new PropertyPath("(Opacity)"));
                    sb.Children.Add(mydouble3);

                    DoubleAnimation mydouble31 = new DoubleAnimation
                    {
                        From = 1.0,
                        To = 0.0,
                        Duration = new Duration(TimeSpan.FromMilliseconds((fast)))
                    };
                    Storyboard.SetTargetProperty(mydouble31, new PropertyPath("(Opacity)"));
                    Storyboard.SetTarget(mydouble31, tebo2[0]);
                    sb.Children.Add(mydouble31);


                    DoubleAnimation mydouble32 = new DoubleAnimation
                    {
                        From = 1.0,
                        To = 0.0,
                        Duration = new Duration(TimeSpan.FromMilliseconds((fast)))
                    };
                    Storyboard.SetTargetProperty(mydouble32, new PropertyPath("(Opacity)"));
                    Storyboard.SetTarget(mydouble32, custom);
                    sb.Children.Add(mydouble32);

                    Storyboard.SetTarget(mydouble2, dummy);
                    Storyboard.SetTargetProperty(mydouble2, new PropertyPath("(Opacity)"));
                    sb.Children.Add(mydouble2);

                    DoubleAnimation mydouble21 = new DoubleAnimation
                    {
                        From = 0.0,
                        To = 1.0,
                        Duration = new Duration(TimeSpan.FromMilliseconds((fast)))
                    };
                    Storyboard.SetTargetProperty(mydouble21, new PropertyPath("(Opacity)"));
                    Storyboard.SetTarget(mydouble21, dummy2);
                    sb.Children.Add(mydouble21);



                    Storyboard.SetTarget(mydouble4, lines[0]);
                    sb.Children.Add(mydouble4);

                    Storyboard.SetTarget(mydouble, stack);
                    Storyboard.SetTargetProperty(mydouble, new PropertyPath("(Canvas.Top)"));
                    sb.Children.Add(mydouble);

                    DoubleAnimation mydouble01 = new DoubleAnimation
                    {
                        From = 60.0,
                        To = 31.0,
                        Duration = new Duration(TimeSpan.FromMilliseconds((fast)))
                    };
                    Storyboard.SetTarget(mydouble01, stack1);

                    Storyboard.SetTargetProperty(mydouble01, new PropertyPath("(Canvas.Top)"));
                    sb.Children.Add(mydouble01);

                    Storyboard.SetTarget(mydouble1, lineCanvas);
                    Storyboard.SetTargetProperty(mydouble1, new PropertyPath("(Canvas.Top)"));
                    sb.Children.Add(mydouble1);

                    DoubleAnimation mydouble41 = new DoubleAnimation
                    {
                        From = 75,
                        To = 850.0,
                        Duration = new Duration(TimeSpan.FromMilliseconds((fast)))
                    };


                    for (int i = 0; i < maparray.GetLength(0); i++)
                    {
                        if (maparray[i, 1] == 25)
                        {
                            if (i + 1 != 26)
                            {
                                Storyboard.SetTarget(mydouble41, lines[i + 1]);
                                Storyboard.SetTargetProperty(mydouble41, new PropertyPath("(Y2)"));
                            }
                            else
                            {
                                Storyboard.SetTarget(mydouble41, lines[0]);
                                Storyboard.SetTargetProperty(mydouble41, new PropertyPath("(Y2)"));
                            }
                        }
                    }

                    sb.Children.Add(mydouble41);

                    Storyboard.SetTargetProperty(mydouble4, new PropertyPath("(Y1)"));


                    //mydouble1.Completed += helpupperclick;
                    b = true;
                }

            }
            return sb;
        }


        private void helpupperclick1(object sender, EventArgs e)
        {
            DoubleAnimation mydouble = new DoubleAnimation
            {
                From = 30.0,
                To = 60.0,
                Duration = new Duration(TimeSpan.FromMilliseconds((0)))
            };
            //mydouble.RepeatBehavior = RepeatBehavior.Forever;

            DoubleAnimation mydouble1 = new DoubleAnimation
            {
                From = -30.0,
                To = 0.0,
                Duration = new Duration(TimeSpan.FromMilliseconds((0)))
            };
            //mydouble1.RepeatBehavior = RepeatBehavior.Forever;

            DoubleAnimation mydouble3 = new DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                Duration = new Duration(TimeSpan.FromMilliseconds((0)))
            };
            tebo[0].BeginAnimation(OpacityProperty, mydouble3);
            tebo2[0].BeginAnimation(OpacityProperty, mydouble3);

            DoubleAnimation mydouble4 = new DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                Duration = new Duration(TimeSpan.FromMilliseconds((fast)))
            };

            custom2.BeginAnimation(OpacityProperty, mydouble4);

            stack.Children.RemoveAt(26);
            //stack1.Children.RemoveAt(26);


            stack.BeginAnimation(Canvas.TopProperty, mydouble);
            stack1.BeginAnimation(Canvas.TopProperty, mydouble);
            lineCanvas.BeginAnimation(Canvas.TopProperty, mydouble1);

            TextBlock dummy = tebo[0];
            tebo.RemoveAt(0);
            tebo.Add(dummy);

            stack.Children.RemoveAt(0);
            stack.Children.Add(dummy);


            int x = int.Parse(custom2.Text);
            if (x != 1)
            {
                custom2.Text = "" + (x - 1);
            }
            else
            {
                custom2.Text = 26 + "";
            }


            //int dummy = maparray[0,0];
            int dummy1 = maparray[0, 1];


            for (int i = 0; i < maparray.GetLength(0) - 1; i++)
            {
                //  maparray[i,0] = maparray[i + 1, 0];
                if (maparray[i + 1, 1] == 0)
                {
                    maparray[i, 1] = 25;
                }
                else
                {
                    maparray[i, 1] = maparray[i + 1, 1] - 1;
                }
            }

            //maparray[maparray.GetLength(0) - 1, 0] = dummy;
            if (dummy1 == 0)
            {
                maparray[maparray.GetLength(0) - 1, 1] = 25;
            }
            else
            {
                maparray[maparray.GetLength(0) - 1, 1] = dummy1 - 1;
            }

            rotate();

            for (int i = tebo.Count - 1; i > -1; i--)
            {
                if (tebo[i].FontWeight == FontWeights.UltraBold)
                {
                    if (i == 0)
                    {
                        tebo[0].Foreground = Brushes.Red;
                        tebo[0].FontWeight = FontWeights.UltraBold;
                        tebo[1].FontWeight = FontWeights.Normal;
                        tebo[1].Foreground = Brushes.Black;
                    }
                    else if (i != 25)
                    {
                        tebo[i + 1].Foreground = Brushes.Red;
                        tebo[i + 1].FontWeight = FontWeights.UltraBold;
                        tebo[i].FontWeight = FontWeights.Normal;
                        tebo[i].Foreground = Brushes.Black;
                    }

                    else
                    {
                        tebo[0].Foreground = Brushes.Red;
                        tebo[0].FontWeight = FontWeights.UltraBold;
                        tebo[25].FontWeight = FontWeights.Normal;
                        tebo[25].Foreground = Brushes.Black;

                    }
                }
            }
            up1done(up1, EventArgs.Empty);
            b = true;
        }

        public void upperclick1(object sender, EventArgs e)
        {

            if (b)
            {

                resetColors();
                b = false;
                DoubleAnimation mydouble = new DoubleAnimation
                {
                    From = 60.0,
                    To = 31.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds((fast)))
                };
                //mydouble.RepeatBehavior = RepeatBehavior.Forever;

                DoubleAnimation mydouble1 = new DoubleAnimation
                {
                    From = 0.0,
                    To = -30.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds((fast)))
                };
                //mydouble1.RepeatBehavior = RepeatBehavior.Forever;

                DoubleAnimation mydouble2 = new DoubleAnimation
                {
                    From = 0.0,
                    To = 1.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds((fast)))
                };

                DoubleAnimation mydouble3 = new DoubleAnimation
                {
                    From = 1.0,
                    To = 0.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds((fast)))
                };

                DoubleAnimation mydouble4 = new DoubleAnimation
                {
                    From = 100.0,
                    To = 850.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds((fast)))
                };

                mydouble1.Completed += helpupperclick1;
                TextBlock dummy = cloneTextBlock(tebo[0]);

                TextBlock dummy2 = cloneTextBlock(tebo2[0]);




                Line l = new Line();
                l = cloneLine(lines[0]);

                stack.Children.Add(dummy);
                //stack1.Children.Add(dummy2);
                tebo[0].BeginAnimation(OpacityProperty, mydouble3);
                //tebo2[0].BeginAnimation(OpacityProperty, mydouble3);
                custom2.BeginAnimation(OpacityProperty, mydouble3);

                dummy.BeginAnimation(OpacityProperty, mydouble2);
                //dummy2.BeginAnimation(OpacityProperty, mydouble2);

                lines[0].BeginAnimation(Line.Y1Property, mydouble4);

                for (int i = 0; i < maparray.GetLength(0); i++)
                {
                    if (maparray[i, 1] == 0)
                    {
                        lines[i].BeginAnimation(Line.Y2Property, mydouble4);
                    }
                }

                stack.BeginAnimation(Canvas.TopProperty, mydouble);

                //stack1.BeginAnimation(Canvas.TopProperty, mydouble);

                lineCanvas.BeginAnimation(Canvas.TopProperty, mydouble1);

            }

        }

        public void rotate()
        {

            next = false;
            rotated = true;
            if (tebo2[0].Text[0] == Convert.ToChar(nextint[0] + 65) || tebo2[0].Text[0] == Convert.ToChar(nextint[1] + 65))
            {
                next = true;
            }

            foreach (Line l in lines)
            {
                lineCanvas.Children.Remove(l);
            }

            for (int i = 0; i < 26; i++)
            {
                Line t1 = new Line
                {
                    X1 = 170,
                    Y1 = 29.4 * i + 75,
                    Opacity = 0.5,
                    X2 = 30,
                    Y2 = 0,

                    Stroke = Brushes.Black
                };

                t1.Y2 = 29.4 * maparray[i, 1] + 75;

                lines[i] = t1;
                lineCanvas.Children.Add(t1);
            }

        }

        #endregion

        #region cloning
        private Line cloneLine(Line l)
        {
            Line l1 = new Line
            {
                Stroke = l.Stroke,
                X1 = l.X1,
                X2 = l.X2,
                Y1 = l.Y1,
                Y2 = l.Y2
            };
            return l1;
        }

        private TextBlock cloneTextBlock(TextBlock t)
        {
            TextBlock dummy = new TextBlock
            {
                Text = t.Text,
                Background = t.Background,
                Height = t.Height,
                Width = t.Width,
                FontSize = t.FontSize,
                TextAlignment = t.TextAlignment,
                Foreground = t.Foreground,
                FontWeight = t.FontWeight
            };
            return dummy;
        }
        #endregion

        #region reset
        public void resetColors()
        {
            foreach (Line l in lineTrash)
            {
                lineCanvas.Children.Remove(l);
            }

            lineTrash.Clear();


            if (int.Parse(tebo[0].Text) % 2 == 0)
            {
                for (int i = 0; i < tebo.Count; i++)
                {
                    tebo[i].Background = Brushes.Silver;

                    if (i % 2 == 0)
                    {
                        tebo[i].Background = Brushes.Gainsboro;
                    }
                }
            }
            else
            {
                for (int i = 0; i < tebo.Count; i++)
                {
                    tebo[i].Background = Brushes.Gainsboro;

                    if (i % 2 == 0)
                    {
                        tebo[i].Background = Brushes.Silver;
                    }
                }
            }

            if (tebo2[0].Text[0] % 2 == 0)
            {
                for (int i = 0; i < tebo2.Count; i++)
                {
                    tebo2[i].Background = Brushes.Silver;

                    if (i % 2 == 0)
                    {
                        tebo2[i].Background = Brushes.Gainsboro;
                    }
                }
            }
            else
            {
                for (int i = 0; i < tebo.Count; i++)
                {
                    tebo2[i].Background = Brushes.Gainsboro;

                    if (i % 2 == 0)
                    {
                        tebo2[i].Background = Brushes.Silver;
                    }
                }
            }

            for (int i = 0; i < lines.GetLength(0); i++)
            {
                lines[i].Stroke = Brushes.Black;
                lines[i].Opacity = 0.5;
                if (i % 2 == 0)
                {
                    lines[i].Stroke = Brushes.Black;
                }
            }

            //System.GC.Collect();
        }

        #endregion

        #region Constructor and Sync - Controller

        public void changeoffset(int offset, int ringoffset)
        {
            tebo = new List<TextBlock>();
            tebo2 = new List<TextBlock>();
            lines = new Line[26];
            custom = new TextBlock();
            custom2 = new TextBlock();
            lineCanvas = new Canvas();
            lineToAnimat = new Line();
            lineToAnimat2 = new Line();
            lineTrash = new List<Line>();
            stack = new StackPanel();
            stack1 = new StackPanel();
            stack2 = new StackPanel();
            iAm = new TextBlock();


            Children.Remove(content);
            content = alpha(offset, ringoffset - 1);
            Children.Add(content);

        }

        public Rotor2(int version, int map, double width, double height, int offset, int ringoffset)
        {
            StackPanel s = new StackPanel
            {
                Orientation = Orientation.Vertical
            };

            this.width = width;
            this.height = height;
            this.version = version;
            this.map = map;

            content = alpha(offset, ringoffset - 1);
            Children.Add(content);
        }

        private Canvas alpha(int offset, int ringoffset)
        {
            Rectangle myRectangle = new Rectangle
            {
                Width = 200,
                Height = 890,

                RadiusX = 15,
                RadiusY = 15,

                Fill = Brushes.LightSteelBlue,
                Stroke = Brushes.Silver,
                StrokeThickness = 30
            };

            Rectangle walzeDisplay = new Rectangle
            {
                Width = 50,
                Height = 50,

                RadiusX = 5,
                RadiusY = 5,

                Fill = Brushes.Silver,
                Stroke = Brushes.Silver,
                StrokeThickness = 30
            };

            stack.Orientation = Orientation.Vertical;
            stack1.Orientation = Orientation.Vertical;
            stack2.Orientation = Orientation.Horizontal;

            Canvas.SetTop(stack1, 60);
            Canvas.SetLeft(stack1, 0);

            Canvas.SetTop(stack, 60);
            Canvas.SetLeft(stack, 170);

            Canvas temp = new Canvas();

            temp.Children.Add(walzeDisplay);

            temp.Children.Add(myRectangle);
            temp.Children.Add(lineCanvas);

            int max = 26;
            int rest = 26 - offset;

            for (int i = offset; i < max; i++)
            {

                int inew = (i - offset + ringoffset) % 26;
                TextBlock t = new TextBlock
                {
                    Text = "" + ((26 - i + ringoffset) % 26 + 1),
                    Width = 29.4,
                    Height = 29.4,


                    FontSize = 20,
                    Background = Brushes.Gainsboro,
                    TextAlignment = TextAlignment.Center
                };
                if ((int.Parse(t.Text)) % 2 != 0)
                {
                    t.Background = Brushes.Silver;
                }

                if (i == 0)
                {
                    t.FontWeight = FontWeights.UltraBold;
                    t.Foreground = Brushes.OrangeRed;
                    t.FontSize = 22;

                }


                stack.Children.Add(t);

                tebo.Add(t);

                TextBlock t2 = new TextBlock
                {
                    Text = "" + Convert.ToChar(i + 65),
                    Width = 29.4,
                    Height = 29.4,


                    FontSize = 20,
                    Background = Brushes.Gainsboro,
                    TextAlignment = TextAlignment.Center
                };
                if (i % 2 == 0)
                {
                    t2.Background = Brushes.Silver;
                }

                stack1.Children.Add(t2);

                tebo2.Add(t2);

                Line t1 = new Line
                {
                    X1 = 170,
                    Y1 = 29.4 * inew + 75,
                    Opacity = 0.5,
                    X2 = 30,
                    Y2 = 0,

                    Stroke = Brushes.Black
                };



                maparray[inew, 0] = inew;
                switch (map)
                {
                    case 1:
                        maparray[inew, 1] = ((getRotorsAsInt(version, 0)[i] + rest + ringoffset) % 26);
                        t1.Y2 = 29.4 * ((getRotorsAsInt(version, 0)[i] + rest + ringoffset) % 26) + 75;
                        iAm.Text = "I";
                        nextint[0] = getNotchesAsInt(version, 0)[0];
                        if (offset == nextint[0])
                        {
                            next = true;
                        }

                        break;

                    case 2:
                        maparray[inew, 1] = (getRotorsAsInt(version, 1)[i] + rest + ringoffset) % 26;
                        t1.Y2 = 29.4 * ((getRotorsAsInt(version, 1)[i] + rest + ringoffset) % 26) + 75;
                        nextint[0] = getNotchesAsInt(version, 1)[0];
                        iAm.Text = "II";
                        if (offset == nextint[0])
                        {
                            next = true;
                        }

                        break;
                    case 3:
                        maparray[inew, 1] = (getRotorsAsInt(version, 2)[i] + rest + ringoffset) % 26;
                        t1.Y2 = 29.4 * ((getRotorsAsInt(version, 2)[i] + rest + ringoffset) % 26) + 75;
                        nextint[0] = getNotchesAsInt(version, 2)[0];
                        iAm.Text = "III";
                        if (offset == nextint[0])
                        {
                            next = true;
                        }

                        break;
                    case 4:
                        maparray[inew, 1] = (getRotorsAsInt(version, 3)[i] + rest + ringoffset) % 26;
                        t1.Y2 = 29.4 * ((getRotorsAsInt(version, 3)[i] + rest + ringoffset) % 26) + 75;
                        nextint[0] = getNotchesAsInt(version, 3)[0];
                        iAm.Text = "IV";
                        if (offset == nextint[0])
                        {
                            next = true;
                        }

                        break;
                    case 5:
                        maparray[inew, 1] = (getRotorsAsInt(version, 4)[i] + rest + ringoffset) % 26;
                        t1.Y2 = 29.4 * ((getRotorsAsInt(version, 4)[i] + rest + ringoffset) % 26) + 75;
                        nextint[0] = getNotchesAsInt(version, 4)[0];
                        iAm.Text = "V";
                        if (offset == nextint[0])
                        {
                            next = true;
                        }

                        if (i == 0)
                        {
                            t2.Foreground = Brushes.OrangeRed;
                            t2.FontWeight = FontWeights.UltraBold;
                            t2.FontSize = 22;
                        }
                        break;

                    case 6:
                        maparray[inew, 1] = (getRotorsAsInt(version, 5)[i] + rest + ringoffset) % 26;
                        t1.Y2 = 29.4 * ((getRotorsAsInt(version, 5)[i] + rest + ringoffset) % 26) + 75;
                        nextint = sechsnext;
                        iAm.Text = "VI";
                        if (offset == nextint[0] || offset == nextint[1])
                        {
                            next = true;
                        }

                        break;
                    case 7:
                        maparray[inew, 1] = (getRotorsAsInt(version, 6)[i] + rest + ringoffset) % 26;
                        t1.Y2 = 29.4 * ((getRotorsAsInt(version, 6)[i] + rest + ringoffset) % 26) + 75;
                        nextint = siebennext;
                        iAm.Text = "VII";
                        if (offset == nextint[0] || offset == nextint[1])
                        {
                            next = true;
                        }

                        break;
                    case 8:
                        maparray[inew, 1] = (getRotorsAsInt(version, 7)[i] + rest + ringoffset) % 26;
                        t1.Y2 = 29.4 * ((getRotorsAsInt(version, 7)[i] + rest + ringoffset) % 26) + 75;
                        nextint = achtnext;
                        iAm.Text = "VIII";
                        if (offset == nextint[0] || offset == nextint[1])
                        {
                            next = true;
                        }

                        break;

                }

                lines[inew] = t1;



                if (i == nextint[0] + 1 || i == nextint[1] + 1)
                {
                    t2.FontWeight = FontWeights.UltraBold;
                    t2.Foreground = Brushes.OrangeRed;
                    t2.FontSize = 22;
                }


                lineCanvas.Children.Add(t1);

            }

            for (int i = 0; i < offset; i++)
            {
                int inew = (i + rest + ringoffset) % 26;

                TextBlock t = new TextBlock
                {
                    Text = "" + ((26 - i + ringoffset) % 26 + 1),
                    Width = 29.4,
                    Height = 29.4,


                    FontSize = 20,
                    Background = Brushes.Gainsboro,
                    TextAlignment = TextAlignment.Center
                };
                if ((int.Parse(t.Text)) % 2 != 0)
                {
                    t.Background = Brushes.Silver;
                }

                if (i == 0)
                {
                    t.FontWeight = FontWeights.UltraBold;
                    t.Foreground = Brushes.OrangeRed;
                    t.FontSize = 22;

                }


                stack.Children.Add(t);

                tebo.Add(t);

                TextBlock t2 = new TextBlock
                {
                    Text = "" + Convert.ToChar(i + 65),
                    Width = 29.4,
                    Height = 29.4,


                    FontSize = 20,
                    Background = Brushes.Gainsboro,
                    TextAlignment = TextAlignment.Center
                };
                if (i % 2 == 0)
                {
                    t2.Background = Brushes.Silver;
                }

                stack1.Children.Add(t2);

                tebo2.Add(t2);

                Line t1 = new Line
                {
                    X1 = 170,
                    Y1 = 29.4 * ((i - offset + 26 + ringoffset) % 26) + 75,
                    Opacity = 0.5,
                    X2 = 30,
                    Y2 = 0,

                    Stroke = Brushes.Black
                };



                maparray[inew, 0] = inew;
                switch (map)
                {
                    case 1:
                        maparray[inew, 1] = ((getRotorsAsInt(version, 0)[i] + rest + ringoffset) % 26);
                        t1.Y2 = 29.4 * ((getRotorsAsInt(version, 0)[i] + rest + ringoffset) % 26) + 75;
                        iAm.Text = "I";
                        nextint[0] = getNotchesAsInt(version, 0)[0];
                        break;

                    case 2:
                        maparray[inew, 1] = ((getRotorsAsInt(version, 1)[i] + rest + ringoffset) % 26);
                        t1.Y2 = 29.4 * ((getRotorsAsInt(version, 1)[i] + rest + ringoffset) % 26) + 75;
                        nextint[0] = getNotchesAsInt(version, 1)[0];
                        iAm.Text = "II";
                        break;
                    case 3:
                        maparray[inew, 1] = (getRotorsAsInt(version, 2)[i] + rest + ringoffset) % 26;
                        t1.Y2 = 29.4 * ((getRotorsAsInt(version, 2)[i] + rest + ringoffset) % 26) + 75;
                        nextint[0] = getNotchesAsInt(version, 2)[0];
                        iAm.Text = "III";
                        break;
                    case 4:
                        maparray[inew, 1] = (getRotorsAsInt(version, 3)[i] + rest + ringoffset) % 26;
                        t1.Y2 = 29.4 * ((getRotorsAsInt(version, 3)[i] + rest + ringoffset) % 26) + 75;
                        nextint[0] = getNotchesAsInt(version, 3)[0];
                        iAm.Text = "IV";
                        break;
                    case 5:
                        maparray[inew, 1] = (getRotorsAsInt(version, 4)[i] + rest + ringoffset) % 26;
                        t1.Y2 = 29.4 * ((getRotorsAsInt(version, 4)[i] + rest + ringoffset) % 26) + 75;
                        nextint[0] = getNotchesAsInt(version, 4)[0];
                        iAm.Text = "V";
                        if (i == 0)
                        {
                            t2.Foreground = Brushes.OrangeRed;
                            t2.FontWeight = FontWeights.UltraBold;
                            t2.FontSize = 22;
                        }
                        break;
                    case 6:
                        maparray[inew, 1] = (getRotorsAsInt(version, 5)[i] + rest + ringoffset) % 26;
                        t1.Y2 = 29.4 * ((getRotorsAsInt(version, 5)[i] + rest + ringoffset) % 26) + 75;
                        nextint = sechsnext;
                        iAm.Text = "VI";
                        if (i == 0)
                        {
                            t2.Foreground = Brushes.OrangeRed;
                            t2.FontWeight = FontWeights.UltraBold;
                            t2.FontSize = 22;
                        }
                        break;
                    case 7:
                        maparray[inew, 1] = (getRotorsAsInt(version, 6)[i] + rest + ringoffset) % 26;
                        t1.Y2 = 29.4 * ((getRotorsAsInt(version, 6)[i] + rest + ringoffset) % 26) + 75;
                        nextint = siebennext;
                        iAm.Text = "VII";
                        if (i == 0)
                        {
                            t2.Foreground = Brushes.OrangeRed;
                            t2.FontWeight = FontWeights.UltraBold;
                            t2.FontSize = 22;
                        }
                        break;
                    case 8:
                        maparray[inew, 1] = (getRotorsAsInt(version, 7)[i] + rest + ringoffset) % 26;
                        t1.Y2 = 29.4 * ((getRotorsAsInt(version, 7)[i] + rest + ringoffset) % 26) + 75;
                        nextint = achtnext;
                        iAm.Text = "VIII";
                        if (i == 0)
                        {
                            t2.Foreground = Brushes.OrangeRed;
                            t2.FontWeight = FontWeights.UltraBold;
                            t2.FontSize = 22;
                        }
                        break;

                }
                lines[inew] = t1;



                if (i == nextint[0] + 1 || i == nextint[1] + 1)
                {
                    t2.FontWeight = FontWeights.UltraBold;
                    t2.Foreground = Brushes.OrangeRed;
                    t2.FontSize = 22;
                }


                lineCanvas.Children.Add(t1);

            }

            temp.Children.Add(stack1);
            temp.Children.Add(stack);

            up = new Button();
            up.Click += upperclick;
            up.Height = 50;
            up.Width = 50;
            up.Content = "UP";
            Canvas.SetLeft(up, 25);
            Canvas.SetTop(up, 5);

            down = new Button();
            down.Click += downerclick;
            down.Height = 50;
            down.Width = 50;
            down.Content = "Down";
            Canvas.SetLeft(down, 125);
            Canvas.SetTop(down, 5);

            custom.Text = "" + Convert.ToChar(offset + 65);

            custom.Height = 50;
            custom.Width = 50;
            custom.FontSize = 40;
            custom.TextAlignment = TextAlignment.Center;
            Canvas.SetLeft(custom, 75);
            Canvas.SetTop(custom, 5);



            Rectangle k = new Rectangle
            {
                RadiusX = 5,
                RadiusY = 5,
                Height = 40,
                Width = 40,
                Opacity = 1.0,
                StrokeThickness = 3.0,
                Stroke = Brushes.RoyalBlue,
                Fill = Brushes.Silver
            };
            Canvas.SetTop(k, 13);
            Canvas.SetLeft(k, 80);

            temp.Children.Add(k);

            Rectangle k1 = new Rectangle
            {
                RadiusX = 5,
                RadiusY = 5,
                Height = 30,
                Width = 30,
                Opacity = 1.0,
                StrokeThickness = 3.0,
                Stroke = Brushes.RoyalBlue
            };
            //k.Fill = Brushes.DimGray;
            Canvas.SetTop(k1, 60);
            Canvas.SetLeft(k1, 0);

            temp.Children.Add(k1);





            Line lx = new Line
            {
                X1 = 100,
                Y1 = 54,
                Opacity = 1,
                X2 = 100,
                Y2 = 70,
                StrokeThickness = 3.0,
                Stroke = Brushes.RoyalBlue
            };

            temp.Children.Add(lx);

            Line lx1 = new Line
            {
                X1 = 100,
                Y1 = 70,
                Opacity = 1,
                X2 = 30,
                Y2 = 70,
                StrokeThickness = 3.0,
                Stroke = Brushes.RoyalBlue
            };

            temp.Children.Add(lx1);

            temp.Children.Add(custom);
            temp.Children.Add(up);
            temp.Children.Add(down);

            up1 = new Button();
            up1.Click += upperclick1;
            up1.Height = 50;
            up1.Width = 50;
            up1.Content = "UP";
            Canvas.SetLeft(up1, 25);
            Canvas.SetTop(up1, 830);

            down1 = new Button();
            down1.Click += downerclick1;
            down1.Height = 50;
            down1.Width = 50;
            down1.Content = "Down";
            Canvas.SetTop(down1, 830);
            Canvas.SetLeft(down1, 125);

            custom2.Text = "" + (ringoffset + 1);

            custom2.Height = 50;
            custom2.Width = 50;
            custom2.FontSize = 40;
            custom2.TextAlignment = TextAlignment.Center;

            Canvas.SetLeft(custom2, 75);
            Canvas.SetTop(custom2, 830);

            temp.Children.Add(custom2);
            temp.Children.Add(up1);
            temp.Children.Add(down1);


            iAm.Height = 50;
            iAm.Width = 50;
            iAm.FontSize = 30;
            iAm.TextAlignment = TextAlignment.Center;
            Canvas.SetLeft(iAm, -30);
            Canvas.SetTop(iAm, 5);
            iAm.Background = Brushes.Orange;
            iAm.Uid = "" + map;
            temp.Children.Add(iAm);

            return temp;
        }

        #endregion


    }
}
