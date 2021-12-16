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
            BitmapImage bmpImage = new BitmapImage(file);
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmpImage));
            using (MemoryStream stream = new MemoryStream())
            {
                encoder.Save(stream);
                data = stream.ToArray();
            }

            MemoryStream stream2 = new MemoryStream(data);
            PngBitmapDecoder decoder = new PngBitmapDecoder(stream2, BitmapCreateOptions.None, BitmapCacheOption.Default);
            return decoder.Frames.First();
        }
    }
}
