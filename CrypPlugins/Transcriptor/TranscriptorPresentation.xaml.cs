/*
   Copyright 2014 Olga Groh

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
using CrypTool.Plugins.Transcriptor;
using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Transcriptor
{
    /// <summary>
    /// Interaktionslogik für TranscriptorPresentation.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("Transcriptor.Properties.Resources")]
    public partial class TranscriptorPresentation : UserControl
    {
        # region Variables

        private readonly CrypTool.Plugins.Transcriptor.Transcriptor transcriptor;
        private string rectangleColor, selectedRectangleColor;
        private int alphabetCount = 0, indexCount = 0, currentRectangeleWidth, currentRectangleHeight;
        private bool mtOn, mouseDown, ctrlBtnPressed = false, firstSymbolOn = false, isBlackImage = false;
        private readonly List<Symbol> symbolList = new List<Symbol>(); //contains all symbols wich will be used for the Text
        private readonly ObservableCollection<Symbol> symbolItems = new ObservableCollection<Symbol>(); // Handels ListboxItems
        private readonly Dictionary<char, int> statsList = new Dictionary<char, int>();
        private readonly List<Symbol> firstSymbols = new List<Symbol>();
        private double xCordinateDown, yCordinateDown, xCordinateUp, yCordinateUp;
        private readonly Rectangle rectangle;
        private BitmapSource croppedBitmap;
        private Int32Rect rcFrom;
        private Rect rect;
        private float threshold;

        #endregion

        public TranscriptorSettings Settings { get; set; }

        public TranscriptorPresentation(CrypTool.Plugins.Transcriptor.Transcriptor transcriptor)
        {
            InitializeComponent();
            this.transcriptor = transcriptor;
            DataContext = this;
            mouseDown = false;

            rectangle = new Rectangle
            {
                Fill = Brushes.Transparent,
            };
        }

        #region Get\Set

        public string SelectedRectangleColor
        {
            get => selectedRectangleColor;
            set => selectedRectangleColor = value;
        }

        public string RectangleColor
        {
            get => rectangleColor;
            set => rectangleColor = value;
        }

        public bool MatchTemplateOn
        {
            get => mtOn;
            set => mtOn = value;
        }

        public float Threshold
        {
            get => threshold;
            set => threshold = value;
        }

        #endregion

        #region Events

        /// <summary>
        /// MouseDown is used for:
        /// 1. When Ctrl-Btn is pushed a Symbol can be deleted
        /// 2. When FirstSymbol on is active a Symbol can be marked as such
        /// 3. Startpoint of the rectangles are saved in xCordinateDown and yCordinateDown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (canvas.Children.Contains(rectangle))
                {
                    canvas.Children.Remove(rectangle);
                }

                // Removes a Symbol Element from Canvas and symbolList
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    ctrlBtnPressed = true;

                    //Contains the Name of the clicked Object
                    string element = (e.OriginalSource as FrameworkElement).Name;

                    if (element.Contains("rectangle"))
                    {
                        // Gets the rectangle's ID
                        int number = Convert.ToInt32(element.Substring(9));

                        for (int i = 0; i < symbolList.Count; i++)
                        {
                            // The statsList's value is subtracted beacause the Symbol is removed
                            if (number == symbolList[i].Id)
                            {
                                int value = statsList[symbolList[i].Letter];
                                statsList[symbolList[i].Letter] = value - 1;

                                //When the value is 0 the Symbol is also removed from the ListBox
                                if (statsList[symbolList[i].Letter] == 0)
                                {
                                    /*Since the Symbol Object which gets earased isn't always
                                     * the one that is in the symbolItems List an if-case is
                                     * needed to find the right one in the List. So it can be removed*/
                                    for (int j = 0; j < symbolItems.Count; j++)
                                    {
                                        if (symbolItems[j].Letter == symbolList[i].Letter)
                                        {
                                            symbolItems.RemoveAt(j);
                                            break;
                                        }
                                    }

                                    //Serialisieren

                                    symbolListbox.Items.Refresh();
                                    statsList.Remove(symbolList[i].Letter);
                                }

                                /*If the rectangle has a different Color, the rectangle
                                 * is a firstSymbol Object so it will be removed from the firstSymbol List, too.*/
                                if (symbolList[i].Rectangle.Stroke != (SolidColorBrush)new BrushConverter().ConvertFromString(SelectedRectangleColor))
                                {
                                    firstSymbols.Remove(symbolList[i]);
                                }

                                //Finally the Symbol will be also removed from the symbolList
                                symbolList.RemoveAt(i);
                                break;
                            }
                        }

                        //and the rectangle in the Canvas will be also removed
                        canvas.Children.Remove(e.OriginalSource as FrameworkElement);

                        if (statsList.Count <= transcriptor.Alphabet.Length)
                        {
                            addSymbolButton.IsEnabled = true;
                        }

                        //we changed our data structure => serialize
                        Serialize();
                    }
                }
                else
                {
                    //The Cordinates will be saved in Variables so the rectangle can be draged
                    if (!firstSymbolOn)
                    {
                        mouseDown = true;
                        ctrlBtnPressed = false;

                        xCordinateDown = e.GetPosition(canvas).X;
                        yCordinateDown = e.GetPosition(canvas).Y;

                        rectangle.Stroke = (SolidColorBrush)new BrushConverter().ConvertFromString(RectangleColor);
                        rectangle.StrokeThickness = 1;

                        Canvas.SetLeft(rectangle, xCordinateDown);
                        Canvas.SetTop(rectangle, yCordinateDown);
                        canvas.Children.Add(rectangle);
                    }
                    else
                    {
                        // If the Object is a rectangle and firstSymbol on
                        FrameworkElement element = (e.OriginalSource as FrameworkElement);

                        if (element.Name.Contains("rectangle"))
                        {
                            int number = Convert.ToInt32(element.Name.Substring(9));

                            for (int k = 0; k < symbolList.Count; k++)
                            {
                                /*If the Id is correct the Color of the rectangle will
                                be changed and the symbol will be added to the firstSymbol List*/
                                if (symbolList[k].Id == number)
                                {
                                    if (symbolList[k].Rectangle.Stroke == (SolidColorBrush)new BrushConverter().ConvertFromString(SelectedRectangleColor))
                                    {
                                        symbolList[k].Rectangle.Stroke = (SolidColorBrush)new BrushConverter().ConvertFromString(RectangleColor);
                                        firstSymbols.Add(symbolList[k]);
                                        break;
                                    }
                                    else
                                    {
                                        //If the User clicks again on a firstSymbol rectangle it will be removed from the List
                                        symbolList[k].Rectangle.Stroke = (SolidColorBrush)new BrushConverter().ConvertFromString(SelectedRectangleColor);
                                        firstSymbols.Remove(symbolList[k]);
                                        break;
                                    }
                                }
                            }
                            //we changed our data structure => serialize
                            Serialize();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                transcriptor.GuiLogMessage(ex);
            }
        }

        /// <summary>
        /// The rectanle can be draged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (mouseDown)
                {
                    rectangle.Width = (int)(Math.Max(e.GetPosition(canvas).X, xCordinateDown) - Math.Min(xCordinateDown, e.GetPosition(canvas).X));
                    rectangle.Height = (int)(Math.Max(e.GetPosition(canvas).Y, yCordinateDown) - Math.Min(yCordinateDown, e.GetPosition(canvas).Y));

                    Canvas.SetLeft(rectangle, Math.Min(xCordinateDown, e.GetPosition(canvas).X));
                    Canvas.SetTop(rectangle, Math.Min(yCordinateDown, e.GetPosition(canvas).Y));
                }
            }
            catch (Exception ex)
            {
                transcriptor.GuiLogMessage(ex);
            }
        }

        /// <summary>
        /// Draws the finished rectangle and crops the Image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (picture.Source != null)
                {
                    if (canvas.Children.Contains(rectangle))
                    {
                        canvas.Children.Remove(rectangle);
                    }

                    xCordinateUp = e.GetPosition(canvas).X;
                    yCordinateUp = e.GetPosition(canvas).Y;

                    if (!ctrlBtnPressed && !firstSymbolOn)
                    {
                        //The Rectangle's Width and the Height is calculated
                        currentRectangeleWidth = (int)(Math.Max(xCordinateUp, xCordinateDown) - Math.Min(xCordinateDown, xCordinateUp));
                        currentRectangleHeight = (int)(Math.Max(yCordinateUp, yCordinateDown) - Math.Min(yCordinateDown, yCordinateUp)); ;

                        rectangle.Width = currentRectangeleWidth;
                        rectangle.Height = currentRectangleHeight;

                        Canvas.SetLeft(rectangle, Math.Min(xCordinateDown, xCordinateUp));
                        Canvas.SetTop(rectangle, Math.Min(yCordinateDown, yCordinateUp));
                        canvas.Children.Add(rectangle);
                        mouseDown = false;

                        //The Rectangle will be used to crop an Image
                        if (rectangle.Width != 0 && rectangle.Height != 0)
                        {
                            rect = new Rect(Canvas.GetLeft(rectangle), Canvas.GetTop(rectangle), rectangle.Width, rectangle.Height);
                            rcFrom = new Int32Rect
                            {
                                X = (int)((rect.X) * ((picture.Source.Width) / (picture.Width))),
                                Y = (int)((rect.Y) * ((picture.Source.Height) / (picture.Height))),
                                Width = (int)((rect.Width) * ((picture.Source.Width) / (picture.Width))),
                                Height = (int)((rect.Height) * ((picture.Source.Height) / (picture.Height)))
                            };

                            /*croppedBitmap gets the rcFrom Objet and the picture. It returns a cut Image
                             * afterwards the Imgae is presented in the GUI*/
                            croppedBitmap = new CroppedBitmap(picture.Source as BitmapSource, rcFrom);
                            croppedImage.Source = croppedBitmap;

                            calculateProbability();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                transcriptor.GuiLogMessage(ex);
            }
        }

        /// <summary>
        /// Adds a Symbol to the symbolList and to the ListBox.
        /// If Mode is set to semi-automatik the MatchTemplate Methode is called
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addSymbolButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //If the all the letters in the used alphabet is in the ListBox the Button is disabled
                if (statsList.Count >= transcriptor.Alphabet.Length)
                {
                    addSymbolButton.IsEnabled = false;
                }
                else
                {
                    alphabetCount = 0;
                    char newLetter = transcriptor.Alphabet[alphabetCount];

                    //Uses the next free Letter
                    while (statsList.ContainsKey(newLetter))
                    {
                        newLetter = transcriptor.Alphabet[alphabetCount++];
                    }

                    //Creates a new Symbol with the SymbolImage and letter afterwards the AddSymbolToList is called
                    Symbol newSymbol = new Symbol(indexCount, newLetter, croppedBitmap);
                    symbolItems.Add(newSymbol);
                    symbolListbox.ItemsSource = symbolItems;
                    indexCount++;

                    /*If MatchTemplateOn is set to true the AddSymbolToList Method is not necasary here.
                     * MatchTemplate mode cannot check if a symbol is already in the List*/
                    if (MatchTemplateOn)
                    {
                        MatchSymbol(newSymbol, currentRectangeleWidth, currentRectangleHeight,
                        (int)Math.Min(xCordinateDown, xCordinateUp), (int)Math.Min(yCordinateDown, yCordinateUp));
                    }
                    else
                    {
                        AddSymbolToList(newSymbol, currentRectangeleWidth, currentRectangleHeight,
                        Math.Min(xCordinateDown, xCordinateUp), Math.Min(yCordinateDown, yCordinateUp));
                    }
                    //we changed our data structure => serialize
                    Serialize();
                }
            }
            catch (Exception ex)
            {
                transcriptor.GuiLogMessage(ex);
            }
        }

        /// <summary>
        /// When the user double clicks in an item a Symbol will be created an added to symbolList.
        /// When the user also press the Ctrl-Button all Symbols like the item will be deleted in all lists
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //item contains the ListBox Item
                ListBoxItem item = sender as ListBoxItem;

                if (item != null || item.IsSelected)
                {
                    //All Symbols like the item object will be removed
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        for (int i = 0; i < symbolList.Count; i++)
                        {
                            if (symbolList[i].Letter.Equals(item.Content.ToString()[0]))
                            {
                                canvas.Children.Remove(symbolList[i].Rectangle);
                                symbolList.RemoveAt(i);
                                i--;
                            }
                        }

                        //Removes the letter in statsList
                        statsList.Remove(item.Content.ToString()[0]);

                        //Also in firstSymbols
                        for (int k = 0; k < firstSymbols.Count; k++)
                        {
                            if (firstSymbols[k].Letter.Equals(item.Content.ToString()[0]))
                            {
                                firstSymbols.RemoveAt(k);
                                k--;
                            }
                        }

                        //Finds the Symbol in symbolItems and removes it
                        for (int j = 0; j < symbolItems.Count; j++)
                        {
                            if (symbolItems[j].Letter.Equals(item.Content.ToString()[0]))
                            {
                                symbolItems.RemoveAt(j);
                                break;
                            }
                        }

                        if (statsList.Count <= transcriptor.Alphabet.Length)
                        {
                            addSymbolButton.IsEnabled = true;
                        }
                    }
                    //A new Symbol gets created and added in symbolList 
                    else
                    {
                        //From the item the letter can be extracted and added to the symbolList and canvas
                        Symbol symbol = new Symbol(indexCount, item.Content.ToString()[0], croppedBitmap);
                        indexCount++;
                        AddSymbolToList(symbol, currentRectangeleWidth, currentRectangleHeight, Math.Min(xCordinateDown, xCordinateUp),
                            Math.Min(yCordinateDown, yCordinateUp));
                    }
                    //we changed our data structure => serialize
                    Serialize();
                }
            }
            catch (Exception ex)
            {
                transcriptor.GuiLogMessage(ex);
            }
        }

        /// <summary>
        /// Sets the button either firstSymbol on or off
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TransformButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (firstSymbolOn == false)
                {

                    TransformButton.Content = Transcriptor.Properties.Resources.FirstSymbolOn;
                    firstSymbolOn = true;
                }
                else
                {
                    TransformButton.Content = Transcriptor.Properties.Resources.FirstSymbolOff;
                    firstSymbolOn = false;
                }
            }
            catch (Exception ex)
            {
                transcriptor.GuiLogMessage(ex);
            }
        }

        /// <summary>
        /// The user can add spaces in the Text
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void spaceButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Creates a spaceSymbol Objects and add it in the symbolList
                Symbol spaceSymbol = new Symbol(indexCount, ' ', croppedBitmap);
                indexCount++;

                AddSymbolToList(spaceSymbol, currentRectangeleWidth, currentRectangleHeight,
                            Math.Min(xCordinateDown, xCordinateUp), Math.Min(yCordinateDown, yCordinateUp));
                //we changed our data structure => serialize
                Serialize();
            }
            catch (Exception ex)
            {
                transcriptor.GuiLogMessage(ex);
            }
        }

        /// <summary>
        /// If mode is set to semi-Automatik the symbolList needs to sorted
        /// first after that the list will be saved in a String and handed over to the output Plugin
        /// If mode is set to manually the sorting has been done by the user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void generateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StringBuilder textBuilder = new StringBuilder();
                double upperBound = 0, lowerBound = 0;

                if (MatchTemplateOn)
                {
                    //Fist symbolList will be first sorted after the X Cordinate
                    symbolList.Sort(new SymbolComparer(true));
                    /*firstSymbols will be sorted after the y Cordinate so its
                     * not necasary to click on the lines in the Text by order*/
                    firstSymbols.Sort(new SymbolComparer(false));

                    for (int i = 0; i < firstSymbols.Count; i++)
                    {
                        upperBound = firstSymbols[i].Y + (firstSymbols[i].Rectangle.Height / 2);
                        lowerBound = firstSymbols[i].Y - (firstSymbols[i].Rectangle.Height / 2);

                        /*The symbolList objects which are in the upper and lower Bound will be append to the Text
                         * since the sortig of the symbolList is already done the Text will be presented properly*/
                        for (int j = 0; j < symbolList.Count; j++)
                        {
                            if (symbolList[j].Y > lowerBound && symbolList[j].Y < upperBound)
                            {
                                textBuilder.Append(symbolList[j]);
                            }
                        }
                    }
                }
                else
                {
                    /*If the mode is set to manually the sorting needs to done by the user and
                     * the symbolList Letters will be append to the Text*/
                    for (int i = 0; i < symbolList.Count; i++)
                    {
                        textBuilder.Append(symbolList[i].Letter);
                    }
                }

                transcriptor.GenerateText(textBuilder.ToString());
            }
            catch (Exception ex)
            {
                transcriptor.GuiLogMessage(ex);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Draws a new Rectangle, custom ToolTip and adds newSymbol to the SymbolList
        /// </summary>
        /// <param name="newSymbol"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void AddSymbolToList(Symbol newSymbol, int width, int height, double x, double y)
        {
            //Adds a fixed rectangle to the canvas
            Rectangle newRectangle = new Rectangle
            {
                Fill = Brushes.Transparent,
                Stroke = (SolidColorBrush)new BrushConverter().ConvertFromString(SelectedRectangleColor),
                StrokeThickness = 1,
                Width = width,
                Height = height,
                Name = "rectangle" + newSymbol.Id,
            };

            //Adds the right with an image
            newRectangle.ToolTip = AddToolTip(newSymbol.Letter);

            //places the rectangle on the canvas
            Canvas.SetLeft(newRectangle, x);
            Canvas.SetTop(newRectangle, y);
            canvas.Children.Add(newRectangle);

            //the rectangle is bound to a Symbol
            newSymbol.Rectangle = newRectangle;
            newSymbol.X = x;
            newSymbol.Y = y;

            symbolList.Add(newSymbol);

            if (statsList.ContainsKey(newSymbol.Letter))
            {
                int value = statsList[newSymbol.Letter];
                statsList[newSymbol.Letter] = value + 1;
            }
            else
            {
                statsList.Add(newSymbol.Letter, 1);
            }
        }

        /// <summary>
        /// Calculates for all Symbols in the itemList how similar they are to the croppedBitmap
        /// </summary>
        private void calculateProbability()
        {
            Image<Gray, byte> rectangleImage = new Image<Gray, byte>(ToBitmap(croppedBitmap));

            if (symbolItems.Count > 0)
            {
                //Inverts the Image if it has black background
                if (isBlackImage)
                {
                    rectangleImage = rectangleImage.Not();
                }

                //Both images must have the same size so they both get set to 50x50 pixel
                rectangleImage = rectangleImage.Resize(50, 50, Emgu.CV.CvEnum.Inter.Linear);

                for (int i = 0; i < symbolItems.Count; i++)
                {
                    Image<Gray, byte> itemImage = new Image<Gray, byte>(ToBitmap(symbolItems[i].Image));

                    if (isBlackImage)
                    {
                        itemImage = itemImage.Not();
                    }

                    itemImage = itemImage.Resize(50, 50, Emgu.CV.CvEnum.Inter.Linear);

                    //Calculates the absolute differance of both images so both images will overlap
                    Image<Gray, byte> absImage = rectangleImage.AbsDiff(itemImage);

                    /*afterwards the cropImage is added to the absImage so only the pixels are visible
                     * wich are in the symbol from the image*/
                    Image<Gray, byte> diffImage = absImage + rectangleImage;

                    int diffPixelCounter = 0, itemPixelCounter = 0;

                    for (int x = 0; x < diffImage.Width; x++)
                    {
                        for (int y = 0; y < diffImage.Height; y++)
                        {
                            //Saves the color from the pixel of diffImage and itemImage
                            System.Drawing.Color diffPixelColor = diffImage.Bitmap.GetPixel(x, y);
                            System.Drawing.Color itemPixelColor = itemImage.Bitmap.GetPixel(x, y);

                            //Counts only the pixes without the background
                            if (diffPixelColor.R <= 127)
                            {
                                diffPixelCounter++;
                            }

                            //Counts the pixels from the item symbol
                            if (itemPixelColor.R <= 127)
                            {
                                itemPixelCounter++;
                            }
                        }
                    }

                    //Afterwards the diffpixels are divided with the itempixel to get are procentual value
                    if (itemPixelCounter == 0)
                    {
                        symbolItems[i].Probability = 0;
                    }
                    else
                    {
                        symbolItems[i].Probability = Math.Round((diffPixelCounter / (double)itemPixelCounter) * 100d, 2);
                    }
                }

                //Handels the sorting
                symbolListbox.Items.SortDescriptions.Clear();
                SortDescription sortDescription = new SortDescription("Probability", ListSortDirection.Descending);
                symbolListbox.Items.SortDescriptions.Add(sortDescription);
                symbolListbox.SelectedIndex = 0;
                symbolListbox.ScrollIntoView(symbolListbox.Items[0]);
            }
            else
            {
                //If the image has black background the picture has to be inverted first
                if (rectangleImage.Bitmap.GetPixel(0, 0).R == 0)
                {
                    isBlackImage = true;
                }
            }
        }

        /// <summary>
        /// Finds equal symbols and adds them to the symbolList
        /// </summary>
        /// <param name="newSymbol"></param>
        private void MatchSymbol(Symbol newSymbol, int width, int height, int symbolX, int symbolY)
        {
            //Since Emgu CV is not compatible to WPF the pictures have to be changed to System.Drawing objects 
            System.Drawing.Rectangle cropRect = new System.Drawing.Rectangle(symbolX, symbolY, width, height);
            Image<Gray, byte> sourceImage = new Image<Gray, byte>(ToBitmap(picture.Source as BitmapSource));

            //The Images have to be resized so they have the same Width and height like the picture.Source otherwise the coordinates are wrong
            sourceImage = sourceImage.Resize((int)picture.Width, (int)picture.Height, Emgu.CV.CvEnum.Inter.Linear);
            System.Drawing.Bitmap symbolBitmap = new System.Drawing.Bitmap(sourceImage.Bitmap);
            symbolBitmap = symbolBitmap.Clone(cropRect, System.Drawing.Imaging.PixelFormat.DontCare);

            //The templateImage is the Image with the Symbol
            Image<Gray, byte> templateImage = new Image<Gray, byte>(symbolBitmap);
            Image<Gray, float> resultImage;

            //The template method
            resultImage = sourceImage.MatchTemplate(templateImage, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed);

            float[,,] matches = resultImage.Data;
            bool skip = false;

            for (int y = 0; y < matches.GetLength(0); y++)
            {
                for (int x = 0; x < matches.GetLength(1); x++)
                {
                    /*The matchScore will be calculated for x and y and if its bigger then the choosen Threshold
                     * a Symbol Object will be created*/
                    double matchScore = matches[y, x, 0];

                    if (matchScore > Threshold)
                    {
                        Symbol equalSymbol = new Symbol(indexCount, newSymbol.Letter, newSymbol.Image);
                        indexCount++;

                        AddSymbolToList(equalSymbol, currentRectangeleWidth, currentRectangleHeight, x, y);

                        //a Skip is necasary beacause otherwise MatchTemplate finds Symbol objects several times
                        x += currentRectangeleWidth;
                        skip = true;
                    }
                }

                //For the y coordinate a skip is necasary, too
                if (skip)
                {
                    y += currentRectangleHeight;
                    skip = false;
                }
            }
        }

        /// <summary>
        /// Converts a BitmapSource to a System.Drawing.Bitmap Object
        /// </summary>
        /// <param name="bitmapSource"></param>
        /// <returns>System.Drawing.Bitmap Object bitmap</returns>
        private System.Drawing.Bitmap ToBitmap(BitmapSource bitmapSource)
        {
            System.Drawing.Bitmap bitmap;
            using (MemoryStream outStream = new MemoryStream())
            {
                // from System.Media.BitmapImage to System.Drawing.Bitmap
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapSource));
                enc.Save(outStream);
                bitmap = new System.Drawing.Bitmap(outStream);

                return bitmap;
            }
        }

        /// <summary>
        /// Adds the right toolTip in a rectangle
        /// </summary>
        /// <param name="letter"></param>
        /// <returns></returns>
        private object AddToolTip(char letter)
        {
            if (letter.Equals(' '))
            {
                return Transcriptor.Properties.Resources.Space;

            }
            else
            {
                /*searches the symbolItems Object with the same Letter as newSymbol.Letter
                 * *this way the Image from the ListBox can be presented in the ToolTip, too*/
                int index = 0;

                for (int i = 0; i < symbolItems.Count; i++)
                {
                    if (letter == symbolItems[i].Letter)
                    {
                        index = i;
                        break;
                    }
                }

                //ToolTip Layout configurations
                StackPanel stack = new StackPanel
                {
                    Orientation = Orientation.Vertical
                };
                stack.Children.Add(new TextBlock
                {
                    Text = letter.ToString(),
                    FontSize = 15,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 0),
                    FontWeight = FontWeights.Bold
                });

                stack.Children.Add(new Image
                {
                    Source = symbolItems[index].Image,
                    Stretch = Stretch.Fill,
                    Width = 40,
                    Height = 40,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    HorizontalAlignment = HorizontalAlignment.Left
                });

                return stack;
            }

        }

        /// <summary>
        /// Deserializes data structure of the transcriptor from a hidden field of the settings
        /// </summary>
        public void Deserialize()
        {
            canvas.Children.Clear();
            indexCount = 0;
            alphabetCount = 0;
            symbolList.Clear();
            symbolItems.Clear();
            firstSymbols.Clear();
            if (string.IsNullOrEmpty(Settings.SerializedData))
            {
                return;
            }
            MemoryStream stream = new MemoryStream(Convert.FromBase64String(Settings.SerializedData));
            BinaryReader reader = new BinaryReader(stream);
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int size = reader.ReadInt32();
                Symbol symbol = Symbol.Deserialize(reader.ReadBytes(size));
                symbol.Probability = 0;
                symbolItems.Add(symbol);
            }
            symbolListbox.ItemsSource = symbolItems;
            count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int size = reader.ReadInt32();
                Symbol symbol = Symbol.Deserialize(reader.ReadBytes(size));
                symbol.Probability = 0;
                //set picture reference to each symbol
                foreach (Symbol s in symbolItems.Where(s => s.Id == symbol.Id))
                {
                    symbol.Id = s.Id;
                    break;
                }
                AddSymbolToList(symbol, (int)symbol.Rectangle.Width, (int)symbol.Rectangle.Height, symbol.X, symbol.Y);
                indexCount++;
            }
            count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int size = reader.ReadInt32();
                Symbol symbol = Symbol.Deserialize(reader.ReadBytes(size));
                symbol.Probability = 0;
                firstSymbols.Add(symbol);
                //set picture reference to each symbol
                foreach (Symbol s in firstSymbols.Where(s => s.Id == symbol.Id))
                {
                    symbol.Id = s.Id;
                    break;
                }
                //mark first symbols
                foreach (Symbol s in symbolList)
                {
                    if (s.Id == symbol.Id)
                    {
                        s.Rectangle.Stroke = (SolidColorBrush)new BrushConverter().ConvertFromString(RectangleColor);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Serializes the data scructure of the transcriptor to a hidden field of the settings
        /// </summary>
        public void Serialize()
        {
            MemoryStream stream = new MemoryStream();
            stream.Write(BitConverter.GetBytes(symbolItems.Count), 0, 4);
            foreach (Symbol s in symbolItems)
            {
                byte[] data = Symbol.Serialize(s);
                stream.Write(BitConverter.GetBytes(data.Length), 0, 4);
                stream.Write(data, 0, data.Length);
            }
            stream.Write(BitConverter.GetBytes(symbolList.Count), 0, 4);
            foreach (Symbol s in symbolList)
            {
                byte[] data = Symbol.Serialize(s, false);
                stream.Write(BitConverter.GetBytes(data.Length), 0, 4);
                stream.Write(data, 0, data.Length);
            }
            stream.Write(BitConverter.GetBytes(firstSymbols.Count), 0, 4);
            foreach (Symbol s in firstSymbols)
            {
                byte[] data = Symbol.Serialize(s, false);
                stream.Write(BitConverter.GetBytes(data.Length), 0, 4);
                stream.Write(data, 0, data.Length);
            }
            Settings.SerializedData = Convert.ToBase64String(stream.GetBuffer());
        }
        #endregion
    }
}
