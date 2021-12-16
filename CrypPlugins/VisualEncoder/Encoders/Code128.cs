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

using CrypTool.PluginBase;
using CrypTool.Plugins.VisualEncoder.Model;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using VisualEncoder.Properties;
using ZXing;
using ZXing.Common;

namespace CrypTool.Plugins.VisualEncoder.Encoders
{
    internal class Code128 : DimCodeEncoder
    {
        #region legend Strings

        private readonly LegendItem startEndLegend = new LegendItem
        {
            ColorBlack = Color.Green,
            ColorWhite = Color.LightGreen,
            LableValue = Resources.C128_STARTEND_LABLE,
            DiscValue = Resources.C128_STARTEND_DISC

        };

        private readonly LegendItem ivcLegend = new LegendItem
        {
            ColorBlack = Color.Blue,
            ColorWhite = Color.LightBlue,
            LableValue = Resources.C128_ICV_LABLE,
            DiscValue = Resources.C128_ICV_DISC
        };

        #endregion

        public Code128(VisualEncoder caller) : base(caller) {/*empty*/}

        protected override Image GenerateBitmap(string input, VisualEncoderSettings settings)
        {
            BarcodeWriter barcodeWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.CODE_128,
                Options = new EncodingOptions
                {
                    Margin = 1,
                    Height = 100,
                    Width = 300
                }
            };
            string payload = input;
            return barcodeWriter.Write(payload);
        }

        protected override string EnrichInput(string input, VisualEncoderSettings settings)
        {
            if (input.Length > 80)//80 is the maximum for c128
            {
                return input.Substring(0, 80);
            }

            return input;
        }

        protected override bool VerifyInput(string input, VisualEncoderSettings settings)
        {
            if (input.Any(InvalidC128Char)) //if one char  is invalid
            {
                caller.GuiLogMessage(Resources.CODE39_INVALIDE_INPUT, NotificationLevel.Error);
                return false;
            }
            return true;
        }

        private bool InvalidC128Char(char c)
        {
            return !((c >= 32 && c <= 126) || (c >= 200 && c <= 211));
        }

        protected override List<LegendItem> GetLegend(string input, VisualEncoderSettings settings)
        {
            return new List<LegendItem> { startEndLegend, ivcLegend };
        }


        protected override Image GeneratePresentationBitmap(Image input, VisualEncoderSettings settings)
        {
            Bitmap bitmap = new Bitmap(input);
            int barSpaceCount = 0;
            bool isOnBlackBar = false;
            int barHight = 0;

            #region left 6 bars
            for (int x = 0; barSpaceCount <= 6; x++)
            {
                if (bitmap.GetPixel(x, bitmap.Height / 2).R == Color.Black.R)
                {
                    if (!isOnBlackBar)
                    {
                        barSpaceCount++;
                        isOnBlackBar = true;
                    }

                    if (barHight == 0)
                    {
                        barHight = CalcBarHight(bitmap, x);
                    }
                }
                else
                {
                    if (isOnBlackBar)
                    {
                        barSpaceCount++;
                        isOnBlackBar = false;
                    }
                }


                if (barSpaceCount > 0)
                {
                    bitmap = FillBitmapOnX(x, 0, barHight, bitmap, startEndLegend.ColorBlack, startEndLegend.ColorWhite);
                }
            }
            #endregion
            barSpaceCount = 0;
            isOnBlackBar = false;
            #region right bars
            for (int x = bitmap.Width - 1; barSpaceCount <= 13; x--)
            {
                if (bitmap.GetPixel(x, bitmap.Height / 2).R == Color.Black.R)
                {
                    if (!isOnBlackBar)
                    {
                        barSpaceCount++;
                        isOnBlackBar = true;
                    }
                }
                else
                {
                    if (isOnBlackBar)
                    {
                        barSpaceCount++;
                        isOnBlackBar = false;
                    }
                }

                if (barSpaceCount > 0)
                {
                    bitmap = barSpaceCount <= 6 ? FillBitmapOnX(x, 0, barHight, bitmap, startEndLegend.ColorBlack, startEndLegend.ColorWhite)
                                            : FillBitmapOnX(x, 0, barHight, bitmap, ivcLegend.ColorBlack, ivcLegend.ColorWhite);
                }
            }
            #endregion
            return bitmap;
        }

    }
}
