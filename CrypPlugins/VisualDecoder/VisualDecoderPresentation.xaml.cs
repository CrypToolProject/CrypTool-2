/*
    Copyright 2013 Christopher Konze, University of Kassel
 
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

using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Windows.Media.Color;

namespace CrypTool.Plugins.VisualDecoder
{
    /// <summary>
    /// Interaktionslogik für DimCodeEncoderPresentation.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("VisualDecoder.Properties.Resources")]
    public partial class VisualDecoderPresentation : UserControl
    {
        public VisualDecoderPresentation()
        {
            InitializeComponent();
        }

        /// <summary>
        /// simple image setter
        /// </summary>
        public void SetImages(byte[] img)
        {
            BitmapDecoder decoder = BitmapDecoder.Create(new MemoryStream(img),
                                               BitmapCreateOptions.PreservePixelFormat,
                                               BitmapCacheOption.None);
            if (decoder.Frames.Count > 0)
            {
                Image.Source = decoder.Frames[0];
            }
        }

        /// <summary>
        /// sets the payload and the detected codetype and change the color of the presentation
        /// </summary>
        public void SetData(string payload, string codetype)
        {
            Payload.Text = payload;
            PayloadLable.Visibility = Visibility.Visible;
            CodeType.Content = codetype;
            CodeTypeLable.Visibility = Visibility.Visible;

            BodyBorder.Background = new SolidColorBrush(payload.Length == 0
                                                ? Color.FromArgb(0xAF, 0xFF, 0xD4, 0xC1)
                                                : Color.FromArgb(0xAF, 0xE2, 0xFF, 0xCE));

            HeaderBorder.Background = new SolidColorBrush(payload.Length == 0
                                                ? Color.FromArgb(0xFF, 0xE5, 0x6B, 0x00)
                                                : Color.FromArgb(0xFF, 0x47, 0x93, 0x08));

        }

        /// <summary>
        /// reset the presentation 
        /// </summary>
        public void ClearPresentation()
        {
            Payload.Text = "";
            PayloadLable.Visibility = Visibility.Hidden;
            CodeType.Content = "";
            CodeTypeLable.Visibility = Visibility.Hidden;
            BodyBorder.Background = new SolidColorBrush(Color.FromArgb(0xAF, 0xFF, 0xD4, 0xC1));
            HeaderBorder.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xE5, 0x6B, 0x00));
        }
    }
}
