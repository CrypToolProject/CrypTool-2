/*
   Copyright 2008-2022 CrypTool Team

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
using System;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Startcenter.Util
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
