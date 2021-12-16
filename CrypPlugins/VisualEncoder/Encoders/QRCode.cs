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
using System.Drawing;
using VisualEncoder.Properties;
using ZXing;
using ZXing.Common;

namespace CrypTool.Plugins.VisualEncoder.Encoders
{
    internal class QRCode : DimCodeEncoder
    {
        #region legend Strings

        private readonly LegendItem alignmentLegend = new LegendItem
        {
            ColorBlack = Color.Blue,
            ColorWhite = Color.LightBlue,
            LableValue = Resources.QR_ALIG_LABLE,
            DiscValue = Resources.QR_ALIG_DISC,
        };

        private readonly LegendItem formatAreaLegend = new LegendItem
        {
            ColorBlack = Color.Green,
            ColorWhite = Color.LightGreen,
            LableValue = Resources.QR_FORMAT_LABLE,
            DiscValue = Resources.QR_FORMAT_DISC,
        };

        private readonly LegendItem versionAreaLegend = new LegendItem
        {
            ColorBlack = Color.Red,
            ColorWhite = Color.LightSalmon,
            LableValue = Resources.QR_VERSION_LABLE,
            DiscValue = Resources.QR_VERSION_DISC,
        };
        private readonly LegendItem dataLegend = new LegendItem
        {
            ColorBlack = Color.Black,
            ColorWhite = Color.White,
            LableValue = Resources.QR_NORMAL_LABLE,
            DiscValue = Resources.QR_NORMAL_DISC,
        };

        #endregion

        public QRCode(VisualEncoder caller) : base(caller) {/*empty*/}

        protected override Image GenerateBitmap(string input, VisualEncoderSettings settings)
        {
            BarcodeWriter barcodeWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new EncodingOptions
                {
                    Margin = 1,
                    Height = 300,
                    Width = 300
                }
            };
            string payload = input;
            return barcodeWriter.Write(payload);
        }


        protected override List<LegendItem> GetLegend(string input, VisualEncoderSettings settings)
        {
            List<LegendItem> legend = new List<LegendItem> { alignmentLegend, dataLegend, versionAreaLegend, formatAreaLegend };
            return legend;
        }


        protected override Image GeneratePresentationBitmap(Image input, VisualEncoderSettings settings)
        {
            Bitmap bitmap = new Bitmap(input);
            LockBitmap lockBitmap = new LockBitmap(bitmap);
            #region find borders and positions
            lockBitmap.LockBits();
            int x = 0;
            int y = 0;

            // find upper left corner
            while (lockBitmap.GetPixel(x, y).R != Color.Black.R)
            {
                if (x < lockBitmap.Width)
                {
                    x++;
                    y++;
                }
                else //avoid endless search
                {   //if we found no bar end, we stop here
                    return bitmap;
                }
            }
            int leftX = x;
            int upperY = y;

            // size of aligment Lable
            while (lockBitmap.GetPixel(x, y).R == Color.Black.R)
            {
                if (x < lockBitmap.Width)
                {
                    x++;
                }
                else //avoid endless search
                {   //if we found no bar end, we stop here
                    return bitmap;
                }
            }
            int aligmentSize = x - leftX;

            x = lockBitmap.Width - 1;
            //find upper right corner
            while (lockBitmap.GetPixel(x, y).R != Color.Black.R)
            {
                if (x > 0)
                {
                    x--;
                }
                else //avoid endless search
                {   //if we found no bar end, we stop here
                    return bitmap;
                }
            }
            int rightX = x;

            x = leftX;
            y = lockBitmap.Height - 1;

            //find downer y
            while (lockBitmap.GetPixel(x, y).R != Color.Black.R)
            {
                if (y > 0)
                {
                    y--;
                }
                else //avoid endless search
                {   //if we found no bar end, we stop here
                    return bitmap;
                }
            }

            int downerY = y;
            int dotSize = aligmentSize / 7;


            lockBitmap.UnlockBits();
            #endregion

            #region format area

            //upper right format area
            bitmap = FillArea(rightX - aligmentSize - dotSize + 1,
                              rightX,
                              upperY + aligmentSize + dotSize,
                              upperY + aligmentSize + 2 * dotSize - 1,
                              bitmap, formatAreaLegend.ColorBlack, formatAreaLegend.ColorWhite);

            //downer left format area
            bitmap = FillArea(leftX + aligmentSize + dotSize,
                              leftX + aligmentSize + 2 * dotSize - 1,
                              downerY - aligmentSize - dotSize + 1,
                              downerY,
                              bitmap, formatAreaLegend.ColorBlack, formatAreaLegend.ColorWhite);

            //upper left horizontal format area
            bitmap = FillArea(leftX + aligmentSize + dotSize,
                            leftX + aligmentSize + 2 * dotSize - 1,
                            upperY,
                            upperY + aligmentSize + 2 * dotSize - 1,
                            bitmap, formatAreaLegend.ColorBlack, formatAreaLegend.ColorWhite);

            //upper left vertical format area
            bitmap = FillArea(leftX,
                            leftX + aligmentSize + dotSize,
                            upperY + aligmentSize + dotSize,
                            upperY + aligmentSize + 2 * dotSize - 1,
                            bitmap, formatAreaLegend.ColorBlack, formatAreaLegend.ColorWhite);
            #endregion
            #region version area

            //upper right version area
            bitmap = FillArea(rightX - aligmentSize - 4 * dotSize + 1,
                              rightX - aligmentSize - dotSize,
                              upperY,
                               upperY + aligmentSize - dotSize - 1,
                              bitmap, versionAreaLegend.ColorBlack, versionAreaLegend.ColorWhite);

            //downer left version area 
            bitmap = FillArea(leftX,
                              leftX + aligmentSize - dotSize - 1,
                              downerY - aligmentSize - 4 * dotSize + 1,
                              downerY - aligmentSize - dotSize,
                              bitmap, versionAreaLegend.ColorBlack, versionAreaLegend.ColorWhite);
            #endregion
            #region aligment pattern
            //upperleft aligment pattern
            bitmap = FillArea(leftX,
                              leftX + aligmentSize,
                              upperY,
                              upperY + aligmentSize,
                              bitmap, alignmentLegend.ColorBlack, alignmentLegend.ColorWhite);

            //upperright aligment pattern
            bitmap = FillArea(rightX - aligmentSize,
                              rightX, upperY,
                              upperY + aligmentSize,
                              bitmap, alignmentLegend.ColorBlack, alignmentLegend.ColorWhite);

            //downerleft aligment pattern
            bitmap = FillArea(leftX,
                              leftX + aligmentSize,
                              downerY - aligmentSize,
                              downerY, bitmap, alignmentLegend.ColorBlack, alignmentLegend.ColorWhite);

            //horizontal aligment sync pattern
            bitmap = FillArea(leftX + aligmentSize + dotSize,
                              rightX - aligmentSize - dotSize,
                              upperY + aligmentSize - dotSize,
                              upperY + aligmentSize - 1,
                              bitmap, alignmentLegend.ColorBlack, alignmentLegend.ColorWhite);

            //vertical aligment sync pattern
            bitmap = FillArea(leftX + aligmentSize - dotSize,
                             leftX + aligmentSize - 1,
                             upperY + aligmentSize + dotSize,
                             downerY - aligmentSize - dotSize,
                             bitmap, alignmentLegend.ColorBlack, alignmentLegend.ColorWhite);
            #endregion
            return bitmap;
        }
    }
}
