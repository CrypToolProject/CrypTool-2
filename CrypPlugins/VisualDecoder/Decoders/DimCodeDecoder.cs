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


using CrypTool.Plugins.VisualDecoder.Model;
using System;
using System.Drawing;
using System.IO;

namespace CrypTool.Plugins.VisualDecoder.Decoders
{
    /// <summary>
    /// base class for all dimcode decoder
    /// </summary>
    internal class DimCodeDecoder
    {
        private readonly ImageConverter imageConverter = new ImageConverter();

        protected readonly VisualDecoder Caller;
        protected Pen MarkingPen;


        protected DimCodeDecoder(VisualDecoder caller)
        {
            Caller = caller;
            MarkingPen = new Pen(Color.FromArgb(190, Color.Blue), 10.0f);
        }


        public virtual DimCodeDecoderItem Decode(byte[] input)
        {
            throw new NotImplementedException();
        }

        #region helper
        public byte[] ImageToByteArray(System.Drawing.Image imageIn)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                byte[] image = ms.ToArray();
                return image;
            }
        }

        public Bitmap ByteArrayToImage(byte[] byteArrayIn)
        {
            Image img = (Image)imageConverter.ConvertFrom(byteArrayIn);
            return new Bitmap(img);
        }
        #endregion helper
    }
}
