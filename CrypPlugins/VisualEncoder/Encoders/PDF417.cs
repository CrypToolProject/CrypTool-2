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
    internal class PDF417 : DimCodeEncoder
    {
        #region legend Strings
        private readonly LegendItem rowIndiLegend = new LegendItem
        {
            ColorBlack = Color.Blue,
            ColorWhite = Color.LightBlue,
            LableValue = Resources.PDF417_ROWID_LABLE,
            DiscValue = Resources.PDF417_ROWID_DISC

        };

        private readonly LegendItem startEndPatternLegend = new LegendItem
        {
            ColorBlack = Color.Green,
            ColorWhite = Color.LightGreen,
            LableValue = Resources.PDF417_SEPAT_LABLE,
            DiscValue = Resources.PDF417_SEPAT_DISC
        };

        #endregion

        public PDF417(VisualEncoder caller) : base(caller) {/*empty*/}

        protected override Image GenerateBitmap(string input, VisualEncoderSettings settings)
        {

            BarcodeWriter barcodeWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.PDF_417,
                Options = new EncodingOptions
                {
                    Margin = 0,
                    Height = 110,
                    Width = 110
                }
            };

            string payload = input;
            return barcodeWriter.Write(payload);
        }

        protected override List<LegendItem> GetLegend(string input, VisualEncoderSettings settings)
        {
            List<LegendItem> legend = new List<LegendItem> { startEndPatternLegend, rowIndiLegend };
            return legend;
        }


        protected override Image GeneratePresentationBitmap(Image input, VisualEncoderSettings settings)
        {
            Bitmap bitmap = new Bitmap(input);
            LockBitmap lockBitmap = new LockBitmap(bitmap);
            lockBitmap.LockBits();
            int x = 0;
            int y = lockBitmap.Height / 2;

            #region find borders

            // set x  to the beginning od the code (left first black bar)
            while (lockBitmap.GetPixel(x, y).R != Color.Black.R)
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

            //set y to the beginning of the code (upside of the code)
            while (lockBitmap.GetPixel(x, y - 1).R == Color.Black.R)
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

            int xleft = x;
            x = lockBitmap.Width - 1;

            // set x to the end of the code
            while (lockBitmap.GetPixel(x, y).R != Color.Black.R)
            {
                if (x > 1)
                {
                    x--;
                }
                else //avoid endless search
                {   //if we found no bar end, we stop here
                    return bitmap;
                }
            }

            int xRight = x;

            lockBitmap.UnlockBits();
            int barHight = CalcBarHight(bitmap, xleft);
            lockBitmap.LockBits();
            #endregion
            int barcount = 0;
            bool isOnBlackBar = false;

            #region color left start pattern and row indicator
            for (; barcount < 17; xleft++)
            {
                if (lockBitmap.GetPixel(xleft, y).R == Color.Black.R)
                {
                    if (!isOnBlackBar)
                    {
                        barcount++;
                        isOnBlackBar = true;
                    }
                }
                else
                {
                    if (isOnBlackBar)
                    {
                        barcount++;
                        isOnBlackBar = false;
                    }

                }

                if (barcount >= 0 && barcount <= 7)
                {
                    for (int yFrom = y; yFrom <= barHight; yFrom++)
                    {
                        lockBitmap.SetPixel(xleft, yFrom, lockBitmap.GetPixel(xleft, yFrom).R == Color.Black.R ? startEndPatternLegend.ColorBlack
                                                                                                                : startEndPatternLegend.ColorWhite);
                    }
                }
                else if (barcount > 8 && barcount < 17)
                {
                    for (int yFrom = y; yFrom <= barHight; yFrom++)
                    {
                        lockBitmap.SetPixel(xleft, yFrom, lockBitmap.GetPixel(xleft, yFrom).R == Color.Black.R ? rowIndiLegend.ColorBlack
                                                                                                                : rowIndiLegend.ColorWhite);
                    }
                }
            }
            #endregion
            #region color right stop pattern and row indicator
            barcount = 0;
            isOnBlackBar = false;

            for (; barcount < 18; xRight--)
            {
                if (lockBitmap.GetPixel(xRight, y).R == Color.Black.R)
                {
                    if (!isOnBlackBar)
                    {
                        barcount++;
                        isOnBlackBar = true;
                    }
                }
                else
                {
                    if (isOnBlackBar)
                    {
                        barcount++;
                        isOnBlackBar = false;
                    }

                }

                if (barcount >= 0 && barcount <= 9)
                {
                    for (int yFrom = y; yFrom <= barHight; yFrom++)
                    {
                        lockBitmap.SetPixel(xRight, yFrom, lockBitmap.GetPixel(xRight, yFrom).R == Color.Black.R ? startEndPatternLegend.ColorBlack
                                                                                                                : startEndPatternLegend.ColorWhite);
                    }
                }
                else if (barcount > 9 && barcount < 18)
                {
                    for (int yFrom = y; yFrom <= barHight; yFrom++)
                    {
                        lockBitmap.SetPixel(xRight, yFrom, lockBitmap.GetPixel(xRight, yFrom).R == Color.Black.R ? rowIndiLegend.ColorBlack
                                                                                                                : rowIndiLegend.ColorWhite);
                    }
                }
            }
            #endregion
            lockBitmap.UnlockBits();

            return bitmap;
        }
    }
}
