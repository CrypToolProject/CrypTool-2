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
using System.Collections.Generic;
using System.Drawing;
using ZXing;

namespace CrypTool.Plugins.VisualDecoder.Decoders
{
    /// <summary>
    /// The ZXingDecoder is a common decoder class for all the dimCodes that are supported by the ZXing Libery.
    /// </summary>
    internal class ZXingDecoder : DimCodeDecoder
    {

        private readonly List<BarcodeFormat> list;
        private readonly string codeType;

        public ZXingDecoder(VisualDecoder caller, BarcodeFormat codeType) : base(caller)
        {
            list = new List<BarcodeFormat> { codeType };
            this.codeType = codeType.ToString();
        }

        public override DimCodeDecoderItem Decode(byte[] input)
        {
            Bitmap image = ByteArrayToImage(input);

            BarcodeReader barcodeReader = new BarcodeReader
            {
                AutoRotate = true,
                PossibleFormats = list,
                PureBarcode = false,
                TryHarder = true
            };

            Result result = barcodeReader.Decode(image);  // decode barcode

            if (result != null)
            {
                image = DrawRectangleZXing(image, result);

                return new DimCodeDecoderItem
                {
                    BitmapWithMarkedCode = ImageToByteArray(image),
                    CodePayload = result.Text,
                    CodeType = codeType
                };
            }
            return null;
        }


        /// <summary>
        /// Methode for a general rectangel draw by using the result of the ZXing Libery
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        // https://zxingnet.svn.codeplex.com/svn/trunk/Clients/WindowsFormsDemo/WindowsFormsDemoForm.cs
        protected Bitmap DrawRectangleZXing(Bitmap bitmap, Result result)
        {
            Rectangle rect = new Rectangle((int)result.ResultPoints[0].X, (int)result.ResultPoints[0].Y, 1, 1);
            foreach (ResultPoint point in result.ResultPoints) // extend rectangle with each point if necessary 
            {
                if (point.X < rect.Left)
                {
                    rect = new Rectangle((int)point.X, rect.Y, rect.Width + rect.X - (int)point.X, rect.Height);
                }
                if (point.X > rect.Right)
                {
                    rect = new Rectangle(rect.X, rect.Y, rect.Width + (int)point.X - rect.X, rect.Height);
                }
                if (point.Y < rect.Top)
                {
                    rect = new Rectangle(rect.X, (int)point.Y, rect.Width, rect.Height + rect.Y - (int)point.Y);
                }
                if (point.Y > rect.Bottom)
                {
                    rect = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height + (int)point.Y - rect.Y);
                }
            }
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.DrawRectangle(MarkingPen, rect);
            }
            return bitmap;
        }
    }

}
