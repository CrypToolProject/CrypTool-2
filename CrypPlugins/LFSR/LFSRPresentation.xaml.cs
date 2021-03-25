/*
   Copyright 2009 Sören Rinne, Ruhr-Universität Bochum, Germany

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

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Threading;
using System.Windows.Threading;
using CrypTool.PluginBase;

namespace CrypTool.LFSR
{
    /// <summary>
    /// Interaction logic for LFSRPresentation.xaml
    /// </summary>
    //[Localization("LFSR.Properties.Resources")]
    public partial class LFSRPresentation : UserControl
    {
        public LFSRPresentation()
        {
            InitializeComponent();
        }

        public void DrawLFSR(char[] state, char[] tapSequence, int clockingBit)
        {
            try
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    try
                    {
                        const double x0 = 25;
                        const double boxWidth = 30;
                        double boxesWidth = state.Length * boxWidth;
                        const double arrowWidth = 7;
                        const double arrowHeight = 6;

                        // hide initial textbox
                        infoText.Visibility = Visibility.Hidden;
                        polynomialText.Visibility = Visibility.Visible;

                        // add lines and triangles
                        Line HoriLine1 = new Line();
                        HoriLine1.X1 = x0;
                        HoriLine1.Y1 = 18;
                        HoriLine1.X2 = x0 + boxesWidth + 55;
                        HoriLine1.Y2 = 18;
                        HoriLine1.Stroke = Brushes.Black;
                        HoriLine1.StrokeThickness = 1;
                        myGrid.Children.Add(HoriLine1);

                        Line HoriLine2 = new Line();
                        HoriLine2.X1 = x0;
                        HoriLine2.Y1 = 47;
                        HoriLine2.X2 = x0 + boxesWidth - state.Length + 1;
                        HoriLine2.Y2 = 47;
                        HoriLine2.Stroke = Brushes.Black;
                        HoriLine1.StrokeThickness = 1;
                        myGrid.Children.Add(HoriLine2);

                        Line VertLine1 = new Line();
                        VertLine1.X1 = x0;
                        VertLine1.Y1 = 17.5;
                        VertLine1.X2 = x0;
                        VertLine1.Y2 = 47.5;
                        VertLine1.Stroke = Brushes.Black;
                        VertLine1.StrokeThickness = 1;
                        myGrid.Children.Add(VertLine1);

                        Line VertLine2 = new Line();
                        VertLine2.X1 = x0 + boxesWidth - state.Length + 1;
                        VertLine2.Y1 = 32;
                        VertLine2.X2 = x0 + boxesWidth - state.Length + 1;
                        VertLine2.Y2 = 47;
                        VertLine2.Stroke = Brushes.Black;
                        VertLine2.StrokeThickness = 1;
                        myGrid.Children.Add(VertLine2);

                        // add connection circle
                        /*Ellipse ConnectionCircle = new Ellipse();
                        ConnectionCircle.HorizontalAlignment = HorizontalAlignment.Left;
                        ConnectionCircle.VerticalAlignment = VerticalAlignment.Top;
                        ConnectionCircle.Fill = Brushes.Black;
                        ConnectionCircle.Width = 4;
                        ConnectionCircle.Height = 4;
                        ConnectionCircle.Margin = new Thickness(27.5 + state.Length * 30, 15.5, 0, 0);
                        myGrid.Children.Add(ConnectionCircle);*/

                        // add left triangle ////////////////////
                        // Create a path to draw a geometry with.
                        Path leftTriangle = new Path();
                        leftTriangle.Stroke = Brushes.Black;
                        leftTriangle.StrokeThickness = 1;
                        leftTriangle.Fill = Brushes.Black;

                        // Create a StreamGeometry to use to specify myPath.
                        StreamGeometry geometryLT = new StreamGeometry();
                        geometryLT.FillRule = FillRule.EvenOdd;

                        // Open a StreamGeometryContext that can be used to describe this StreamGeometry 
                        // object's contents.
                        using (StreamGeometryContext ctx = geometryLT.Open())
                        {
                            double x = x0 + 8;
                            double y = 18;
                            // Begin the triangle at the point specified. Notice that the shape is set to 
                            // be closed so only two lines need to be specified below to make the triangle.
                            ctx.BeginFigure(new Point(x, y - arrowHeight / 2), true /* is filled */, true /* is closed */);

                            // Draw a line to the next specified point.
                            ctx.LineTo(new Point(x, y + arrowHeight / 2), true /* is stroked */, false /* is smooth join */);

                            // Draw another line to the next specified point.
                            ctx.LineTo(new Point(x + arrowWidth, y), true /* is stroked */, false /* is smooth join */);
                        }

                        // Freeze the geometry (make it unmodifiable)
                        // for additional performance benefits.
                        geometryLT.Freeze();

                        // Specify the shape (triangle) of the Path using the StreamGeometry.
                        leftTriangle.Data = geometryLT;

                        myGrid.Children.Add(leftTriangle);

                        // add right triangle ///////////////////
                        // Create a path to draw a geometry with.
                        Path rightTriangle = new Path();
                        rightTriangle.Stroke = Brushes.Black;
                        rightTriangle.StrokeThickness = 1;
                        rightTriangle.Fill = Brushes.Black;

                        // Create a StreamGeometry to use to specify myPath.
                        StreamGeometry geometryRT = new StreamGeometry();
                        geometryRT.FillRule = FillRule.EvenOdd;

                        // Open a StreamGeometryContext that can be used to describe this StreamGeometry 
                        // object's contents.
                        using (StreamGeometryContext ctx = geometryRT.Open())
                        {
                            double x = x0 + 55 + boxesWidth;
                            double y = 18;
                            // Begin the triangle at the point specified. Notice that the shape is set to 
                            // be closed so only two lines need to be specified below to make the triangle.
                            ctx.BeginFigure(new Point(x, y - arrowHeight / 2), true /* is filled */, true /* is closed */);

                            // Draw a line to the next specified point.
                            ctx.LineTo(new Point(x, y + arrowHeight / 2), true /* is stroked */, false /* is smooth join */);

                            // Draw another line to the next specified point.
                            ctx.LineTo(new Point(x + 7, y), true /* is stroked */, false /* is smooth join */);
                        }

                        // Freeze the geometry (make it unmodifiable)
                        // for additional performance benefits.
                        geometryRT.Freeze();

                        // Specify the shape (triangle) of the Path using the StreamGeometry.
                        rightTriangle.Data = geometryRT;

                        // Data="M180,14 L180,22 L187,18 Z"
                        myGrid.Children.Add(rightTriangle);



                        TextBox[] myTextBoxes = new TextBox[state.Length];
                        Grid[] myGrids = new Grid[state.Length];
                        Ellipse[] myEllipses = new Ellipse[state.Length];
                        Line[] myLinesVert = new Line[state.Length];
                        Line[] myLinesVertRed = new Line[state.Length];
                        Line[] myLinesHori = new Line[state.Length];

                        // add TextBoxes
                        int i;
                        double left;
                        for (i = 0; i < state.Length; i++)
                        {
                            // add textboxes
                            left = x0 + (double)i * 29 + 15;
                            myTextBoxes[i] = new TextBox();
                            myTextBoxes[i].Margin = new Thickness(left, 3, 0, 0);
                            myTextBoxes[i].Width = boxWidth;
                            myTextBoxes[i].Height = boxWidth;
                            myTextBoxes[i].HorizontalAlignment = HorizontalAlignment.Left;
                            myTextBoxes[i].VerticalAlignment = VerticalAlignment.Top;
                            myTextBoxes[i].Name = "textBoxBit" + i;
                            myTextBoxes[i].Visibility = Visibility.Visible;
                            myTextBoxes[i].BorderThickness = new Thickness(1);
                            myTextBoxes[i].IsReadOnly = true;
                            myTextBoxes[i].TextAlignment = TextAlignment.Center;
                            myTextBoxes[i].VerticalContentAlignment = VerticalAlignment.Center;
                            myTextBoxes[i].BorderBrush = Brushes.Black;
                            //if (tapSequence[i] == '1') myTextBoxes[i].Background = Brushes.DodgerBlue;
                            if (clockingBit == i) myTextBoxes[i].Background = Brushes.Orange;

                            myGrid.Children.Add(myTextBoxes[i]);

                            // add XORs
                            myGrids[i] = new Grid();
                            myGrids[i].Name = "XORGrid" + i;
                            myGrids[i].Height = boxWidth;
                            myGrids[i].Width = boxWidth;
                            myGrids[i].HorizontalAlignment = HorizontalAlignment.Left;
                            myGrids[i].VerticalAlignment = VerticalAlignment.Top;
                            myGrids[i].Margin = new Thickness(left, 32, 0, 0);

                            myGrid.Children.Add(myGrids[i]);

                            if (tapSequence[i] == '0') myGrids[i].Visibility = Visibility.Hidden;
                            else
                            {
                                myGrids[i].Visibility = Visibility.Visible;

                                myEllipses[i] = new Ellipse();
                                myEllipses[i].Name = "ellipseXOR" + i;
                                myEllipses[i].Stroke = Brushes.DodgerBlue;
                                myEllipses[i].Margin = new Thickness(9, 9, 9, 9);

                                myLinesVert[i] = new Line();
                                myLinesVert[i].Name = "VertLineXOR" + i;
                                myLinesVert[i].Stroke = Brushes.Black;
                                myLinesVert[i].StrokeThickness = 1;
                                myLinesVert[i].X1 = boxWidth / 2;
                                myLinesVert[i].Y1 = 0.5;
                                myLinesVert[i].X2 = boxWidth / 2;
                                myLinesVert[i].Y2 = 9;

                                myLinesVertRed[i] = new Line();
                                myLinesVertRed[i].Name = "VertLineXORRed" + i;
                                myLinesVertRed[i].Stroke = Brushes.DodgerBlue;
                                myLinesVertRed[i].StrokeThickness = 1;
                                myLinesVertRed[i].X1 = boxWidth / 2;
                                myLinesVertRed[i].Y1 = 9;
                                myLinesVertRed[i].X2 = boxWidth / 2;
                                myLinesVertRed[i].Y2 = 20;

                                myLinesHori[i] = new Line();
                                myLinesHori[i].Name = "HoriLineXOR" + i;
                                myLinesHori[i].Stroke = Brushes.DodgerBlue;
                                myLinesHori[i].StrokeThickness = 1;
                                myLinesHori[i].X1 = 9;
                                myLinesHori[i].Y1 = boxWidth / 2;
                                myLinesHori[i].X2 = 20;
                                myLinesHori[i].Y2 = boxWidth / 2;

                                myGrids[i].Children.Add(myEllipses[i]);
                                myGrids[i].Children.Add(myLinesVert[i]);
                                myGrids[i].Children.Add(myLinesVertRed[i]);
                                myGrids[i].Children.Add(myLinesHori[i]);
                            }
                        }
                        // disable /*last*/ and first XOR
                        //myGrids[0].Visibility = Visibility.Hidden;
                        myGrids[state.Length - 1].Visibility = Visibility.Hidden;

                        // add output bit label
                        Label outPutLabel = new Label();
                        left = x0 + 60 + i * boxWidth;
                        outPutLabel.Margin = new Thickness(left, 3, 0, 0);
                        outPutLabel.Width = boxWidth;
                        outPutLabel.Height = boxWidth;
                        outPutLabel.HorizontalContentAlignment = HorizontalAlignment.Center;
                        outPutLabel.VerticalContentAlignment = VerticalAlignment.Center;
                        outPutLabel.HorizontalAlignment = HorizontalAlignment.Left;
                        outPutLabel.VerticalAlignment = VerticalAlignment.Top;
                        outPutLabel.Name = "outputLabel";
                        myGrid.Children.Add(outPutLabel);

                        // add input bit label
                        Label inPutLabel = new Label();
                        left = 0;
                        inPutLabel.Margin = new Thickness(left, 18, 0, 0);
                        inPutLabel.Width = boxWidth;
                        inPutLabel.Height = boxWidth;
                        inPutLabel.HorizontalContentAlignment = HorizontalAlignment.Center;
                        inPutLabel.VerticalContentAlignment = VerticalAlignment.Center;
                        inPutLabel.HorizontalAlignment = HorizontalAlignment.Left;
                        inPutLabel.VerticalAlignment = VerticalAlignment.Top;
                        inPutLabel.Name = "inputLabel";
                        myGrid.Children.Add(inPutLabel);
                    }
                    catch (Exception)
                    {

                    }

                }, null);
            }
            catch (Exception)
            {

            }

        }

        public void FillBoxes(char[] state, char[] tapSequence, char output, char input, string polynomial)
        {
            
            // fill the boxes with current state
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    // get the textboxes as children of myGrid
                    Visual childVisual;

                    for (int i = 0; i < state.Length; i++)
                    {
                        childVisual = (Visual)LogicalTreeHelper.FindLogicalNode(myGrid, "textBoxBit" + i);
                        childVisual.SetValue(TextBox.TextProperty, state[i].ToString());

                        /*
                        // this only seems to work for children not added at runtime
                        Label myInfoText = myGrid.FindName("infoText") as Label;
                        if (myInfoText != null)
                        {
                            myInfoText.Background = Brushes.DodgerBlue;
                        }*/
                    }

                    // update output label
                    childVisual = (Visual)LogicalTreeHelper.FindLogicalNode(myGrid, "outputLabel");
                    childVisual.SetValue(Label.ContentProperty, output);

                    // update input label
                    childVisual = (Visual)LogicalTreeHelper.FindLogicalNode(myGrid, "inputLabel");
                    childVisual.SetValue(Label.ContentProperty, input);

                    // update polynome
                    childVisual = (Visual)VisualTreeHelper.GetChild(polynomialGrid, 0);
                    childVisual.SetValue(Label.ContentProperty, polynomial);                        
                } catch (Exception) { }
            }, null);
        }

        public void DeleteAll(int end)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                // remove all elements
                myGrid.Children.RemoveRange(0, end);
                polynomialText.Visibility = Visibility.Hidden;

                // show initial infoText again
                infoText.Visibility = Visibility.Visible;
            }, null);
        }

        public void ChangeBackground(NotificationLevel logLevel)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                if (logLevel == NotificationLevel.Warning) innercircle.Fill = Brushes.Gold;
                if (logLevel == NotificationLevel.Error) innercircle.Fill = Brushes.Red;
                if (logLevel == NotificationLevel.Info) innercircle.Fill = Brushes.White;
            }, null);
        }

        public Brush ReturnBackgroundColour()
        {
            Brush myBrush = null;
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                if (completeGrid != null)
                    myBrush = innercircle.Fill;
            }, null);

            return myBrush;
        }
    }
}
