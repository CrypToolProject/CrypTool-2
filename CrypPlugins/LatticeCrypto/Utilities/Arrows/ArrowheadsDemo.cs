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
//-----------------------------------------------
// ArrowheadsDemo.cs (c) 2007 by Charles Petzold
//-----------------------------------------------

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace LatticeCrypto.Utilities.Arrows
{
    public class Arrowheads : Window
    {
        //[STAThread]
        //public static void Main()
        //{
        //    Application app = new Application();
        //    app.Run(new Arrowheads());
        //}

        public Arrowheads()
        {
            Canvas canv = new Canvas();
            Content = canv;

            // ArrowLine with animated arrow properties.
            ArrowLine aline1 = new ArrowLine
            { Stroke = Brushes.Red, StrokeThickness = 3, X1 = 100, Y1 = 400, X2 = 400, Y2 = 100 };
            canv.Children.Add(aline1);

            DoubleAnimation animaDouble1 = new DoubleAnimation(10, 50, new Duration(new TimeSpan(0, 0, 5)))
            { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever };
            aline1.BeginAnimation(ArrowLineBase.ArrowAngleProperty, animaDouble1);

            DoubleAnimation animaDouble2 = new DoubleAnimation(10, 200, new Duration(new TimeSpan(0, 0, 5)))
            { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever };
            aline1.BeginAnimation(ArrowLineBase.ArrowLengthProperty, animaDouble2);

            // ArrowLine with animated point properties.
            ArrowLine aline2 = new ArrowLine
            {
                ArrowEnds = ArrowEnds.Both,
                Stroke = Brushes.Blue,
                StrokeThickness = 1,
                X1 = 100,
                Y1 = 100,
                X2 = 200,
                Y2 = 400
            };

            canv.Children.Add(aline2);

            AnimationTimeline animaDouble3 = new DoubleAnimation(100, 400, new Duration(new TimeSpan(0, 0, 5)))
            {
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            aline2.BeginAnimation(ArrowLine.X1Property, animaDouble3);

            AnimationTimeline animaDouble4 = new DoubleAnimation(400, 100, new Duration(new TimeSpan(0, 0, 5)))
            {
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            aline2.BeginAnimation(ArrowLine.Y2Property, animaDouble4);

            // ArrowPolyline rotated.            
            ArrowPolyline apoly = new ArrowPolyline();
            RotateTransform xform = new RotateTransform();
            apoly.LayoutTransform = xform;
            apoly.ArrowEnds = ArrowEnds.Both;
            apoly.Stroke = Brushes.Green;
            apoly.StrokeThickness = 3;

            apoly.Points.Add(new Point(25, 25));
            apoly.Points.Add(new Point(125, 25));
            apoly.Points.Add(new Point(125, 125));
            apoly.Points.Add(new Point(25, 125));

            canv.Children.Add(apoly);

            AnimationTimeline animaDouble5 = new DoubleAnimation(0, 360, new Duration(new TimeSpan(0, 0, 10)))
            {
                RepeatBehavior = RepeatBehavior.Forever
            };
            xform.BeginAnimation(RotateTransform.AngleProperty, animaDouble5);
        }
    }
}