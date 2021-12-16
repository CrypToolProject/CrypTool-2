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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using VisualEncoder.Properties;
using ZXing;
using ZXing.Common;

namespace CrypTool.Plugins.VisualEncoder.Encoders
{
    internal class EAN13 : DimCodeEncoder
    {
        #region legend Strings


        private readonly LegendItem icvLegend = new LegendItem
        {
            ColorBlack = Color.Blue,
            ColorWhite = Color.LightBlue,
            LableValue = Resources.EAN13_ICV_LABLE,
            DiscValue = Resources.EAN13_ICV_DISC

        };

        private readonly LegendItem fixedLegend = new LegendItem
        {
            ColorBlack = Color.Green,
            ColorWhite = Color.LightGreen,
            LableValue = Resources.EAN13_FIXED_LABLE,
            DiscValue = Resources.EAN13_FIXED_DISC
        };


        #endregion

        public EAN13(VisualEncoder caller) : base(caller) {/*empty*/}

        protected override Image GenerateBitmap(string input, VisualEncoderSettings settings)
        {
            BarcodeWriter barcodeWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.EAN_13,
                Options = new EncodingOptions
                {
                    Height = 100,
                    Width = 300
                }
            };

            string payload = input;

            if (settings.AppendICV)
            {
                payload = payload.Substring(0, 12); // cut of last byte to let the lib calculate the ICV
            }

            return barcodeWriter.Write(payload);
        }

        protected override string EnrichInput(string input, VisualEncoderSettings settings)
        {
            input = input.Substring(Math.Max(0, input.Length - 13));

            if (input.Length < 13)
            {
                input = input.PadLeft(13, '0');
            }
            return input;
        }

        protected override bool VerifyInput(string input, VisualEncoderSettings settings)
        {
            if (input.Any(b => b < '0' || b > '9'))
            {
                caller.GuiLogMessage(Resources.EAN_INVALIDE_INPUT, NotificationLevel.Error);
                return false;
            }
            return true;
        }

        protected override List<LegendItem> GetLegend(string input, VisualEncoderSettings settings)
        {
            List<LegendItem> legend = new List<LegendItem> { fixedLegend };

            if (settings.AppendICV)
            {
                legend.Add(icvLegend);
            }

            return legend;
        }

        protected override Image GeneratePresentationBitmap(Image input, VisualEncoderSettings settings)
        {
            Bitmap bitmap = new Bitmap(input);
            int barcount = 0;
            bool isOnBlackBar = false;
            int barHight = 0;

            for (int x = 0; x < bitmap.Width; x++)
            {
                if (bitmap.GetPixel(x, bitmap.Height / 2).R == Color.Black.R)
                {
                    if (!isOnBlackBar)
                    {
                        barcount++;
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
                        barcount++;
                        isOnBlackBar = false;
                    }

                }

                if ((barcount >= 1 && barcount <= 3) || (barcount >= 29 && barcount <= 31) || (barcount >= 57 && barcount <= 59))
                {
                    bitmap = FillBitmapOnX(x, 0, barHight, bitmap, fixedLegend.ColorBlack, fixedLegend.ColorWhite);
                }
                else if ((barcount >= 53 && barcount <= 56) && settings.AppendICV)
                {
                    bitmap = FillBitmapOnX(x, 0, barHight, bitmap, icvLegend.ColorBlack, icvLegend.ColorWhite);
                }
            }
            return bitmap;
        }
    }
}
