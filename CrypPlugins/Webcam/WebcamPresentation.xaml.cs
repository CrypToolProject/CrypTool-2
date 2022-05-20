/*
   Copyright 2019 CrypTool 2 Team <ct2contact@CrypTool.org>

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
using CrypTool.Plugins.Webcam;
using Emgu.CV;
using System;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Encoder = System.Drawing.Imaging.Encoder;

namespace Webcam
{

    /// <summary>
    /// Interaktionslogik für WebCamPresentation.xaml
    /// </summary>
    public partial class WebcamPresentation : UserControl
    {
        private Capture _capture = null;
        private ImageCodecInfo _jpgEncoder;
        private Encoder _encoder;
        public WebcamSettings WebcamSettings  { get;set;}

        public WebcamPresentation()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Starts the camera with the given id 
        /// </summary>
        /// <param name="device"></param>
        public void Start(int id = 0)
        {
            _capture = new Capture(id);
            _jpgEncoder = GetEncoder(ImageFormat.Jpeg);
            _encoder = Encoder.Quality;
        }

        /// <summary>
        /// Updates the ui camera image and returns the image as
        /// jpeg in a byte array
        /// </summary>
        /// <returns></returns>
        public byte[] CaptureImage()
        {
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    _capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Brightness, WebcamSettings.Brightness);
                    _capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Contrast, WebcamSettings.Contrast);
                    _capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Sharpness, WebcamSettings.Sharpness);
                    System.Drawing.Bitmap bitmap = _capture.QueryFrame().Bitmap;
                    EncoderParameters encoderParameters = new EncoderParameters(1);
                    EncoderParameter encoderParameter = new EncoderParameter(_encoder, WebcamSettings.ImageQuality);
                    encoderParameters.Param[0] = encoderParameter;
                    bitmap.Save(stream, _jpgEncoder, encoderParameters);
                    stream.Position = 0;
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = stream;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                    Image.Source = bitmapImage;
                    return stream.ToArray();
                }
            }
            catch (Exception)
            {
                //wtf?
            }
            return null;
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        /// <summary>
        /// Stops and disposes the camera
        /// </summary>
        public void Stop()
        {
            if (_capture != null)
            {
                _capture.Dispose();
            }
            _capture = null;
        }

    }
}
