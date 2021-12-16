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
    internal class Code39 : DimCodeEncoder
    {
        #region legend Strings

        private readonly LegendItem startEndLegend = new LegendItem
        {
            ColorBlack = Color.Green,
            ColorWhite = Color.LightGreen,
            LableValue = Resources.C38_STARTEND_LABLE,
            DiscValue = Resources.C38_STARTEND_DISC
        };

        private readonly LegendItem ivcLegend = new LegendItem
        {
            ColorBlack = Color.Blue,
            ColorWhite = Color.LightBlue,
            LableValue = Resources.C38_ICV_LABLE,
            DiscValue = Resources.C38_ICV_DISC
        };

        #endregion

        public Code39(VisualEncoder caller) : base(caller) {/*empty*/}

        /// <summary>
        /// see superclass
        /// </summary>
        protected override Image GenerateBitmap(string input, VisualEncoderSettings settings)
        {
            BarcodeWriter barcodeWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.CODE_39,
                Options = new EncodingOptions
                {
                    Margin = 5,
                    Height = 100,
                    Width = 300// it automaticly gets bigger
                }
            };

            string payload = input;
            if (settings.AppendICV)
            {
                if (payload.Length == 80) // replace last digit with icv
                {
                    payload = payload.Substring(0, 79) + calcICV(payload.Substring(0, 79));
                }
                else // simply append
                {
                    payload += calcICV(payload);
                }
            }

            return barcodeWriter.Write(payload);
        }
        /// <summary>
        /// see superclass
        /// </summary>
        protected override string EnrichInput(string input, VisualEncoderSettings settings)
        {
            if (input.Length > 80)//80 is the maximum for c39
            {
                return input.Substring(0, 80);
            }

            return input;
        }

        /// <summary>
        /// see superclass
        /// </summary>
        protected override bool VerifyInput(string input, VisualEncoderSettings settings)
        {
            if (input.Any(c => Code39CharToInt(c) == -1))
            {
                caller.GuiLogMessage(Resources.CODE39_INVALIDE_INPUT, NotificationLevel.Error);
                return false;
            }
            return true;
        }

        /// <summary>
        /// see superclass
        /// </summary>
        protected override List<LegendItem> GetLegend(string input, VisualEncoderSettings settings)
        {
            return new List<LegendItem> { startEndLegend, ivcLegend };
        }

        /// <summary>
        /// see superclass
        /// </summary>s
        protected override Image GeneratePresentationBitmap(Image input, VisualEncoderSettings settings)
        {
            Bitmap bitmap = new Bitmap(input);
            int barSpaceCount = 0;
            bool isOnBlackBar = false;
            int barHight = 0;

            #region color start region
            for (int x = 0; barSpaceCount < 10; x++)
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

            #region color endregion and checksum

            barSpaceCount = 0;
            isOnBlackBar = false;

            for (int x = bitmap.Width - 1; barSpaceCount <= 19; x--)
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
                    bitmap = barSpaceCount <= 9 ? FillBitmapOnX(x, 0, barHight, bitmap, startEndLegend.ColorBlack, startEndLegend.ColorWhite)
                                            : FillBitmapOnX(x, 0, barHight, bitmap, ivcLegend.ColorBlack, ivcLegend.ColorWhite);
                }
            }
            #endregion

            return bitmap;
        }

        #region helper

        /// <summary>
        /// calculation of icv: add all char with his corresponding Code39 value in module 43
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private string calcICV(string p)
        {
            return "" + Code39IntToChar(p.Sum(c => Code39CharToInt(c)) % 43);
        }



        /// <summary>
        /// get the code39 encoding value  0 = 0, ... , 9 = 9, A = 10, ... , Z = 35 ...
        /// </summary>
        /// <param name="c"></param>
        /// <returns> the value or -1 if the char has no representation in the CODE38 coding</returns>
        private static int Code39CharToInt(char c)
        {
            if (c >= '0' && c <= '9')
            {
                return c - '0'; //char c is 0-9 return integer 0-9
            }

            if (c >= 'A' && c <= 'Z')
            {
                return c - 'A' + 10; //A = 10 ... Z = 35
            }

            switch (c)
            {
                case '-':
                    return 36;
                case '.':
                    return 37;
                case ' ':
                    return 38;
                case '$':
                    return 39;
                case '/':
                    return 40;
                case '+':
                    return 41;
                case '%':
                    return 42;
            }

            return -1;
            //throw new Exception("Code39CharToInt: Invalide char got: '" + c + "'. fix your input validation!");
        }

        /// <summary>
        /// get the code39 encoding value  0 = 0, ... , 9 = 9, 10 = 'A', ... , 35 = 'Z' ...
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        private static char Code39IntToChar(int i)
        {
            if (i >= 0 && i <= 9)
            {
                return (char)('0' + i);
            }

            if (i >= 10 && i <= 35)
            {
                return (char)('A' + i - 10);
            }

            switch (i)
            {
                case 36:
                    return '-';
                case 37:
                    return '.';
                case 38:
                    return ' ';
                case 39:
                    return '$';
                case 40:
                    return '/';
                case 41:
                    return '+';
                case 42:
                    return '%';
            }

            throw new Exception("code39intToChar: Invalide int got: '" + i + "'. fix your input validation!");
        }
        #endregion
    }
}
