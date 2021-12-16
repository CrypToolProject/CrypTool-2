/*
   Copyright 2014 Nils Rehwald

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
  
   For invisible Watermarks, an existing Project has been used:
   Original Project can be found at https://code.google.com/p/dct-watermark/
   Ported to C# to be used within CrypTool 2 by Nils Rehwald
   Thanks to cgaffa, ZXing and everyone else who worked on the original Project for making the original Java sources available publicly
   Thanks to Nils Kopal for Support and Bugfixing 
*/

using CrypTool.PluginBase;
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
using net.watermark;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using Image = System.Drawing.Image;

namespace CrypTool.Plugins.WatermarkCreator
{

    [Author("Nils Rehwald", "nilsrehwald@gmail.com", "Uni Kassel", "http://www.uni-kassel.de/eecs/fachgebiete/ais/")]
    [PluginInfo("WatermarkCreator.Properties.Resources", "PluginCaption", "PluginTooltip", "WatermarkCreator/userdoc.xml", "WatermarkCreator/icon.png")]
    [ComponentCategory(ComponentCategory.Steganography)]
    public class WatermarkCreator : ICrypComponent
    {
        #region Private Variables and Constructor

        public WatermarkCreator()
        {
            _settings = new WatermarkCreatorSettings();
            _settings.UpdateTaskPaneVisibility();
            _settings.PropertyChanged += new PropertyChangedEventHandler(settings_PropertyChanged);
        }

        private readonly WatermarkCreatorSettings _settings;

        //Default values for invisible watermarking
        private int _textSize = 0;
        private string _font = "arial";
        private int _location = 1;
        private double _opacity = 1.0;
        private int _boxSize = 10;
        private readonly int _errorCorrection = 0;
        private long _s1 = 19;
        private long _s2 = 24;
        private double _locationPercentage = 0.05;

        private bool _stopped;
        private Watermark water;

        //Selection of fonts
        private readonly string[] fontsChosen = new string[]
        {
            "Aharoni",
            "Andalus", "Arabic Typesetting", "Arial", "Arial Black", "Calibri", "Buxton Sketch",
            "Cambria Math", "Comic Sans MS", "DFKai-SB", "Franklin Gothic Medium", "Lucida Console",
            "Simplified Arabic", "SketchFlow Print", "Symbol", "Times New Roman", "Traditional Arabic",
            "Webdings", "Wingdings"
        };

        private enum Commands { EmbVisText, EmbInvisText, ExtInvisText };

        private enum Location { Top, Bottom, Other };

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "ImageCaption", "ImageTooltip")]
        public ICrypToolStream InputPicture
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "WatermarkCaption", "WatermarkTooltip")]
        public string Watermark
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "WatermarkImageCaption", "WatermarkImageTooltip")]
        public ICrypToolStream OutputPicture
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "TextCaption", "TextTooltip")]
        public string EmbeddedText
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members

        public ISettings Settings => _settings;

        public UserControl Presentation => null;

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            _stopped = false;
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);

            if (InputPicture == null)
            {
                GuiLogMessage("NoPictureError", NotificationLevel.Error);
                return;
            }
            switch (_settings.ModificationType)
            {
                case (int)Commands.EmbVisText: //Embed Visible Text

                    GetVisVariables();

                    if (Watermark == null)
                    {
                        GuiLogMessage("NoTextError", NotificationLevel.Error);
                        return;
                    }

                    WmVisibleText();

                    OnPropertyChanged("OutputPicture");
                    ProgressChanged(1, 1);
                    break;

                case (int)Commands.EmbInvisText: //Embed Invisible Text

                    GetInvisVariables();

                    if (Watermark == null)
                    {
                        GuiLogMessage("NoTextError", NotificationLevel.Error);
                        return;
                    }

                    CreateInvisibleWatermark();

                    OnPropertyChanged("OutputPicture");
                    ProgressChanged(1, 1);

                    break;

                case (int)Commands.ExtInvisText: //Detect Invisible Text
                    GetInvisVariables();

                    EmbeddedText = DetectInvisibleWatermark();

                    OnPropertyChanged("EmbeddedText");
                    ProgressChanged(1, 1);

                    break;
            }
        }



        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
            water = null;
            _stopped = true;
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
            _stopped = true;
            if (water != null)
            {
                //Stop embedding/extracting process for invisible watermark which can take quite some time in case of larce pictures
                water.Stop();
            }
            water = null;
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

        private void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                _settings.UpdateTaskPaneVisibility();
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Exception during settings_PropertyChanged: {0}", ex), NotificationLevel.Error);
            }
        }

        #endregion

        #region Helpers

        private void WmVisibleText() //Embed visible text
        {
            using (CStreamReader reader = InputPicture.CreateReader())
            {
                using (Bitmap bitmap = new Bitmap(reader))
                {
                    Bitmap image = PaletteToRGB(bitmap);
                    Image photo = image;
                    int width = photo.Width;
                    int height = photo.Height;

                    Bitmap bitmapmPhoto = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                    bitmapmPhoto.SetResolution(72, 72);
                    Graphics graphicPhoto = Graphics.FromImage(bitmapmPhoto);

                    graphicPhoto.SmoothingMode = SmoothingMode.AntiAlias;
                    graphicPhoto.DrawImage(photo, new Rectangle(0, 0, width, height), 0, 0, width, height, GraphicsUnit.Pixel);
                    Font font = null;
                    SizeF size = new SizeF();

                    for (int i = _textSize; i > 3; i--) //Test which is the largest possible fontsize to be used
                    {
                        if (_stopped)
                        {
                            return;
                        }
                        font = new Font(_font, i, FontStyle.Bold);
                        size = graphicPhoto.MeasureString(Watermark, font);
                        if ((ushort)size.Width < (ushort)width) //Text fits into image
                        {
                            break;
                        }
                    }

                    int yPixlesFromBottom = 0;
                    switch (_location)
                    {
                        case (int)Location.Bottom:
                            yPixlesFromBottom = (int)(height * .05);
                            break;
                        case (int)Location.Top:
                            yPixlesFromBottom = (int)(height * .95);
                            break;
                        case (int)Location.Other:
                            yPixlesFromBottom = (int)(height * _locationPercentage);
                            break;
                    }
                    float yPosFromBottom = ((height - yPixlesFromBottom) - (size.Height / 2));
                    float xCenterOfImg = (width / 2f);
                    StringFormat strFormat = new StringFormat
                    {
                        Alignment = StringAlignment.Center
                    };
                    SolidBrush semiTransBrush2 = new SolidBrush(Color.FromArgb(153, 0, 0, 0));
                    graphicPhoto.DrawString(Watermark, font, semiTransBrush2, new PointF(xCenterOfImg + 1, yPosFromBottom + 1), strFormat);
                    SolidBrush semiTransBrush = new SolidBrush(Color.FromArgb(153, 255, 255, 255));
                    graphicPhoto.DrawString(Watermark, font, semiTransBrush, new PointF(xCenterOfImg, yPosFromBottom), strFormat);

                    CreateOutputStream(bitmapmPhoto);
                }
            }
        }

        private void CreateInvisibleWatermark() //Embed invisible Watermark
        {
            water = new Watermark(_boxSize, _errorCorrection, _opacity, _s1, _s2);
            using (CStreamReader reader = InputPicture.CreateReader())
            {
                using (Bitmap bitmap = new Bitmap(reader))
                {
                    if (_stopped)
                    {
                        return;
                    }
                    water.embed(bitmap, Watermark);
                    CreateOutputStream(bitmap);
                }
            }
        }

        private string DetectInvisibleWatermark() //Extract invisible watermark
        {
            water = new Watermark(_boxSize, _errorCorrection, _opacity, _s1, _s2);
            using (CStreamReader reader = InputPicture.CreateReader())
            {
                using (Bitmap bitmap = new Bitmap(reader))
                {
                    if (_stopped)
                    {
                        return "";
                    }
                    return water.extractText(bitmap);
                }
            }
        }

        //Some functions for creating the image output / doing manipulations
        private void CreateOutputStream(Bitmap bitmap)
        {
            ImageFormat format = ImageFormat.Bmp;

            Bitmap saveableBitmap = CopyBitmap(bitmap, format);

            MemoryStream outputStream = new MemoryStream();
            saveableBitmap.Save(outputStream, format);
            saveableBitmap.Dispose();
            bitmap.Dispose();

            OutputPicture = new CStreamWriter(outputStream.GetBuffer());
        }

        private Bitmap CopyBitmap(Bitmap bitmap, ImageFormat format)
        {
            MemoryStream buffer = new MemoryStream();
            bitmap.Save(buffer, format);
            Bitmap saveableBitmap = (Bitmap)Image.FromStream(buffer);
            return saveableBitmap;
        }

        private Bitmap PaletteToRGB(Bitmap original)
        {
            original = CopyBitmap(original, ImageFormat.Bmp);
            Bitmap image = new Bitmap(original.Width, original.Height, PixelFormat.Format24bppRgb);
            Graphics graphics = Graphics.FromImage(image);
            graphics.DrawImage(original, 0, 0, original.Width, original.Height);
            graphics.Dispose();
            original.Dispose();
            return image;
        }

        private void GetInvisVariables() //Get user defined settings for invisible watermarking
        {
            _boxSize = _settings.BoxSize;
            //_errorCorrection = _settings.ErrorCorrection;
            _opacity = _settings.Opacity;
            if (_opacity > 1000.0)
            {
                _opacity = 1000;
            }
            if (_opacity <= 0.0)
            {
                _opacity = 0.0;
            }
            else
            {
                _opacity = _opacity / 1000;
            }
            if (_opacity > 1) //Just in case
            {
                _opacity = 1.0;
            }
            _s1 = _settings.Seed1;
            _s2 = _settings.Seed2;
        }

        private void GetVisVariables() //Get user defined settings for visible watermarking
        {
            _textSize = _settings.TextSizeMax < 3 ? 12 : _settings.TextSizeMax;
            _location = _settings.WatermarkLocation;
            _locationPercentage = (double)_settings.LocationPercentage / 100;
            string[] fonts = GetFonts();
            _font = fontsChosen[_settings.FontType];
            if (!fonts.Contains(_font))
            {
                GuiLogMessage(_font + " was not found on this Computer. Using Arial instead", NotificationLevel.Warning);
                _font = "Arial";
            }
        }

        private string[] GetFonts() //Get fonts installed on the user's system to check whether chosen font is available
        {
            InstalledFontCollection installedFontCollection = new InstalledFontCollection();
            FontFamily[] fontFamilies = installedFontCollection.Families;
            int count = fontFamilies.Length;
            string[] fonts = new string[count];
            for (int j = 0; j < count; ++j)
            {
                fonts[j] = fontFamilies[j].Name;
            }
            return fonts;
        }

        #endregion

    }
}
