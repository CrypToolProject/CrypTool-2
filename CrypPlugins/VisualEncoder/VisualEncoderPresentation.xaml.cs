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

using CrypTool.Plugins.VisualEncoder.Model;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;

namespace CrypTool.Plugins.VisualEncoder
{
    /// <summary>
    /// Interaktionslogik für DimCodeEncoderPresentation.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("VisualEncoder.Properties.Resources")]
    public partial class VisualEncoderPresentation : UserControl
    {

        public VisualEncoderPresentation()
        {
            InitializeComponent();
        }

        #region setter

        public void SetImages(byte[] explaindImg, byte[] pureImg)
        {
            BitmapDecoder explImageDecoder = BitmapDecoder.Create(new MemoryStream(explaindImg),
                                            BitmapCreateOptions.PreservePixelFormat,
                                            BitmapCacheOption.None);

            BitmapDecoder pureImageDecoder = BitmapDecoder.Create(new MemoryStream(pureImg),
                                            BitmapCreateOptions.PreservePixelFormat,
                                            BitmapCacheOption.None);

            if (explImageDecoder != null && explImageDecoder.Frames.Count > 0)
            {
                ExplImage.Source = explImageDecoder.Frames[0];
            }
            if (pureImageDecoder != null && pureImageDecoder.Frames.Count > 0)
            {
                PureImage.Source = pureImageDecoder.Frames[0];
            }
            UpdateImage();
        }

        /// <summary>
        /// updates the legend with the new legend
        /// </summary>
        /// <param name="legend"></param>
        public void SetList(List<LegendItem> legend)
        {
            legend1.Visibility = Visibility.Hidden;
            legend2.Visibility = Visibility.Hidden;
            legend3.Visibility = Visibility.Hidden;
            legend4.Visibility = Visibility.Hidden;

            if (legend.Count >= 1)
            {
                legend1.Visibility = Visibility.Visible;
                lable1.Content = legend[0].LableValue;
                disc1.Text = legend[0].DiscValue;
                ellipse1.Fill = ContvertColorToBrush(legend[0].ColorBlack);
            }
            if (legend.Count >= 2)
            {
                legend2.Visibility = Visibility.Visible;
                lable2.Content = legend[1].LableValue;
                disc2.Text = legend[1].DiscValue;
                ellipse2.Fill = ContvertColorToBrush(legend[1].ColorBlack);
            }
            if (legend.Count >= 3)
            {
                legend3.Visibility = Visibility.Visible;
                lable3.Content = legend[2].LableValue;
                disc3.Text = legend[2].DiscValue;
                ellipse3.Fill = ContvertColorToBrush(legend[2].ColorBlack);
            }
            if (legend.Count >= 4)
            {
                legend4.Visibility = Visibility.Visible;
                lable4.Content = legend[3].LableValue;
                disc4.Text = legend[3].DiscValue;
                ellipse4.Fill = ContvertColorToBrush(legend[3].ColorBlack);
            }
        }

        #endregion

        #region helper

        private static SolidColorBrush ContvertColorToBrush(Color colorValue)
        {
            return new SolidColorBrush(System.Windows.Media.Color.FromArgb(colorValue.A,
                                                                           colorValue.R,
                                                                           colorValue.G,
                                                                           colorValue.B));
        }

        private void Explain_Expanded(object sender, RoutedEventArgs e)
        {
            panel.Width = 565;
            UpdateImage();
        }

        private void Explain_Collapsed(object sender, RoutedEventArgs e)
        {
            panel.Width = 365;
            UpdateImage();
        }

        private void UpdateImage()
        {
            Image.Source = Explain.IsExpanded ? ExplImage.Source : PureImage.Source;
            Image.Width = Explain.IsExpanded ? ExplImage.Width : PureImage.Width;
            Image.Height = Explain.IsExpanded ? ExplImage.Height : PureImage.Height;
        }
        #endregion 
    }
}
