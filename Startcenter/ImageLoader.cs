using System;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Startcenter
{
    internal static class ImageLoader
    {
        public static ImageSource LoadImage(Uri file)
        {
            byte[] data;
            var bmpImage = new BitmapImage(file);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmpImage));
            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                data = stream.ToArray();
            }

            var stream2 = new MemoryStream(data);
            var decoder = new PngBitmapDecoder(stream2, BitmapCreateOptions.None, BitmapCacheOption.Default);
            return decoder.Frames.First();
        }
    }
}
