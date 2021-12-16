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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Controls;


namespace CrypTool.Plugins.ImageProcessor
{
    [Author("Bastian Heuser", "bhe@student.uni-kassel.de", "Uni Kassel", "http://www.uni-kassel.de/eecs/fachgebiete/ais/")]
    [PluginInfo("CrypTool.Plugins.ImageProcessor.Properties.Resources", "PluginCaption", "PluginTooltip", "ImageProcessor/userdoc.xml", new[] { "ImageProcessor/icon.png" })]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class ImageProcessor : ICrypComponent
    {
        #region Private Variables and Constructor

        private readonly ImageProcessorSettings settings = new ImageProcessorSettings();

        public ImageProcessor()
        {
            settings = new ImageProcessorSettings();
            settings.UpdateTaskPaneVisibility();
            settings.PropertyChanged += new PropertyChangedEventHandler(settings_PropertyChanged);
        }

        #endregion

        #region Data Properties

        /// <summary>
        /// Input image1 ICrypToolStream, handles "inputImage1".
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputImage1Caption", "InputImage1Tooltip", true)]
        public ICrypToolStream InputImage1
        {
            get;
            set;
        }

        /// <summary>
        /// Input image2 ICrypToolStream, handles "inputImage2".
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputImage2Caption", "InputImage2Tooltip")]
        public ICrypToolStream InputImage2
        {
            get;
            set;
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

        #region IPlugin Members

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
            InputImage1 = null;
            InputImage2 = null;
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);

            if (InputImage1 == null)
            {
                if (InputImage2 != null)
                {
                    InputImage1 = InputImage2;
                }
                else
                {
                    GuiLogMessage("Please select an image.", NotificationLevel.Error);
                    return;
                }
            }

            using (CStreamReader reader = InputImage1.CreateReader())
            {
                using (Bitmap bitmap = new Bitmap(reader))
                {
                    using (Image<Bgr, byte> img = new Image<Bgr, byte>(bitmap))
                    {
                        switch (settings.Action)
                        {
                            case ActionType.none:   // Copy image to output
                                CreateOutputStream(img.ToBitmap(), false);
                                break;
                            case ActionType.flip:   // Flip Image
                                switch (settings.FlipType)
                                {
                                    case 0:         // Horizontal
                                        img._Flip(Emgu.CV.CvEnum.FlipType.Horizontal);
                                        CreateOutputStream(img.ToBitmap());
                                        break;
                                    case 1:         // Vertical
                                        img._Flip(Emgu.CV.CvEnum.FlipType.Vertical);
                                        CreateOutputStream(img.ToBitmap());
                                        break;
                                }
                                break;
                            case ActionType.gray:   // Gray Scale
                                using (Image<Gray, double> grayImg = img.Convert<Gray, byte>().Convert<Gray, double>())
                                {
                                    CreateOutputStream(grayImg.ToBitmap());
                                }
                                break;
                            case ActionType.smooth: // Smoothing
                                if (settings.Smooth > 10000)
                                {
                                    settings.Smooth = 10000;
                                }

                                int smooth = settings.Smooth;
                                if (smooth % 2 == 0)
                                {
                                    smooth++;
                                    settings.Smooth = smooth;
                                }
                                img._SmoothGaussian(smooth);
                                CreateOutputStream(img.ToBitmap());
                                break;
                            case ActionType.resize: // Resizeing
                                if (settings.SizeX > 4096)
                                {
                                    settings.SizeX = 4096;
                                }

                                if (settings.SizeY > 4096)
                                {
                                    settings.SizeY = 4096;
                                }

                                using (Image<Bgr, byte> newImg = img.Resize(settings.SizeX, settings.SizeY, Emgu.CV.CvEnum.Inter.Linear))
                                {
                                    CreateOutputStream(newImg.ToBitmap());
                                }
                                break;
                            case ActionType.crop:   // Cropping
                                CropImage();
                                break;
                            case ActionType.rotate: // Rotating
                                using (Image<Bgr, byte> newImg = img.Rotate(settings.Degrees % 360, new Bgr(System.Drawing.Color.White)))
                                {
                                    CreateOutputStream(newImg.ToBitmap());
                                }
                                break;
                            case ActionType.invert: // Inverting
                                using (Image<Bgr, byte> newImg = img.Not())
                                {
                                    CreateOutputStream(newImg.ToBitmap());
                                }
                                break;
                            case ActionType.create: // Create Image
                                if (settings.SizeX > 65536)
                                {
                                    settings.SizeX = 65536;
                                }

                                if (settings.SizeY > 65536)
                                {
                                    settings.SizeY = 65536;
                                }

                                using (Image<Gray, float> newImg = new Image<Gray, float>(settings.SizeX, settings.SizeY))
                                {
                                    CreateOutputStream(newImg.ToBitmap());
                                }
                                break;
                            case ActionType.and:    // And-connect Images
                                if (InputImage1 != null && InputImage2 != null)
                                {
                                    using (Image<Bgr, byte> secondImg = new Image<Bgr, byte>(new Bitmap(InputImage2.CreateReader())))
                                    {
                                        using (Image<Bgr, byte> newImg = img.And(secondImg))
                                        {
                                            CreateOutputStream(newImg.ToBitmap());
                                        }
                                    }
                                }
                                else
                                {
                                    GuiLogMessage("Please select two images.", NotificationLevel.Error);
                                }
                                break;
                            case ActionType.or:    // Or-connect Images
                                if (InputImage1 != null && InputImage2 != null)
                                {
                                    using (Image<Bgr, byte> secondImg = new Image<Bgr, byte>(new Bitmap(InputImage2.CreateReader())))
                                    {
                                        using (Image<Bgr, byte> newImg = img.Or(secondImg))
                                        {
                                            CreateOutputStream(newImg.ToBitmap());
                                        }
                                    }
                                }
                                else
                                {
                                    GuiLogMessage("Please select two images.", NotificationLevel.Error);
                                }
                                break;
                            case ActionType.xor:    // Xor-connect Images
                                if (InputImage1 != null && InputImage2 != null)
                                {
                                    using (Image<Bgr, byte> secondImg = new Image<Bgr, byte>(new Bitmap(InputImage2.CreateReader())))
                                    {
                                        using (Image<Bgr, byte> newImg = img.Xor(secondImg))
                                        {
                                            CreateOutputStream(newImg.ToBitmap());
                                        }
                                    }
                                }
                                else
                                {
                                    GuiLogMessage("Please select two images.", NotificationLevel.Error);
                                }
                                break;
                            case ActionType.xorgray: // Xor- ImageGrayScales
                                if (InputImage1 != null && InputImage2 != null)
                                {
                                    using (Image<Gray, byte> grayImg2 = new Image<Bgr, byte>(new Bitmap(InputImage2.CreateReader())).Convert<Gray, byte>())
                                    {
                                        using (Image<Gray, byte> grayImg1 = img.Convert<Gray, byte>())
                                        {
                                            using (Image<Gray, byte> newImg = grayImg1.Xor(grayImg2))
                                            {
                                                CreateOutputStream(newImg.ToBitmap());
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    GuiLogMessage("Please select two images.", NotificationLevel.Error);
                                }
                                break;
                            case ActionType.blacknwhite:
                                blacknwhite(img);
                                break;
                        }

                        OnPropertyChanged("OutputImage");
                    }
                }
            }

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

        #region HelpFunctions

        /// <summary>
        /// Resizes bitmap b to width nWidth and height nHeight.
        /// </summary>
        /// <param name="b">The bitmap to resize.</param>
        /// <param name="nWidth">The new width.</param>
        /// <param name="nHeight">The new height.</param>
        private Bitmap ResizeBitmap(Bitmap b)
        {
            int newWidth = b.Width;
            int newHeight = b.Height;
            while (newWidth < 300 || newHeight < 300)
            {
                newWidth *= 2;
                newHeight *= 2;
            }
            Bitmap result = new Bitmap(newWidth, newHeight);
            newWidth += newWidth / (b.Width * 2 - 1);
            newHeight += newHeight / (b.Height * 2 - 1);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.SmoothingMode = SmoothingMode.None;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                RectangleF outRect = new RectangleF(0, 0, newWidth, newHeight);
                g.DrawImage(b, outRect);
            }
            return result;
        }

        /// <summary>Create output stream to display.</summary>
        /// <param name="bitmap">The bitmap to display.</param>
        private void CreateOutputStream(Bitmap bitmap, bool avoidWPFSmoothing = true)
        {
            // Avoid smoothing of WPF
            if (avoidWPFSmoothing && bitmap.Height < 300)
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

        private void CropImage()
        {
            if (InputImage1 == null)
            {
                return;
            }

            using (CStreamReader reader = InputImage1.CreateReader())
            {
                using (Bitmap bitmap = new Bitmap(reader))
                {
                    int x1 = settings.SliderX1 * bitmap.Width / 10000;
                    int x2 = bitmap.Width - settings.SliderX2 * bitmap.Width / 10000 - x1;
                    int y1 = settings.SliderY1 * bitmap.Height / 10000;
                    int y2 = bitmap.Height - settings.SliderY2 * bitmap.Height / 10000 - y1;
                    Rectangle cropRect = new Rectangle(x1, y1, x2, y2);
                    Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);

                    using (Graphics g = Graphics.FromImage(target))
                    {
                        g.DrawImage(bitmap, new Rectangle(0, 0, target.Width, target.Height),
                                         cropRect,
                                         GraphicsUnit.Pixel);
                        CreateOutputStream(target);
                    }
                }
            }
        }

        //Convert bgr Image to black and white
        private void blacknwhite(Image<Bgr, byte> img = null)
        {
            if (img == null)
            {
                if (InputImage1 == null)
                {
                    return;
                }

                img = new Image<Bgr, byte>(new Bitmap(InputImage1.CreateReader()));
            }
            using (Image<Gray, byte> blacknwhite = img.Convert<Gray, byte>().ThresholdBinary(new Gray(settings.Threshold), new Gray(255)))
            {
                CreateOutputStream(blacknwhite.ToBitmap());
            }
        }

        //Changes the contrast of an image
        private void contrast(Image<Bgr, byte> img = null)
        {
            if (img == null)
            {
                if (InputImage1 == null)
                {
                    return;
                }

                img = new Image<Bgr, byte>(new Bitmap(InputImage1.CreateReader()));
            }
            Image<Bgr, byte> constrast_img = img.Copy();
            constrast_img._GammaCorrect((double)settings.Contrast / 100); // 0.01 - 10
            CreateOutputStream(constrast_img.ToBitmap());
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

        private void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SliderX1":
                case "SliderX2":
                case "SliderY1":
                case "SliderY2":
                    CropImage();
                    OnPropertyChanged("OutputImage");
                    break;
                case "Threshold":
                    blacknwhite();
                    OnPropertyChanged("OutputImage");
                    break;
                case "Contrast":
                    contrast();
                    OnPropertyChanged("OutputImage");
                    break;
                default:
                    break;
            }
        }

        #endregion
    }

}
