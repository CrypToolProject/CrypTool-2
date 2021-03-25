/*
   Copyright 2011 CrypTool 2 Team <ct2contact@CrypTool.org>

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
using System.Collections.Generic;
using System.Linq;
using CrypTool.PluginBase;
using System.ComponentModel;
using CrypTool.PluginBase.Miscellaneous;
using System.Windows.Controls;
using System.Numerics;

namespace CrypTool.Plugins.Decimalization
{
    [Author("Andreas Grüner", "agruener@informatik.hu-berlin.de", "Humboldt University Berlin", "http://www.hu-berlin.de")]
    [PluginInfo("Decimalization.Properties.Resources", "PluginCaption", "PluginTooltip", "Decimalization/DetailedDescription/doc.xml", "Decimalization/Decimalization.png")]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class Decimalization : ICrypComponent
    {
        #region Data Class

        private class Result
        {
            private String sres;
            private int[] ires;
            
            public Result(int[] arr)
            {
                Ires = arr;
                Sres = String.Join("", arr.Select(n => n.ToString()));
            }

            public String Sres
            {
                get { return sres; }
                set { sres = value; }
            }

            public int[] Ires
            {
                get { return ires; }
                set { ires = value; }
            }
        }

        #endregion

        #region Private Variables

        private readonly DecimalizationSettings settings = new DecimalizationSettings();

        #endregion

        #region Data Properties
        
        [PropertyInfo(Direction.InputData, "InputCaption", "InputTooltip")]
        public byte[] BinaryNumber
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "Output1Caption", "Output1Tooltip")]
        public int[] DecimalNumberInt
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "Output2Caption", "Output2Tooltip")]
        public String DecimalNumberStr
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members

        public ISettings Settings
        {
            get { return settings; }
        }

        public UserControl Presentation
        {
            get { return null; }
        }

        public UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
        }

        public void Execute()
        {
            try
            {
                Result res = null;

                ProgressChanged(0, 1);

                switch (settings.Mode)
                {
                    case 0:
                        res = ProcessVisaMethod();
                        break;
                    case 1:
                        res = ProcessModuloMethod();
                        break;
                    case 2:
                        res = ProcessMultMethod();
                        break;
                    case 3:
                        res = ProcessIBMMethod();
                        break;
                    default:
                        GuiLogMessage("Unknown Decimalization Mode", NotificationLevel.Error);
                        return;
                }

                DecimalNumberInt = res.Ires;
                DecimalNumberStr = res.Sres;

                OnPropertyChanged("DecimalNumberInt");
                OnPropertyChanged("DecimalNumberStr");

                ProgressChanged(1, 1);
            }
            catch (OverflowException e)
            {
                GuiLogMessage("Overflow Exception: Numbers are too big. Try again with smaller numbers.", NotificationLevel.Error);
            }
            catch (DivideByZeroException e)
            {
                GuiLogMessage("Divide By Zero Exception: Try again with other numbers.", NotificationLevel.Error);
            }
        }

        public void PostExecution()
        {
        }

        public void Stop()
        {
        }

        public void Initialize()
        {
            
        }

        public void Dispose()
        {
        }

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        private Result ProcessVisaMethod()
        {
            List<int> listres = new List<int>();
            List<int> listres9 = new List<int>();

            foreach(var b in BinaryNumber)
            {
                int hi = b >> 4;
                if (hi < 10) { listres.Add(hi); if (listres.Count >= settings.Quant) break; } else listres9.Add(hi - 10);
                int lo = b % 0xf;
                if (lo < 10) { listres.Add(lo); if (listres.Count >= settings.Quant) break; } else listres9.Add(lo - 10);
            }

            if (listres.Count < settings.Quant)
                listres.AddRange(listres9.Take(settings.Quant - listres.Count));

            if (listres.Count < settings.Quant)
                GuiLogMessage("Too few random input data for requested quantity of decimals.", NotificationLevel.Warning);

            return new Result(listres.ToArray());
        }

        private Result ProcessModuloMethod()
        {
            BigInteger dividend = byteArrayToBigInteger(BinaryNumber);
            BigInteger divisor = BigInteger.Pow(10, settings.Quant);
            BigInteger bigres = dividend % divisor;

            return new Result(bigIntegerToIntArray(bigres));
        }

        private Result ProcessMultMethod()
        {
            BigInteger dividend = byteArrayToBigInteger(BinaryNumber) * BigInteger.Pow(10, settings.Quant);
            BigInteger bigres = dividend >> (8 * BinaryNumber.Length);

            return new Result(bigIntegerToIntArray(bigres));
        }

        private Result ProcessIBMMethod()
        {
            int[] assocTable = { settings.IbmA, settings.IbmB, settings.IbmC, settings.IbmD, settings.IbmE, settings.IbmF };

            List<int> listres = new List<int>();

            foreach (var b in BinaryNumber)
            {
                int hi = b >> 4;
                listres.Add(hi < 10 ? hi : assocTable[hi - 10]);
                if (listres.Count >= settings.Quant) break;
                int lo = b & 0xf;
                listres.Add(lo < 10 ? lo : assocTable[lo - 10]);
                if (listres.Count >= settings.Quant) break;
            }

            if (listres.Count < settings.Quant)
                GuiLogMessage("Too few random input data for requested quantity of decimals.", NotificationLevel.Warning);

            return new Result(listres.ToArray());
        }

        private int[] bigIntegerToIntArray(BigInteger nr)
        {
            return nr.ToString().Select(c => (int)c - 48).ToArray();
        }

        private BigInteger byteArrayToBigInteger(byte[] bytes)
        {
            byte[] bytesPlusZero = new byte[bytes.Length + 1];
            bytes.CopyTo(bytesPlusZero, 0);
            bytesPlusZero[bytesPlusZero.Length - 1] = 0x00;
            return new BigInteger(bytesPlusZero);
        }

        #endregion
    }
}