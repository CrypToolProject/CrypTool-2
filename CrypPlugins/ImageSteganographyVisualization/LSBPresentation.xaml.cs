using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Color = System.Drawing.Color;

namespace ImageSteganographyVisualization
{

    [CrypTool.PluginBase.Attributes.Localization("ImageSteganographyVisualization.Properties.Resources")]
    public partial class LSBPresentation : UserControl
    {
        private int introViewCounter;
        private readonly ImageSteganographyVisualization imageStegVis;
        private int currentX = 0;
        private int currentY = 0;
        private int height;
        private int width;
        private int totalMessageBits;

        public LSBPresentation(ImageSteganographyVisualization imageStegVis)
        {
            introViewCounter = 0;
            this.imageStegVis = imageStegVis;
            InitializeComponent();
            InitBitMasksButtons();
            ChooseBitsHint.Visibility = Visibility.Hidden;
            ConversionExampleImage.Source = new BitmapImage(new Uri("images/example_conversion.PNG", UriKind.Relative));
            ModelImage.Source = new BitmapImage(new Uri(Properties.Resources.StegModelUrl, UriKind.Relative));
            GoToPixelConversion.IsEnabled = false;
        }

        #region Main menu methods
        /// <summary>
        /// Enables choose bits button after executing the model
        /// </summary>
        public void EnableButtons()
        {
            SeeChooseBitsButton.IsEnabled = true;
            StartHint.Visibility = Visibility.Hidden;
            ChooseBitsHint.Visibility = Visibility.Visible;
            SeeChooseBitsButton.Background = System.Windows.Media.Brushes.LightGreen;
            width = imageStegVis.inputBitmap.Width;
            height = imageStegVis.inputBitmap.Height;
            BitArray messageBits = new BitArray(Encoding.UTF8.GetBytes(imageStegVis.InputSecretMessage));
            totalMessageBits = messageBits.Length;
            CoverImage.Source = BitmapConversion.BitmapToBitmapSource(imageStegVis.inputBitmap);
        }

        /// <summary>
        /// Enables all the buttons of the views in the main menu after choosing hiding bits
        /// </summary>
        public void EnableAllButtons()
        {
            SeeChooseBitsButton.IsEnabled = true;
            SeePixelConversionButton.IsEnabled = true;
            SeeHidingAndCapacityButton.IsEnabled = true;
            SeeChooseBitsButton.Background = System.Windows.Media.Brushes.LightGray;
            StartHint.Visibility = Visibility.Hidden;
            ChooseBitsHint.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Disables the buttons in the main menu after clicking stop
        /// </summary>
        public void DisableButtons()
        {
            SeeChooseBitsButton.IsEnabled = false;
            SeePixelConversionButton.IsEnabled = false;
            SeeHidingAndCapacityButton.IsEnabled = false;
            StartHint.Visibility = Visibility.Visible;
            ChooseBitsHint.Visibility = Visibility.Hidden;
        }

        private void BackToMainMenuClick(object sender, RoutedEventArgs e)
        {
            ShowMainMenu();
        }

        public void ShowMainMenu()
        {
            ChooseBitsView.Visibility = Visibility.Hidden;
            PixelConversionView.Visibility = Visibility.Hidden;
            IntroView.Visibility = Visibility.Hidden;
            HidingAndCapacityView.Visibility = Visibility.Hidden;
            MainMenu.Visibility = Visibility.Visible;
        }

        #endregion

        #region Intro view methods

        private void ShowIntroViewClick(object sender, RoutedEventArgs e)
        {
            MainMenu.Visibility = Visibility.Hidden;
            IntroView.Visibility = Visibility.Visible;
            introViewCounter = 0;
            Intro0.Visibility = Visibility.Visible;
            Intro1.Visibility = Visibility.Hidden;
            Intro2.Visibility = Visibility.Hidden;
            Intro3.Visibility = Visibility.Hidden;
            PrevIntro.IsEnabled = false;
            NextIntro.IsEnabled = true;
        }

        private void NextIntroButtonClick(object sender, RoutedEventArgs e)
        {
            introViewCounter++;
            switch (introViewCounter)
            {
                case 1:
                    Intro0.Visibility = Visibility.Hidden;
                    Intro1.Visibility = Visibility.Visible;
                    PrevIntro.IsEnabled = true;
                    break;
                case 2:
                    Intro1.Visibility = Visibility.Hidden;
                    Intro2.Visibility = Visibility.Visible;
                    break;
                case 3:
                    Intro2.Visibility = Visibility.Hidden;
                    Intro3.Visibility = Visibility.Visible;
                    NextIntro.IsEnabled = false;
                    break;
            }
        }

        private void PrevIntroButtonClick(object sender, RoutedEventArgs e)
        {
            introViewCounter--;
            NextIntro.IsEnabled = true;
            switch (introViewCounter)
            {
                case 0:
                    Intro1.Visibility = Visibility.Hidden;
                    Intro0.Visibility = Visibility.Visible;
                    PrevIntro.IsEnabled = false;
                    break;
                case 1:
                    Intro2.Visibility = Visibility.Hidden;
                    Intro1.Visibility = Visibility.Visible;
                    break;
                case 2:
                    Intro3.Visibility = Visibility.Hidden;
                    Intro2.Visibility = Visibility.Visible;
                    break;
            }
        }

        #endregion

        #region Choose bits view methods

        private void ShowChooseBitsViewClick(object sender, RoutedEventArgs e)
        {
            IntroView.Visibility = Visibility.Hidden;
            PixelConversionView.Visibility = Visibility.Hidden;
            ChooseBitsView.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Event listener to when a bit button is clicked 
        /// </summary>
        private void BitClick(object sender, RoutedEventArgs e)
        {
            Button bitButton = (Button)sender;
            string bitName = bitButton.Name;
            if (bitButton.Content.ToString().Equals("1"))
            {
                bitButton.Background = new SolidColorBrush(Colors.White);
                bitButton.Content = "0";
            }
            else
            {
                bitButton.Background = new SolidColorBrush(Colors.Cyan);
                bitButton.Content = "1";
            }
            SwitchBit(bitName);
        }

        /// <summary>
        /// Apply the changes in the bitmasks in response to clicking a bit button
        /// </summary>
        private void SwitchBit(string bitName)
        {
            char colorChannel = bitName[0];
            int index = (int)char.GetNumericValue(bitName[1]);
            if (colorChannel == 'R')
            {
                imageStegVis.rBitMask[index] = !imageStegVis.rBitMask[index];
            }
            else if (colorChannel == 'G')
            {
                imageStegVis.gBitMask[index] = !imageStegVis.gBitMask[index];
            }
            else
            {
                imageStegVis.bBitMask[index] = !imageStegVis.bBitMask[index];
            }
        }

        private void ApplyChangesClick(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                imageStegVis.HideDataLSB();
            }, null);
            EnableAllButtons();
            GoToPixelConversion.IsEnabled = true;
        }

        #endregion

        #region Pixel conversion view methods

        private void PixelConversionViewClick(object sender, RoutedEventArgs e)
        {
            ChooseBitsView.Visibility = Visibility.Hidden;
            PixelConversionView.Visibility = Visibility.Visible;
            SetPixelConversionView();
        }

        public void SetPixelConversionView()
        {
            IntroView.Visibility = Visibility.Hidden;
            ChooseBitsView.Visibility = Visibility.Hidden;
            PixelConversionView.Visibility = Visibility.Visible;
            Color pixelBefore = imageStegVis.inputBitmap.GetPixel(currentX, currentY);
            Color pixelAfter = imageStegVis.outputBitmap.GetPixel(currentX, currentY);
            SolidColorBrush scbBefore = new SolidColorBrush();
            SolidColorBrush scbAfter = new SolidColorBrush();
            scbBefore.Color = System.Windows.Media.Color.FromRgb(pixelBefore.R, pixelBefore.G, pixelBefore.B);
            scbAfter.Color = System.Windows.Media.Color.FromRgb(pixelAfter.R, pixelAfter.G, pixelAfter.B);
            PixelBeforeSample.Fill = scbBefore;
            PixelAfterSample.Fill = scbAfter;
            SetRValueBitsString(RValueBitsBefore, pixelBefore);
            SetGValueBitsString(GValueBitsBefore, pixelBefore);
            SetBValueBitsString(BValueBitsBefore, pixelBefore);
            SetRValueBitsString(RValueBitsAfter, pixelAfter);
            SetGValueBitsString(GValueBitsAfter, pixelAfter);
            SetBValueBitsString(BValueBitsAfter, pixelAfter);
            CurrentPixelText.Text = string.Format(Properties.Resources.CurrentPixelLabel + " ( x , y ) : ( {0} , {1} )", currentX, currentY);
            // enable and disable arrow buttons when necessary
            if ((currentX == imageStegVis.inputBitmap.Width - 1) && (currentY == imageStegVis.inputBitmap.Height - 1))
            {
                NextPixelButton.IsEnabled = false;
            }
            else
            {
                NextPixelButton.IsEnabled = true;
            }
            if ((currentX == 0) && (currentY == 0))
            {
                PrevPixelButton.IsEnabled = false;
            }
            else
            {
                PrevPixelButton.IsEnabled = true;
            }
            if (HeaderPixel())
            {
                HeaderPixelTB.Visibility = Visibility.Visible;
            }
            else
            {
                HeaderPixelTB.Visibility = Visibility.Hidden;
            }
        }

        private void SetRValueBitsString(TextBlock tb, Color pixel)
        {
            tb.Text = "";
            BitArray bitsArray = new BitArray(new byte[] { pixel.R });
            if (HeaderPixel())
            {
                for (int i = 7; i >= 0; i--)
                {
                    if (bitsArray[i])
                    {
                        tb.Inlines.Add(new Run("1") { Background = System.Windows.Media.Brushes.Yellow });
                    }
                    else
                    {
                        tb.Inlines.Add(new Run("0") { Background = System.Windows.Media.Brushes.Yellow });
                    }
                }
            }
            else
            {
                for (int i = 7; i >= 0; i--)
                {
                    if (bitsArray[i])
                    {
                        if (imageStegVis.rBitMask[i])
                        {
                            tb.Inlines.Add(new Run("1") { Background = System.Windows.Media.Brushes.Yellow });
                        }
                        else { tb.Inlines.Add("1"); }
                    }
                    else
                    {
                        if (imageStegVis.rBitMask[i])
                        {
                            tb.Inlines.Add(new Run("0") { Background = System.Windows.Media.Brushes.Yellow });
                        }
                        else { tb.Inlines.Add("0"); }
                    }
                }
            }
        }

        private void SetGValueBitsString(TextBlock tb, Color pixel)
        {
            tb.Text = "";
            BitArray bitsArray = new BitArray(new byte[] { pixel.G });
            if (HeaderPixel())
            {
                for (int i = 7; i >= 0; i--)
                {
                    if (bitsArray[i])
                    {
                        tb.Inlines.Add(new Run("1") { Background = System.Windows.Media.Brushes.Yellow });
                    }
                    else
                    {
                        tb.Inlines.Add(new Run("0") { Background = System.Windows.Media.Brushes.Yellow });
                    }
                }
            }
            else
            {
                for (int i = 7; i >= 0; i--)
                {
                    if (bitsArray[i])
                    {
                        if (imageStegVis.gBitMask[i])
                        {
                            tb.Inlines.Add(new Run("1") { Background = System.Windows.Media.Brushes.Yellow });
                        }
                        else { tb.Inlines.Add("1"); }
                    }
                    else
                    {
                        if (imageStegVis.gBitMask[i])
                        {
                            tb.Inlines.Add(new Run("0") { Background = System.Windows.Media.Brushes.Yellow });
                        }
                        else { tb.Inlines.Add("0"); }
                    }
                }
            }
        }

        private void SetBValueBitsString(TextBlock tb, Color pixel)
        {
            tb.Text = "";
            BitArray bitsArray = new BitArray(new byte[] { pixel.B });
            if (HeaderPixel())
            {
                for (int i = 7; i >= 0; i--)
                {
                    if (bitsArray[i])
                    {
                        tb.Inlines.Add(new Run("1") { Background = System.Windows.Media.Brushes.Yellow });
                    }
                    else
                    {
                        tb.Inlines.Add(new Run("0") { Background = System.Windows.Media.Brushes.Yellow });
                    }
                }
            }
            else
            {
                for (int i = 7; i >= 0; i--)
                {
                    if (bitsArray[i])
                    {
                        if (imageStegVis.bBitMask[i])
                        {
                            tb.Inlines.Add(new Run("1") { Background = System.Windows.Media.Brushes.Yellow });
                        }
                        else { tb.Inlines.Add("1"); }
                    }
                    else
                    {
                        if (imageStegVis.bBitMask[i])
                        {
                            tb.Inlines.Add(new Run("0") { Background = System.Windows.Media.Brushes.Yellow });
                        }
                        else { tb.Inlines.Add("0"); }
                    }
                }
            }

        }

        private bool HeaderPixel()
        {
            int lastX = width - 1;
            int lastY = height - 1;
            return ((currentY == lastY) && ((currentX == lastX) || (currentX == lastX - 1) || (currentX == lastX - 2)));
        }

        private void NextPixelClick(object sender, RoutedEventArgs e)
        {
            PrevPixelButton.IsEnabled = true;
            if ((currentX == imageStegVis.inputBitmap.Width - 1) && (currentY == imageStegVis.inputBitmap.Height - 1))
            {
                NextPixelButton.IsEnabled = false;
            }
            if (currentX == imageStegVis.inputBitmap.Width - 1 && currentY < imageStegVis.inputBitmap.Height - 1)
            {
                currentY++;
                currentX = 0;
            }
            else
            {
                currentX++;
            }
            SetPixelConversionView();

            PixelX.Text = Properties.Resources.XCoTextArea;
            PixelY.Text = Properties.Resources.YCoTextArea;
            PixelX.GotFocus += TextBoxClicked;
            PixelY.GotFocus += TextBoxClicked;
            InvalidXYMessage.Visibility = Visibility.Hidden;

        }

        private void PrevPixelClick(object sender, RoutedEventArgs e)
        {
            NextPixelButton.IsEnabled = true;

            if ((currentX == 0) && (currentY == 0))
            {
                PrevPixelButton.IsEnabled = false;
            }
            if (currentX == 0 && currentY > 0)
            {
                currentY--;
                currentX = imageStegVis.inputBitmap.Width - 1;
            }
            else
            {
                currentX--;

            }
            SetPixelConversionView();
            PixelX.Text = Properties.Resources.XCoTextArea;
            PixelY.Text = Properties.Resources.YCoTextArea;
            PixelX.GotFocus += TextBoxClicked;
            PixelY.GotFocus += TextBoxClicked;
            InvalidXYMessage.Visibility = Visibility.Hidden;

        }

        private void ManualCoordinatesEnteredClick(object sender, RoutedEventArgs e)
        {
            try
            {
                int x = int.Parse(PixelX.Text);
                int y = int.Parse(PixelY.Text);
                // display warning if the input is not valid
                if ((x < 0) || (x >= imageStegVis.inputBitmap.Width) || (y < 0) || (y >= imageStegVis.inputBitmap.Height))
                {
                    InvalidXYMessage.Visibility = Visibility.Visible;
                    ValidXY.Text = string.Format("0 <= x < {0} and 0 <= y < {1}", imageStegVis.inputBitmap.Width, imageStegVis.inputBitmap.Height);
                }
                else
                {
                    InvalidXYMessage.Visibility = Visibility.Hidden;
                    currentX = x;
                    currentY = y;
                    SetPixelConversionView();
                }
            }
            catch (System.FormatException)
            {
                // display warning in case something other than a number was not entered
                InvalidXYMessage.Visibility = Visibility.Visible;
                ValidXY.Text = string.Format("0 <= x < {0} and 0 <= y < {1}", imageStegVis.inputBitmap.Width, imageStegVis.inputBitmap.Height);
            }
        }

        private void TextBoxClicked(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.Text = string.Empty;
            tb.FontStyle = FontStyles.Normal;
            tb.GotFocus -= TextBoxClicked;
        }

        #endregion

        #region Hiding capacity view methods

        private void ShowHidingAndCapacityInfoViewClick(object sender, RoutedEventArgs e)
        {
            MainMenu.Visibility = Visibility.Hidden;
            HidingAndCapacityView.Visibility = Visibility.Visible;
            SetHidingAndCapacityView();
        }

        private void SetHidingAndCapacityView()
        {
            WidthTB.Text = Properties.Resources.ImageWidthLabel + width;
            HeightTB.Text = Properties.Resources.ImageHeightLabel + height;
            BitsChosenTB.Text = string.Format(Properties.Resources.BitsChosenLabel + " {0}", GetNumberOfChosenBits().ToString());
            HidingCapacityTB.Text = string.Format(Properties.Resources.HidingCapacityText + " {0:0.###} ", (GetNumberOfChosenBits() * width * height));
            HidingCapacityCB.SelectedIndex = 0;
            MessageLengthCB.SelectedIndex = 0;
            double percentageCapacity = (GetNumberOfChosenBits() / 24.0) * 100.0;
            PercentageCapacityLabel.Text = string.Format(Properties.Resources.PercentageCapacityLabel + " {0:0.##} %", percentageCapacity);
        }

        /// <summary>
        /// Initializes the combobox with the appropriate units
        /// </summary>
        private void CBLoaded(object sender, RoutedEventArgs e)
        {
            List<string> units = new List<string>
            {
                "bit",
                "byte",
                "kilobit",
                "megabit"
            };

            ComboBox comboBox = sender as ComboBox;
            comboBox.ItemsSource = units;
            comboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// Converts the size displayed based on the unit selected from the combobox
        /// </summary>
        private void CBUnitChanged(object sender, RoutedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            string value = comboBox.SelectedItem as string;
            double sizeConverted;
            if (comboBox.Name == "HidingCapacityCB")
            {
                sizeConverted = GetNumberOfChosenBits() * width * height;
            }
            else
            {
                sizeConverted = totalMessageBits;
            }
            switch (value)
            {
                case "bit":
                    break;
                case "byte":
                    sizeConverted /= 8;
                    break;
                case "kilobit":
                    sizeConverted /= 1000;
                    break;
                case "megabit":
                    sizeConverted /= 1000000;
                    break;
            }
            if (comboBox.Name == "HidingCapacityCB")
            {
                HidingCapacityTB.Text = string.Format(Properties.Resources.HidingCapacityText + " {0:0.###} ", sizeConverted);
            }
            else
            {
                MessageLengthTB.Text = string.Format(Properties.Resources.MessageLengthLabel + " {0:0.###} ", sizeConverted);
            }
        }

        /// <summary>
        /// Get total number of chosen bits
        /// </summary>
        private int GetNumberOfChosenBits()
        {
            int total = 0;
            for (int i = 0; i < 8; i++)
            {
                if (imageStegVis.rBitMask[i])
                {
                    total++;
                }

                if (imageStegVis.gBitMask[i])
                {
                    total++;
                }

                if (imageStegVis.bBitMask[i])
                {
                    total++;
                }
            }
            return total;
        }

        #endregion

        #region Bitmasks methods
        private void InitButton(Button bitButton)
        {
            char colorChannel = bitButton.Name[0];
            int index = (int)char.GetNumericValue(bitButton.Name[1]);
            bool bitUsed = false;

            if (colorChannel == 'R')
            {
                bitUsed = imageStegVis.rBitMask[index];
            }
            else if (colorChannel == 'G')
            {
                bitUsed = imageStegVis.gBitMask[index];
            }
            else
            {
                bitUsed = imageStegVis.bBitMask[index];
            }

            if (bitUsed)
            {
                bitButton.Content = "1";
                bitButton.Background = System.Windows.Media.Brushes.Cyan;
            }
            else
            {
                bitButton.Content = "0";
                bitButton.Background = System.Windows.Media.Brushes.White;
            }
        }

        private void InitBitMasksButtons()
        {
            InitButton(R0);
            InitButton(R1);
            InitButton(R2);
            InitButton(R3);
            InitButton(R4);
            InitButton(R5);
            InitButton(R6);
            InitButton(R7);

            InitButton(G0);
            InitButton(G1);
            InitButton(G2);
            InitButton(G3);
            InitButton(G4);
            InitButton(G5);
            InitButton(G6);
            InitButton(G7);

            InitButton(B0);
            InitButton(B1);
            InitButton(B2);
            InitButton(B3);
            InitButton(B4);
            InitButton(B5);
            InitButton(B6);
            InitButton(B7);
        }

        #endregion
    }
}


public static class BitmapConversion
{
    public static BitmapSource BitmapToBitmapSource(Bitmap source)
    {
        return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                      source.GetHbitmap(),
                      IntPtr.Zero,
                      Int32Rect.Empty,
                      BitmapSizeOptions.FromEmptyOptions());
    }
}
