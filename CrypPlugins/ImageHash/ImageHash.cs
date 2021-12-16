/*
   Copyright 2011 CrypTool 2 Team <ct2contact@CrypTool.org>

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
using Emgu.CV;
using Emgu.CV.Structure;
using ImageHash.Properties;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace CrypTool.Plugins.ImageHash
{
    [Author("Bastian Heuser", "bhe@student.uni-kassel.de", "Uni Kassel", "http://www.uni-kassel.de/eecs/fachgebiete/ais/")]
    [PluginInfo("ImageHash.Properties.Resources", "PluginCaption", "PluginTooltip", "ImageHash/DetailedDescription/doc.xml", "ImageHash/icon.png")]
    [ComponentCategory(ComponentCategory.HashFunctions)]
    public class ImageHash : ICrypComponent
    {

        #region Private Variables

        private readonly ImageHashSettings settings;
        private ICrypToolStream inputImage;
        private Image<Bgr, byte> orgImg;
        private Image<Gray, double> step1Img;
        private Image<Gray, double> step2Img;
        private Image<Gray, double> step4Img;
        private Bitmap step6Bmp;
        private byte[] outputHash;
        private bool isRunning = true;

        #endregion

        #region Data Properties

        /// <summary>
        /// Input image ICrypToolStream, handles "inputImage".
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputImageCaption", "InputImageTooltip", true)]
        public ICrypToolStream InputImage
        {
            get => inputImage;
            set
            {
                if (value != inputImage)
                {
                    inputImage = value;
                }
            }
        }

        /// <summary>
        /// Output hash byte[].
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputHashCaption", "OutputHashTooltip", true)]
        public byte[] OutputHash
        {
            get => outputHash;
            set
            {
                if (outputHash != value)
                {
                    outputHash = value;
                    OnPropertyChanged("OutputHash");
                }
            }
        }

        /// <summary>
        /// Output processed image as ICrypToolStream.
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputImageCaption", "OutputImageTooltip")]
        public ICrypToolStream OutputImage
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members and Execution

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings => settings;

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation => null;

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            inputImage = null;
            orgImg = null;
            step1Img = null;
            step2Img = null;
            step4Img = null;
            step6Bmp = null;
            outputHash = null;
            isRunning = true;
        }

        /// <summary>
        /// Constructor. Called once when class is called.
        /// </summary>
        public ImageHash()
        {
            settings = new ImageHashSettings();
            settings.PropertyChanged += new PropertyChangedEventHandler(settings_PropertyChanged);
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            int progress = 1;
            const int STEPS = 10;

            // An imagesize under 4x4 does not make any sense
            // Sizes above 128x128 get to slow
            if (settings.Size < 4)
            {
                settings.Size = 4;
                GuiLogMessage(Resources.ToSmallWarning, NotificationLevel.Warning);
            }
            else if (settings.Size > 128)
            {
                settings.Size = 128;
                GuiLogMessage(Resources.ToBigWarning, NotificationLevel.Warning);
            }
            else
            {
                int size = settings.Size;
                bool wrongSize = true;
                int max = 1;
                // Check if the input size is a power of two
                for (int i = 4; i <= 1024; i *= 2)
                {
                    if (size == i)
                    {
                        wrongSize = false;
                        break;
                    }
                    else if (size < i)
                    {
                        max = i;
                        break;
                    }
                }

                int j = max / 2;
                // If the input size is not a power of two, set it to the nearest power of two
                if (wrongSize)
                {
                    int middle = (max - j) / 2;
                    if (size <= j + middle)
                    {
                        size = j;
                    }
                    else
                    {
                        size = max;
                    }
                    settings.Size = size;
                    GuiLogMessage(Resources.PowerOfTwoWarning + " " + size + "x" + size + ".", NotificationLevel.Warning);
                }
            }
            OnPropertyChanged("size");

            ProgressChanged(0, 1);

            if (InputImage == null)
            {
                GuiLogMessage(Resources.NoImageError, NotificationLevel.Error);
                return;
            }

            using (CStreamReader reader = inputImage.CreateReader())
            {
                using (Bitmap bitmap = new Bitmap(reader))
                {
                    // Original Image:
                    orgImg = new Image<Bgr, byte>(bitmap);
                    if ((settings.PresentationStep > 1 && settings.ShowEachStep) || (settings.PresentationStep == 1))
                    {
                        CreateOutputStream(bitmap);
                        OnPropertyChanged("OutputImage");
                    }
                    // Step 1: Gray scale
                    step1Img = orgImg.Convert<Gray, byte>().Convert<Gray, double>();
                    ProgressChanged(progress++, STEPS);
                    if ((settings.PresentationStep > 2 && settings.ShowEachStep) || (settings.PresentationStep == 2))
                    {
                        CreateOutputStream(step1Img.ToBitmap());
                        OnPropertyChanged("OutputImage");
                    }
                    ProgressChanged(progress++, STEPS);

                    // Step 2: Resize to sizexsize (standard: 16x16)
                    int size = settings.Size;   // standard: 16
                    int halfSize = size / 2;    // standard: 8
                    step2Img = step1Img.Resize(size, size, Emgu.CV.CvEnum.Inter.Nearest);
                    ProgressChanged(progress++, STEPS);
                    if ((settings.PresentationStep > 3 && settings.ShowEachStep) || (settings.PresentationStep == 3))
                    {
                        CreateOutputStream(step2Img.ToBitmap());
                        OnPropertyChanged("OutputImage");
                    }
                    ProgressChanged(progress++, STEPS);

                    // Step 3: Find brightest quarter
                    float[] subImage = new float[4];
                    for (int i = 0; i < step2Img.Width && isRunning; i++)
                    {
                        for (int j = 0; j < step2Img.Height && isRunning; j++)
                        {
                            if ((i < halfSize) && (j < halfSize))
                            {
                                subImage[0] += step2Img.ToBitmap().GetPixel(i, j).GetBrightness();
                            }
                            else if ((i >= halfSize) && (j < halfSize))
                            {
                                subImage[1] += step2Img.ToBitmap().GetPixel(i, j).GetBrightness();
                            }
                            else if ((i < halfSize) && (j >= halfSize))
                            {
                                subImage[2] += step2Img.ToBitmap().GetPixel(i, j).GetBrightness();
                            }
                            else if ((i >= halfSize) && (j >= halfSize))
                            {
                                subImage[3] += step2Img.ToBitmap().GetPixel(i, j).GetBrightness();
                            }
                        }
                    }

                    float maxValue = subImage.Max();
                    int flip = subImage.ToList().IndexOf(maxValue);
                    ProgressChanged(progress++, STEPS);

                    // Step 4: Flip brightest quarter to left upper corner
                    step4Img = step2Img;
                    switch (flip)
                    {
                        case 1:
                            step4Img = step4Img.Flip(Emgu.CV.CvEnum.FlipType.Horizontal);
                            subImage = swap(subImage, 1);
                            break;
                        case 2:
                            step4Img = step4Img.Flip(Emgu.CV.CvEnum.FlipType.Vertical);
                            subImage = swap(subImage, 2);
                            break;
                        case 3:
                            step4Img = step4Img.Flip(Emgu.CV.CvEnum.FlipType.Horizontal);
                            step4Img = step4Img.Flip(Emgu.CV.CvEnum.FlipType.Vertical);
                            subImage = swap(subImage, 3);
                            break;
                    }

                    ProgressChanged(progress++, STEPS);
                    if ((settings.PresentationStep > 4 && settings.ShowEachStep) || (settings.PresentationStep == 4))
                    {
                        CreateOutputStream(step4Img.ToBitmap());
                        OnPropertyChanged("OutputImage");
                    }
                    ProgressChanged(progress++, STEPS);

                    // Step 5: Find median
                    float[] median = new float[4];
                    for (int i = 0; i < median.Length; i++)
                    {
                        median[i] = subImage[i] / ((size * size) / median.Length);
                    }
                    ProgressChanged(progress++, STEPS);

                    // Step 6: Set Brightness to 0 or 1, if above or under median
                    Bitmap step4Bmp = step4Img.ToBitmap();
                    step6Bmp = new Bitmap(step4Img.Width, step4Img.Height);
                    bool[][] b = new bool[4][];
                    for (int i = 0; i < b.Length; i++)
                    {
                        b[i] = new bool[(size * size) / median.Length];
                    }

                    for (int i = 0; i < step4Bmp.Width && isRunning; i++)
                    {
                        for (int j = 0; j < step4Bmp.Height && isRunning; j++)
                        {
                            if ((i < halfSize) && (j < halfSize))
                            {
                                float brightness = step4Bmp.GetPixel(i, j).GetBrightness();
                                if (brightness >= median[0])
                                {
                                    step6Bmp.SetPixel(i, j, Color.White);
                                    b[0][i * halfSize + j] = false;
                                }
                                else
                                {
                                    step6Bmp.SetPixel(i, j, Color.Black);
                                    b[0][i * halfSize + j] = true;
                                }
                            }
                            else if ((i >= halfSize) && (j < halfSize))
                            {
                                float brightness = step4Bmp.GetPixel(i, j).GetBrightness();
                                if (brightness >= median[1])
                                {
                                    step6Bmp.SetPixel(i, j, Color.White);
                                    b[1][(i - halfSize) * halfSize + j] = false;
                                }
                                else
                                {
                                    step6Bmp.SetPixel(i, j, Color.Black);
                                    b[1][(i - halfSize) * halfSize + j] = true;
                                }
                            }
                            else if ((i < halfSize) && (j >= halfSize))
                            {
                                float brightness = step4Bmp.GetPixel(i, j).GetBrightness();
                                if (brightness >= median[2])
                                {
                                    step6Bmp.SetPixel(i, j, Color.White);
                                    b[2][i * halfSize + (j - halfSize)] = false;
                                }
                                else
                                {
                                    step6Bmp.SetPixel(i, j, Color.Black);
                                    b[2][i * halfSize + (j - halfSize)] = true;
                                }
                            }
                            else if ((i >= halfSize) && (j >= halfSize))
                            {
                                float brightness = step4Bmp.GetPixel(i, j).GetBrightness();
                                if (brightness >= median[3])
                                {
                                    step6Bmp.SetPixel(i, j, Color.White);
                                    b[3][(i - halfSize) * halfSize + (j - halfSize)] = false;
                                }
                                else
                                {
                                    step6Bmp.SetPixel(i, j, Color.Black);
                                    b[3][(i - halfSize) * halfSize + (j - halfSize)] = true;
                                }
                            }
                        }
                    }

                    ProgressChanged(progress++, STEPS);
                    if (settings.PresentationStep == 5)
                    {
                        CreateOutputStream(step6Bmp);
                        OnPropertyChanged("OutputImage");
                    }
                    ProgressChanged(progress++, STEPS);

                    // Step 7: Calculate the hash
                    bool[] bools = new bool[b.Length * b[0].Length];
                    for (int i = 0; i < b.Length && isRunning; i++)
                    {
                        for (int j = 0; j < b[i].Length && isRunning; j++)
                        {
                            bools[i * b[i].Length + j] = b[i][j];
                        }
                    }

                    byte[] byteArray = new byte[bools.Length / 8];
                    int bitIndex = 0, byteIndex = 0;
                    for (int i = 0; i < bools.Length && isRunning; i++)
                    {
                        if (bools[i])
                        {
                            byteArray[byteIndex] |= (byte)(1 << bitIndex);
                        }
                        bitIndex++;
                        if (bitIndex == 8)
                        {
                            bitIndex = 0;
                            byteIndex++;
                        }
                    }
                    OutputHash = byteArray;
                    OnPropertyChanged("OutputHash");
                }
            }
            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Listener that checks which property changed and acts accordingly.
        /// </summary>
        private void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "PresentationStep":
                    switch (settings.PresentationStep)
                    {
                        case 1:
                            CreateOutputStream(orgImg.ToBitmap());
                            break;
                        case 2:
                            CreateOutputStream(step1Img.ToBitmap());
                            break;
                        case 3:
                            CreateOutputStream(step2Img.ToBitmap());
                            break;
                        case 4:
                            CreateOutputStream(step4Img.ToBitmap());
                            break;
                        case 5:
                            CreateOutputStream(step6Bmp);
                            break;
                    }
                    OnPropertyChanged("OutputImage");
                    break;
            }
        }

        /// <summary>
        /// Swaps indices of subImage horizontally, vertically or both.
        /// </summary>
        /// <param name="subImage">The float array to swap.</param>
        /// <param name="i">Integer how to swap (1=horizontal) (2=vertical) (3=both).</param>
        private float[] swap(float[] subImage, int i)
        {
            switch (i)
            {
                case 1:
                    subImage = swap(subImage, 0, 1);
                    subImage = swap(subImage, 2, 3);
                    break;
                case 2:
                    subImage = swap(subImage, 0, 2);
                    subImage = swap(subImage, 1, 3);
                    break;
                case 3:
                    subImage = swap(subImage, 1);
                    subImage = swap(subImage, 2);
                    break;
            }

            return subImage;
        }

        /// <summary>
        /// Swaps indices i and j in a float array.
        /// </summary>
        /// <param name="subImage">The float array to swap.</param>
        /// <param name="i">First index.</param>
        /// <param name="j">Second index.</param>
        private float[] swap(float[] subImage, int i, int j)
        {
            float f = subImage[i];
            subImage[i] = subImage[j];
            subImage[j] = f;
            return subImage;
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
            isRunning = false;
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


        #region HelpFunctions

        /// <summary>
        /// Resizes bitmap b to width nWidth and height nHeight.
        /// </summary>
        /// <param name="b">The bitmap to resize.</param>
        /// <param name="nWidth">The new width.</param>
        /// <param name="nHeight">The new height.</param>
        private Bitmap ResizeBitmap(Bitmap b)
        {
            int size = 0;
            if (b.Width < b.Height)
            {
                size = b.Width;
            }
            else
            {
                size = b.Height;
            }

            int newSize = size;
            while (newSize < 300)
            {
                newSize *= 2;
            }
            Bitmap result = new Bitmap(newSize, newSize);
            newSize += newSize / (size * 2 - 1);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.SmoothingMode = SmoothingMode.None;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(b, 0, 0, newSize, newSize);
            }
            return result;
        }

        /// <summary>Create output stream to display.</summary>
        /// <param name="bitmap">The bitmap to display.</param>
        private void CreateOutputStream(Bitmap bitmap)
        {
            // Avoid smoothing of WPF
            if (settings.PresentationStep > 2)
            {
                bitmap = ResizeBitmap(bitmap);
            }

            ImageFormat format = ImageFormat.Bmp;
            if (settings.OutputFileFormat == 1)
            {
                format = ImageFormat.Png;
            }
            else if (settings.OutputFileFormat == 2)
            {
                format = ImageFormat.Tiff;
            }

            // Avoid "generic error in GDI+"
            Bitmap saveableBitmap = CopyBitmap(bitmap, format);

            // Save bitmap
            MemoryStream outputStream = new MemoryStream();
            saveableBitmap.Save(outputStream, format);
            saveableBitmap.Dispose();
            bitmap.Dispose();

            OutputImage = new CStreamWriter(outputStream.GetBuffer());
        }

        /// <summary>Makes sure that a bitmap is not a useless "MemoryBitmap".</summary>
        /// <param name="bitmap">Any image.</param>
        /// <param name="format">Image format.</param>
        /// <returns>Definitely not broken bitmap.</returns>
        private Bitmap CopyBitmap(Bitmap bitmap, ImageFormat format)
        {
            MemoryStream buffer = new MemoryStream();
            bitmap.Save(buffer, format);
            Bitmap saveableBitmap = (Bitmap)System.Drawing.Image.FromStream(buffer);
            return saveableBitmap;
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
    }
}
