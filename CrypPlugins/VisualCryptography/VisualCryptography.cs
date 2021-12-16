/*
   Copyright 2021 Nils Kopal <Nils.Kopal<at>CrypTool.org

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
using CrypTool.PluginBase;
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
using CrypTool.Plugins.VisualCryptography.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Windows.Controls;
using Font = VisualCryptography.Font;

namespace CrypTool.Plugins.VisualCryptography
{
    [Author("Nils Kopal", "kopal@CrypTool.org", "CrypTool 2 Team", "https://www.CrypTool.org")]
    [PluginInfo("CrypTool.Plugins.VisualCryptography.Properties.Resources", "PluginCaption", "PluginTooltip", "VisualCryptography/userdoc.xml", new[] { "VisualCryptography/icon.png" })]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class VisualCryptography : ICrypComponent
    {
        #region Private Variables

        private readonly VisualCryptographySettings _settings = new VisualCryptographySettings();
        private readonly VisualCryptographyPresentation _presentation = new VisualCryptographyPresentation();

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "PlaintextCaption", "PlaintextTooltip", true)]
        public object Plaintext
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "Image1Caption", "Image1Tooltip")]
        public byte[] Image1
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "Image2", "Image2Tooltip")]
        public byte[] Image2
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings => _settings;

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation => _presentation;

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);

            //check if we have a string, a CrypToolstream, or a byte[] and encrypt it
            if (Plaintext is ICrypToolStream)
            {
                EncryptImage(true);
            }
            else if (Plaintext is byte[])
            {
                EncryptImage(false);
            }
            else if (Plaintext is string)
            {
                EncryptString();
            }
            else if (Plaintext is null)
            {
                //nothing given at all
                GuiLogMessage(Resources.NoPlaintextInputProvided, NotificationLevel.Error);
            }
            else
            {
                //invalid data type given
                GuiLogMessage(string.Format(Resources.InvalidInput, Plaintext.GetType().Name), NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Encrypts an image
        /// </summary>
        /// <param name="isCrypToolStream"></param>
        private void EncryptImage(bool isCrypToolStream)
        {
            Bitmap bmp;
            Stream stream;
            byte[] image;

            //convert Input to image
            try
            {
                if (isCrypToolStream)
                {
                    //we can directly read the image from the stream
                    stream = ((ICrypToolStream)Plaintext).CreateReader();
                }
                else
                {
                    //we have to create a stream and write the image into it
                    //then, we can read it from the stream
                    stream = new MemoryStream();
                    byte[] data = (byte[])Plaintext;
                    stream.Write(data, 0, data.Length);
                }
                using (stream)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    bmp = new Bitmap(stream);
                }
                //convert bmp to black and white "image array"
                image = new byte[bmp.Width * bmp.Height];
                for (int x = 0; x < bmp.Width; x++)
                {
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        Color pixel = bmp.GetPixel(x, y);
                        byte color = 0;
                        //the grayscale computation is based on the human perception of colors
                        double grayscale = 0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B;
                        if (grayscale > _settings.Threshhold)
                        {
                            color = 0xFF;
                        }
                        image[x + y * bmp.Width] = color;
                    }
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format(Resources.CouldNotReadImage, ex.Message), NotificationLevel.Error);
                return;
            }
            Encrypt((image, bmp.Width, bmp.Height), false);
        }

        /// <summary>
        /// Encrypts a string
        /// </summary>
        private void EncryptString()
        {
            //create the "plaintext image"
            string plaintext = (string)Plaintext;
            (byte[] image, int width, int height) image = CreatePlaintextImageData(plaintext, _settings.CharactersPerRow);
            Encrypt(image, true);
        }

        private void Encrypt((byte[] image, int width, int height) image, bool resize)
        {
            if (resize)
            {
                //increase number of pixels of plaintext image
                image = ResizeImage(image.image, image.width, image.height, 4);
            }

            //encrypt the resized image
            (byte[] image1, byte[] image2, int width, int height) encrypted_images = EncryptImage(image.image, image.width, image.height);

            //convert raw image data (byte arrays) to actual bitmaps
            Bitmap imageFile1 = CreateBitmap(encrypted_images.image1, encrypted_images.width, encrypted_images.height);
            Bitmap imageFile2 = CreateBitmap(encrypted_images.image2, encrypted_images.width, encrypted_images.height);

            _presentation.SetImages(encrypted_images);
            _presentation.UpdateImage(0);

            //output first bitmap
            using (MemoryStream stream = new MemoryStream())
            {
                imageFile1.Save(stream, ImageFormat.Png);
                Image1 = stream.ToArray();
            }
            OnPropertyChanged("Image1");

            //output second bitmap
            using (MemoryStream stream = new MemoryStream())
            {
                imageFile2.Save(stream, ImageFormat.Png);
                Image2 = stream.ToArray();
            }
            OnPropertyChanged("Image2");

            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion


        #region Visual cryptography algorithm part

        /// <summary>
        /// Creates a plaintext image based on the given plaintext string
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="maxwidth"></param>
        /// <returns>(image, width, height)</returns>
        private (byte[] image, int width, int height) CreatePlaintextImageData(string plaintext, int maxwidth = 15)
        {
            int rows = 3; // we want to have an empty row before and after the text
            int columns = plaintext.Length + 2; // we want to have an empty column at beginning and end of each text line

            if (plaintext.Length > maxwidth)
            {
                //calculate rows and columns based on the size of the given plaintext
                rows = (int)Math.Ceiling((plaintext.Length / (double)maxwidth) + 2);
                columns = maxwidth + 2;
            }
            int width = columns * 8;
            int height = rows * 8;

            byte[] image = new byte[width * height];
            for (int i = 0; i < image.Length; i++)
            {
                image[i] = 0xFF;
            }

            int column = 1;
            int row = 1;

            foreach (char c in plaintext)
            {
                PrintPlaintextCharacter(c, image, width, column, row);
                column++;
                if (column == columns - 1)
                {
                    column = 1;
                    row++;
                }
            }
            return (image, width, height);
        }

        /// <summary>
        /// Prints the character into the image at the given column and row position
        /// </summary>
        /// <param name="c"></param>
        /// <param name="image"></param>
        /// <param name="imageWidth"></param>
        /// <param name="column"></param>
        /// <param name="row"></param>
        private void PrintPlaintextCharacter(char c, byte[] image, int imageWidth, int column, int row)
        {
            //or font only contains ASCII chars
            if (c > Font.Data.Length)
            {
                c = '?';
            }

            ulong symbol = Font.Data[c]; //get symbol to "print"
            int x_offset = column * 8;
            int y_offset = row * 8;

            for (int y = 7; y >= 0; y--)
            {
                for (int x = 7; x >= 0; x--)
                {
                    if ((symbol & 1) == 1)
                    {
                        image[x + x_offset + (y + y_offset) * imageWidth] = 0x00;
                    }
                    else
                    {
                        image[x + x_offset + (y + y_offset) * imageWidth] = 0xFF;
                    }
                    symbol = symbol >> 1;
                }
            }
        }

        /// <summary>
        /// Create a bitmap using the given byte array, width, and height
        /// </summary>
        /// <param name="image"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Bitmap CreateBitmap(byte[] image, int width, int height)
        {
            return new Bitmap(width, height, width,
                PixelFormat.Format8bppIndexed,
                Marshal.UnsafeAddrOfPinnedArrayElement(image, 0));
        }

        /// <summary>
        /// Doubles the size of the given image
        /// </summary>
        /// <param name="image"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private static (byte[] image, int width, int height) ResizeImage(byte[] image, int width, int height, int factor)
        {
            if (factor < 2)
            {
                return (image, width, height);
            }
            byte[] newimage = new byte[width * factor * height * factor];
            int width_factor = width * factor;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    byte pixel = image[x + y * width];
                    int x_factor = x * factor;
                    int y_factor = y * factor;
                    for (int i = 0; i < factor; i++)
                    {
                        for (int j = 0; j < factor; j++)
                        {
                            newimage[i + x_factor + (j + y_factor) * width_factor] = pixel;
                        }
                    }
                }
            }
            return (newimage, width_factor, height * factor);
        }

        /// <summary>
        /// Encrypts the given image and creates two "noisy images", which have to be combined for decryption
        /// RNGCryptoServiceProvider is used for creation of random bytes for encryption
        /// </summary>
        /// <param name="image"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private (byte[] image1, byte[] image2, int width, int height) EncryptImage(byte[] image, int width, int height)
        {
            byte[] image1 = new byte[2 * width * 2 * height];
            byte[] image2 = new byte[2 * width * 2 * height];
            int width_height = width * height;
            int width_2 = width * 2;

            //Create random data for encryption
            RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();
            byte[] randomData = new byte[2 * width * height];
            random.GetBytes(randomData);

            //Create list of visual patterns, which is used for selecting the pattern for a pixel during encryption
            List<VisualPattern> visualPatternsList = GenerateVisualPatternsList();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int o = x + y * width;
                    byte pixel = image[o];
                    bool coinFlip = randomData[o] > 127; //50 : 50 chance
                    int x_2 = x * 2;
                    int y_2 = y * 2;

                    VisualPattern pattern = visualPatternsList[randomData[width_height + o] % visualPatternsList.Count];
                    if (pixel == 0xFF)//white pixel
                    {
                        EncryptWhitePixel(image1, image2, x_2, y_2, width_2, pattern, coinFlip);
                    }
                    else //black pixel
                    {
                        EncryptBlackPixel(image1, image2, x_2, y_2, width_2, pattern, coinFlip);
                    }
                }
            }

            return (image1, image2, width_2, height * 2);
        }

        /// <summary>
        /// Create list of visual patterns, which is used for selecting the pattern for a pixel during encryption
        /// </summary>
        /// <returns></returns>
        public List<VisualPattern> GenerateVisualPatternsList()
        {
            List<VisualPattern> visualPatternsList = new List<VisualPattern>();
            switch (_settings.VisualPattern)
            {
                case VisualPattern.Horizontal:
                    visualPatternsList.Add(VisualPattern.Horizontal);
                    break;
                case VisualPattern.Vertical:
                    visualPatternsList.Add(VisualPattern.Vertical);
                    break;
                default:
                case VisualPattern.Diagonal:
                    visualPatternsList.Add(VisualPattern.Diagonal);
                    break;
                case VisualPattern.HorizontalVertical:
                    visualPatternsList.Add(VisualPattern.Horizontal);
                    visualPatternsList.Add(VisualPattern.Vertical);
                    break;
                case VisualPattern.HorizontalDiagonal:
                    visualPatternsList.Add(VisualPattern.Horizontal);
                    visualPatternsList.Add(VisualPattern.Diagonal);
                    break;
                case VisualPattern.VerticalDiagonal:
                    visualPatternsList.Add(VisualPattern.Vertical);
                    visualPatternsList.Add(VisualPattern.Diagonal);
                    break;
                case VisualPattern.HorizontalVerticalDiagonal:
                    visualPatternsList.Add(VisualPattern.Horizontal);
                    visualPatternsList.Add(VisualPattern.Vertical);
                    visualPatternsList.Add(VisualPattern.Diagonal);
                    break;
            }
            return visualPatternsList;
        }

        /// <summary>
        /// Encrypts a white pixel (here, each pixel are 4 pixels) at the defined position
        /// </summary>
        /// <param name="image1"></param>
        /// <param name="image2"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="pattern"></param>
        /// <param name="coinFlip"></param>
        private void EncryptWhitePixel(byte[] image1, byte[] image2, int x, int y, int width, VisualPattern pattern, bool coinFlip)
        {
            switch (pattern)
            {

                case VisualPattern.Horizontal:
                    if (coinFlip)
                    {
                        image1[0 + x + (0 + y) * width] = 0x00;
                        image1[1 + x + (0 + y) * width] = 0x00;
                        image1[0 + x + (1 + y) * width] = 0xFF;
                        image1[1 + x + (1 + y) * width] = 0xFF;

                        image2[0 + x + (0 + y) * width] = 0x00;
                        image2[1 + x + (0 + y) * width] = 0x00;
                        image2[0 + x + (1 + y) * width] = 0xFF;
                        image2[1 + x + (1 + y) * width] = 0xFF;
                    }
                    else
                    {
                        image1[0 + x + (0 + y) * width] = 0xFF;
                        image1[1 + x + (0 + y) * width] = 0xFF;
                        image1[0 + x + (1 + y) * width] = 0x00;
                        image1[1 + x + (1 + y) * width] = 0x00;

                        image2[0 + x + (0 + y) * width] = 0xFF;
                        image2[1 + x + (0 + y) * width] = 0xFF;
                        image2[0 + x + (1 + y) * width] = 0x00;
                        image2[1 + x + (1 + y) * width] = 0x00;
                    }
                    break;

                case VisualPattern.Vertical:
                    if (coinFlip)
                    {
                        image1[0 + x + (0 + y) * width] = 0x00;
                        image1[0 + x + (1 + y) * width] = 0x00;
                        image1[1 + x + (0 + y) * width] = 0xFF;
                        image1[1 + x + (1 + y) * width] = 0xFF;

                        image2[0 + x + (0 + y) * width] = 0x00;
                        image2[0 + x + (1 + y) * width] = 0x00;
                        image2[1 + x + (0 + y) * width] = 0xFF;
                        image2[1 + x + (1 + y) * width] = 0xFF;
                    }
                    else
                    {
                        image1[0 + x + (0 + y) * width] = 0xFF;
                        image1[0 + x + (1 + y) * width] = 0xFF;
                        image1[1 + x + (0 + y) * width] = 0x00;
                        image1[1 + x + (1 + y) * width] = 0x00;

                        image2[0 + x + (0 + y) * width] = 0xFF;
                        image2[0 + x + (1 + y) * width] = 0xFF;
                        image2[1 + x + (0 + y) * width] = 0x00;
                        image2[1 + x + (1 + y) * width] = 0x00;
                    }
                    break;

                case VisualPattern.Diagonal:
                    if (coinFlip)
                    {
                        image1[0 + x + (0 + y) * width] = 0x00;
                        image1[1 + x + (1 + y) * width] = 0x00;
                        image1[0 + x + (1 + y) * width] = 0xFF;
                        image1[1 + x + (0 + y) * width] = 0xFF;

                        image2[0 + x + (0 + y) * width] = 0x00;
                        image2[1 + x + (1 + y) * width] = 0x00;
                        image2[0 + x + (1 + y) * width] = 0xFF;
                        image2[1 + x + (0 + y) * width] = 0xFF;
                    }
                    else
                    {
                        image1[0 + x + (0 + y) * width] = 0xFF;
                        image1[1 + x + (1 + y) * width] = 0xFF;
                        image1[0 + x + (1 + y) * width] = 0x00;
                        image1[1 + x + (0 + y) * width] = 0x00;

                        image2[0 + x + (0 + y) * width] = 0xFF;
                        image2[1 + x + (1 + y) * width] = 0xFF;
                        image2[0 + x + (1 + y) * width] = 0x00;
                        image2[1 + x + (0 + y) * width] = 0x00;
                    }
                    break;
            }
        }

        /// <summary>
        /// Encrypts a black pixel (here, each pixel are 4 pixels) at the defined position
        /// </summary>
        /// <param name="image1"></param>
        /// <param name="image2"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="pattern"></param>
        /// <param name="coinFlip"></param>
        private void EncryptBlackPixel(byte[] image1, byte[] image2, int x, int y, int width, VisualPattern pattern, bool coinFlip)
        {
            switch (pattern)
            {
                case VisualPattern.Horizontal:
                    if (coinFlip)
                    {
                        image1[0 + x + (0 + y) * width] = 0x00;
                        image1[1 + x + (0 + y) * width] = 0x00;
                        image1[0 + x + (1 + y) * width] = 0xFF;
                        image1[1 + x + (1 + y) * width] = 0xFF;

                        image2[0 + x + (0 + y) * width] = 0xFF;
                        image2[1 + x + (0 + y) * width] = 0xFF;
                        image2[0 + x + (1 + y) * width] = 0x00;
                        image2[1 + x + (1 + y) * width] = 0x00;
                    }
                    else
                    {
                        image1[0 + x + (0 + y) * width] = 0xFF;
                        image1[1 + x + (0 + y) * width] = 0xFF;
                        image1[0 + x + (1 + y) * width] = 0x00;
                        image1[1 + x + (1 + y) * width] = 0x00;

                        image2[0 + x + (0 + y) * width] = 0x00;
                        image2[1 + x + (0 + y) * width] = 0x00;
                        image2[0 + x + (1 + y) * width] = 0xFF;
                        image2[1 + x + (1 + y) * width] = 0xFF;
                    }
                    break;

                case VisualPattern.Vertical:
                    if (coinFlip)
                    {
                        image1[0 + x + (0 + y) * width] = 0x00;
                        image1[0 + x + (1 + y) * width] = 0x00;
                        image1[1 + x + (0 + y) * width] = 0xFF;
                        image1[1 + x + (1 + y) * width] = 0xFF;

                        image2[0 + x + (0 + y) * width] = 0xFF;
                        image2[0 + x + (1 + y) * width] = 0xFF;
                        image2[1 + x + (0 + y) * width] = 0x00;
                        image2[1 + x + (1 + y) * width] = 0x00;
                    }
                    else
                    {
                        image1[0 + x + (0 + y) * width] = 0xFF;
                        image1[0 + x + (1 + y) * width] = 0xFF;
                        image1[1 + x + (0 + y) * width] = 0x00;
                        image1[1 + x + (1 + y) * width] = 0x00;

                        image2[0 + x + (0 + y) * width] = 0x00;
                        image2[0 + x + (1 + y) * width] = 0x00;
                        image2[1 + x + (0 + y) * width] = 0xFF;
                        image2[1 + x + (1 + y) * width] = 0xFF;
                    }
                    break;

                case VisualPattern.Diagonal:
                    if (coinFlip)
                    {
                        image1[0 + x + (0 + y) * width] = 0x00;
                        image1[1 + x + (1 + y) * width] = 0x00;
                        image1[0 + x + (1 + y) * width] = 0xFF;
                        image1[1 + x + (0 + y) * width] = 0xFF;

                        image2[0 + x + (0 + y) * width] = 0xFF;
                        image2[1 + x + (1 + y) * width] = 0xFF;
                        image2[0 + x + (1 + y) * width] = 0x00;
                        image2[1 + x + (0 + y) * width] = 0x00;
                    }
                    else
                    {
                        image1[0 + x + (0 + y) * width] = 0xFF;
                        image1[1 + x + (1 + y) * width] = 0xFF;
                        image1[0 + x + (1 + y) * width] = 0x00;
                        image1[1 + x + (0 + y) * width] = 0x00;

                        image2[0 + x + (0 + y) * width] = 0x00;
                        image2[1 + x + (1 + y) * width] = 0x00;
                        image2[0 + x + (1 + y) * width] = 0xFF;
                        image2[1 + x + (0 + y) * width] = 0xFF;
                    }
                    break;
            }
        }

        /// <summary>
        /// Merges two given images (of same size) to a double sized image
        /// Images are overlayed using an offset
        /// </summary>
        /// <param name="image1"></param>
        /// <param name="image2"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static (byte[] image, int width, int height) MergeImages(byte[] image1, byte[] image2, int width, int height, int offset)
        {
            if (offset < 0)
            {
                offset = 0;
            }
            if (offset > height)
            {
                offset = height;
            }

            byte[] mergedImage = new byte[width * height * 2];

            //paint mergedImage white
            for (int i = 0; i < mergedImage.Length; i++)
            {
                mergedImage[i] = 0xFF;
            }

            //paint 2nd image
            int offset2ndImage = width * height - offset * width;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int image2Index = x + y * width;
                    mergedImage[image2Index + offset2ndImage] = image2[image2Index];
                }
            }

            //paint 1st image at offset position
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int image1Index = x + y * width;
                    int mergedImageIndex = image1Index + offset * width;

                    if (mergedImage[mergedImageIndex] == 0xFF) //we only have to paint over white pixels
                    {
                        if (image1[image1Index] == 0x00) // only overwrite white pixels with black pixels
                        {
                            mergedImage[mergedImageIndex] = 0x00;
                        }
                    }
                }
            }
            return (mergedImage, width, height * 2);
        }
        #endregion        
    }
}
