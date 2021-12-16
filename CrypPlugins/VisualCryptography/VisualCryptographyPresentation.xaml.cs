using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace CrypTool.Plugins.VisualCryptography
{
    /// <summary>
    /// Interaktionslogik für VisualCryptographyPresentation.xaml
    /// </summary>
    public partial class VisualCryptographyPresentation : UserControl
    {
        private byte[] _image1;
        private byte[] _image2;
        private int _width;
        private int _height;

        public VisualCryptographyPresentation()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Set the two images which will be shown in the presentation
        /// </summary>
        /// <param name="encrypted_images"></param>
        public void SetImages((byte[] image1, byte[] image2, int width, int height) encrypted_images)
        {
            _image1 = encrypted_images.image1;
            _image2 = encrypted_images.image2;
            _width = encrypted_images.width;
            _height = encrypted_images.height;
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    ScrollBar.Value = 0;
                }
                catch (Exception)
                {
                    //do noting
                }
            }, null);
        }

        /// <summary>
        /// Move both visualized pictures based on the scrollbar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                if (_image1 == null || _image2 == null)
                {
                    return;
                }
                int offset = (int)(e.NewValue * _height);
                UpdateImage(offset);
            }
            catch (Exception)
            {
                //do noting
            }
        }

        /// <summary>
        /// Updates visualized image
        /// </summary>
        /// <param name="offset"></param>
        public void UpdateImage(int offset)
        {
            try
            {
                (byte[] image, int width, int height) merged = VisualCryptography.MergeImages(_image1, _image2, _width, _height, offset);
                System.Drawing.Bitmap bitmap = VisualCryptography.CreateBitmap(merged.image, merged.width, merged.height);

                ImageSource imageSource;
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    BmpBitmapDecoder bitmapDecoder = new BmpBitmapDecoder(memoryStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                    imageSource = bitmapDecoder.Frames[0];
                }

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    try
                    {
                        ImageSourceConverter imageSourceConverter = new ImageSourceConverter();
                        Image.Source = imageSource;
                    }
                    catch (Exception)
                    {
                        //do noting
                    }
                }, null);
            }
            catch (Exception)
            {
                //do noting
            }
        }
    }
}
