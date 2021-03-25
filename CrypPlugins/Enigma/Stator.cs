using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace CrypTool.Enigma
{
    class Stator : Canvas
    {
        double width;
        double height;
        public readonly int map;
        private int model = 3;

        StackPanel stack = new StackPanel();
        StackPanel stack1 = new StackPanel();
        StackPanel stack2 = new StackPanel();

        Line[] lines = new Line[26];

        Canvas lineCanvas = new Canvas();

        List<Line> lineTrash = new List<Line>();

        List<TextBlock> tebo = new List<TextBlock>();
        List<TextBlock> tebo2 = new List<TextBlock>();

        TextBlock[] textBlockToAnimat = new TextBlock[2];
        TextBlock[] textBlockToAnimat2 = new TextBlock[2];

        Line lineToAnimat = new Line();
        Line lineToAnimat2 = new Line();

        

        private double timecounter = 0.0;

        private Canvas content;

        public int[,] maparray = new int[26, 2];

        

        private static int[] getStatorAsInt(int model)
        {
            int[] value = new int[26];

            for (int i = 0; i < EnigmaCore.stators[model].Length; i++)
            {
                value[i] = EnigmaCore.stators[model][i] - 65;
            }
            return value;
        }

         public Stator(int model, double width, double height)
        {
            StackPanel s = new StackPanel();
            s.Orientation = Orientation.Vertical;

            this.width = width;
            this.height = height;
            this.model = model;
            this.content = alpha();
            this.Children.Add(content);
        }
         #region mapping
         public int mapto(int x)
        {
             
             lineToAnimat = lines[maparray[x, 0]];
             textBlockToAnimat[0] = tebo[maparray[x, 0]];
             textBlockToAnimat[1] = tebo2[maparray[x, 1]];
             //rotated = false;
             

             return maparray[x, 1];
         }

         public int maptoreverse(int y)
         {
             int help = 0;
             for (int x = 0; x < maparray.GetLength(0); x++)
             {
                 if (maparray[x, 1] == y)
                     help = x;

             }

             textBlockToAnimat2[1] = tebo2[maparray[y, 0]];
             textBlockToAnimat2[0] = tebo[help];

             lineToAnimat2 = lines[help];
             return help;
         }

        #endregion

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

       private ColorAnimation animateThisTebo(TextBlock tebo, Boolean b)
       {


           ColorAnimation colorAni = new ColorAnimation();
           string s = tebo.Text;
           colorAni.From = Colors.Gainsboro;
           if (tebo.Background == Brushes.Silver)
               colorAni.From = Colors.Silver;
           if (b)
               colorAni.To = Colors.YellowGreen;
           else
               colorAni.To = Colors.Tomato;
           colorAni.Duration = new Duration(TimeSpan.FromMilliseconds(1000));
           colorAni.BeginTime = TimeSpan.FromMilliseconds(timecounter);
           Storyboard.SetTarget(colorAni, tebo);
           Storyboard.SetTargetProperty(colorAni, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));

           timecounter += 1000;

           return colorAni;
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
           mydouble1.BeginTime = TimeSpan.FromMilliseconds(timecounter);
           
            l1.Y1 = l.Y1;
           l1.Y2 = l.Y1;


           mydouble1.From = l.Y1;
           mydouble1.To = l.Y2;
           mydouble1.Duration = new Duration(TimeSpan.FromMilliseconds((1000)));

           DoubleAnimation mydouble = new DoubleAnimation();
           mydouble.From = l.X1;
           mydouble.To = l.X2;
           mydouble.Duration = new Duration(TimeSpan.FromMilliseconds((1000)));
           mydouble.BeginTime = TimeSpan.FromMilliseconds(timecounter);
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

       public void resetColors()
       {
           foreach (Line l in lineTrash)
               lineCanvas.Children.Remove(l);

           lineTrash.Clear();


           
            for (int i = 0; i < tebo.Count; i++)
               {
                   tebo[i].Background = Brushes.Gainsboro;

                   if (i % 2 == 0)
                       tebo[i].Background = Brushes.Silver;
               }
           

            for (int i = 0; i < tebo2.Count; i++)
               {
                   tebo2[i].Background = Brushes.Gainsboro;

                   if (i % 2 == 0)
                       tebo2[i].Background = Brushes.Silver;


               }
           

           for (int i = 0; i < lines.GetLength(0); i++)
           {
               lines[i].Stroke = Brushes.Black;
               lines[i].Opacity = 0.5;
               if (i % 2 == 0)
                   lines[i].Stroke = Brushes.Black;
           }

           System.GC.Collect();
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
           
           l1.Y1 = l.Y2;
           l1.Y2 = l.Y2;

           mydouble1.From = l.Y2;
           mydouble1.To = l.Y1;
           mydouble1.Duration = new Duration(TimeSpan.FromMilliseconds((1000)));

           DoubleAnimation mydouble = new DoubleAnimation();
           mydouble.From = l.X2;
           mydouble.To = l.X1;
           mydouble.Duration = new Duration(TimeSpan.FromMilliseconds((1000)));
           mydouble.BeginTime = TimeSpan.FromMilliseconds(timecounter);
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

       private Canvas alpha()
         {
             Rectangle myRectangle = new Rectangle();
             myRectangle.Width = 200;
             myRectangle.Height = 890;

             myRectangle.RadiusX = 15;
             myRectangle.RadiusY = 15;

             myRectangle.Fill = Brushes.LightSteelBlue;
             myRectangle.Stroke = Brushes.Silver;
             myRectangle.StrokeThickness = 30;

             Rectangle walzeDisplay = new Rectangle();
             walzeDisplay.Width = 50;
             walzeDisplay.Height = 50;

             walzeDisplay.RadiusX = 5;
             walzeDisplay.RadiusY = 5;

             walzeDisplay.Fill = Brushes.Silver;
             walzeDisplay.Stroke = Brushes.Silver;
             walzeDisplay.StrokeThickness = 30;

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
             

             for (int i = 0; i < 26; i++)
             {

                 
                 TextBlock t = new TextBlock();
                 t.Text = "" + Convert.ToChar(i + 65);
                 t.Width = 29.4;
                 t.Height = 29.4;


                 t.FontSize = 20;
                 t.Background = Brushes.Gainsboro;
                 t.TextAlignment = TextAlignment.Center;
                 if (i % 2 == 0)
                     t.Background = Brushes.Silver;

                 stack.Children.Add(t);

                 tebo.Add(t);

                 TextBlock t2 = new TextBlock();
                 t2.Text = "" + Convert.ToChar(i + 65);
                 t2.Width = 29.4;
                 t2.Height = 29.4;


                 t2.FontSize = 20;
                 t2.Background = Brushes.Gainsboro;
                 t2.TextAlignment = TextAlignment.Center;
                 if (i % 2 == 0)
                     t2.Background = Brushes.Silver;



                 stack1.Children.Add(t2);

                 tebo2.Add(t2);

                 Line linetemp = new Line();
                 linetemp.X1 = 170;
                 linetemp.Y1 = 29.4 * i + 75;
                 linetemp.Opacity = 0.5;
                 linetemp.X2 = 30;
                 linetemp.Y2 = 0;

                 

                 linetemp.Stroke = Brushes.Black;


                 maparray[i, 1] = getStatorAsInt(model)[i]  ;
                 linetemp.Y2 = 29.4 * getStatorAsInt(model)[i] + 75;
                 
                 
                 maparray[i, 0] = i;
                 
                 lineCanvas.Children.Add(linetemp);
                 lines[i] = linetemp;
             }

             temp.Children.Add(stack1);
             temp.Children.Add(stack);

             return temp;
         }
    }
}
