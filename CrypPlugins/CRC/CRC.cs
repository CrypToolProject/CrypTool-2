/*
   Copyright 2009-2010 Matthäus Wander, University of Duisburg-Essen

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
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.ComponentModel;

namespace CrypTool.CRC
{
    [Author("Matthäus Wander, Armin Krauß", "wander@CrypTool.org", "Fachgebiet Verteilte Systeme, Universität Duisburg-Essen", "http://www.vs.uni-due.de")]
    [PluginInfo("CRC.Properties.Resources", "PluginCaption", "PluginTooltip", "CRC/DetailedDescription/doc.xml", "CRC/Images/CRC.png")]
    [ComponentCategory(ComponentCategory.HashFunctions)]
    public class CRC : ICrypComponent
    {
        #region Constants and private variables

        private CRCSettings settings;
        private CRCInfo crcspec;

        private CStreamWriter outputStreamWriter;
        private readonly bool integrity;

        private readonly ulong[] table = new ulong[256];

        #endregion

        #region Public interface

        public CRC()
        {
            settings = new CRCSettings();
            //this.settings.OnPluginStatusChanged += settings_OnPluginStatusChanged;
            //this.settings.PropertyChanged += settings_OnPropertyChange;
        }

        //private void settings_OnPropertyChange(object sender, PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName == "CRCMethod")
        //        Execute();
        //}

        public ISettings Settings
        {
            get => settings;
            set => settings = (CRCSettings)value;
        }

        [PropertyInfo(Direction.InputData, "InputStreamCaption", "InputStreamTooltip", true)]
        public ICrypToolStream InputStream
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "OutputStreamCaption", "OutputStreamTooltip", false)]
        public ICrypToolStream OutputStream
        {
            get => outputStreamWriter;

            set
            {
            }
        }

        //[PropertyInfo(Direction.OutputData, "IntegrityCaption", "IntegrityTooltip", false)]
        //public Boolean Integrity
        //{
        //    get
        //    {
        //        return this.integrity;
        //    }

        //    set
        //    {
        //        this.integrity = value;
        //        OnPropertyChanged("Integrity");
        //    }
        //}

        #endregion

        #region IPlugin Members

        public void Dispose()
        {
            if (outputStreamWriter != null)
            {
                outputStreamWriter.Dispose();
                outputStreamWriter = null;
            }
        }

        public void Execute()
        {
            Initialize();
            printInfo();

            ProgressChanged(0.0, 1.0);

            if (InputStream == null)
            {
                GuiLogMessage("Received null value for input CStream, not processing.", NotificationLevel.Warning);
                return;
            }

            // parameter check

            if ((crcspec.polynomial & ~crcspec.mask) != 0)
            {
                GuiLogMessage("The generator polynomial is too big for the given width.", NotificationLevel.Warning);
            }

            if ((crcspec.init & ~crcspec.mask) != 0)
            {
                GuiLogMessage("The init value is too big for the given width.", NotificationLevel.Warning);
            }

            if ((crcspec.xorout & ~crcspec.mask) != 0)
            {
                GuiLogMessage("The xorout value is too big for the given width.", NotificationLevel.Warning);
            }

            // calculate CRC value

            byte[] input = ICrypToolStreamToByteArray(InputStream);

            ulong crc;

            //if (settings.Mode == 0)
            {    // calculate CRC value
                crc = revert(crcspec.init, crcspec.width) & crcspec.mask;

                for (int i = 0; i < input.Length; ++i)
                {
                    byte b = input[i];
                    if (!crcspec.refin)
                    {
                        b = reverseByte[b];
                    }

                    ulong temp = (crc ^ b) & 0xff;
                    for (int j = 0; j < 8; j++)
                    {
                        temp = ((temp & 1) == 1) ? (temp >> 1) ^ crcspec.polynomial_reversed : (temp >> 1);
                    }

                    crc = (crc >> 8) ^ temp;
                    ProgressChanged(i, input.Length);
                }

                if (!crcspec.refout)
                {
                    crc = revert(crc, crcspec.width);
                }

                crc ^= crcspec.xorout & crcspec.mask;
            }
            //else
            //{   // check integrity
            //    ulong temp = revert(crcspec.init, crcspec.width) & crcspec.mask;
            //    crc = 0;

            //    for (int i = 0; i < input.Length; ++i)
            //    {
            //        byte b = input[i];
            //        if (!crcspec.refin) b = reverseByte[b];
            //        for (int j = 0; j < 8; j++)
            //        {
            //            crc = ((crc & 1) == 1) ? (crc >> 1) ^ crcspec.polynomial_reversed : (crc >> 1);
            //            crc ^= ((ulong)(b & 1)) << (crcspec.width - 1);
            //            crc ^= ((temp & 1)) << (crcspec.width - 1);
            //            temp >>= 1;
            //            b >>= 1;
            //        }
            //        ProgressChanged(i, input.Length);
            //    }

            //    if (!crcspec.refout) crc = revert(crc, crcspec.width);
            //    crc ^= crcspec.xorout & crcspec.mask;

            //    Integrity = (crc== 0);
            //}


            // convert CRC value to byte array with minimum number of bytes necessary

            int numoutbytes = (crcspec.width + 7) / 8;
            byte[] outputData = new byte[numoutbytes];

            for (int i = numoutbytes - 1; i >= 0; i--)
            {
                outputData[i] = (byte)crc;
                crc >>= 8;
            }

            // create output stream
            outputStreamWriter = new CStreamWriter(outputData);

            ProgressChanged(1.0, 1.0);
            OnPropertyChanged("OutputStream");
        }

        private byte[] CStreamReaderToByteArray(CStreamReader stream)
        {
            stream.WaitEof();
            byte[] buffer = new byte[stream.Length];
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            stream.ReadFully(buffer);
            return buffer;
        }

        private byte[] ICrypToolStreamToByteArray(ICrypToolStream stream)
        {
            return CStreamReaderToByteArray(stream.CreateReader());
        }

        private static readonly byte[] reverseByte = new byte[256] {
            0x00, 0x80, 0x40, 0xc0, 0x20, 0xa0, 0x60, 0xe0, 0x10, 0x90, 0x50, 0xd0, 0x30, 0xb0, 0x70, 0xf0,
            0x08, 0x88, 0x48, 0xc8, 0x28, 0xa8, 0x68, 0xe8, 0x18, 0x98, 0x58, 0xd8, 0x38, 0xb8, 0x78, 0xf8,
            0x04, 0x84, 0x44, 0xc4, 0x24, 0xa4, 0x64, 0xe4, 0x14, 0x94, 0x54, 0xd4, 0x34, 0xb4, 0x74, 0xf4,
            0x0c, 0x8c, 0x4c, 0xcc, 0x2c, 0xac, 0x6c, 0xec, 0x1c, 0x9c, 0x5c, 0xdc, 0x3c, 0xbc, 0x7c, 0xfc,
            0x02, 0x82, 0x42, 0xc2, 0x22, 0xa2, 0x62, 0xe2, 0x12, 0x92, 0x52, 0xd2, 0x32, 0xb2, 0x72, 0xf2,
            0x0a, 0x8a, 0x4a, 0xca, 0x2a, 0xaa, 0x6a, 0xea, 0x1a, 0x9a, 0x5a, 0xda, 0x3a, 0xba, 0x7a, 0xfa,
            0x06, 0x86, 0x46, 0xc6, 0x26, 0xa6, 0x66, 0xe6, 0x16, 0x96, 0x56, 0xd6, 0x36, 0xb6, 0x76, 0xf6,
            0x0e, 0x8e, 0x4e, 0xce, 0x2e, 0xae, 0x6e, 0xee, 0x1e, 0x9e, 0x5e, 0xde, 0x3e, 0xbe, 0x7e, 0xfe,
            0x01, 0x81, 0x41, 0xc1, 0x21, 0xa1, 0x61, 0xe1, 0x11, 0x91, 0x51, 0xd1, 0x31, 0xb1, 0x71, 0xf1,
            0x09, 0x89, 0x49, 0xc9, 0x29, 0xa9, 0x69, 0xe9, 0x19, 0x99, 0x59, 0xd9, 0x39, 0xb9, 0x79, 0xf9,
            0x05, 0x85, 0x45, 0xc5, 0x25, 0xa5, 0x65, 0xe5, 0x15, 0x95, 0x55, 0xd5, 0x35, 0xb5, 0x75, 0xf5,
            0x0d, 0x8d, 0x4d, 0xcd, 0x2d, 0xad, 0x6d, 0xed, 0x1d, 0x9d, 0x5d, 0xdd, 0x3d, 0xbd, 0x7d, 0xfd,
            0x03, 0x83, 0x43, 0xc3, 0x23, 0xa3, 0x63, 0xe3, 0x13, 0x93, 0x53, 0xd3, 0x33, 0xb3, 0x73, 0xf3,
            0x0b, 0x8b, 0x4b, 0xcb, 0x2b, 0xab, 0x6b, 0xeb, 0x1b, 0x9b, 0x5b, 0xdb, 0x3b, 0xbb, 0x7b, 0xfb,
            0x07, 0x87, 0x47, 0xc7, 0x27, 0xa7, 0x67, 0xe7, 0x17, 0x97, 0x57, 0xd7, 0x37, 0xb7, 0x77, 0xf7,
            0x0f, 0x8f, 0x4f, 0xcf, 0x2f, 0xaf, 0x6f, 0xef, 0x1f, 0x9f, 0x5f, 0xdf, 0x3f, 0xbf, 0x7f, 0xff
        };

        private ulong revert(ulong x, int bits)
        {
            ulong result = 0;

            for (int i = 0; i < bits; i++)
            {
                result = (result << 1) + (x & 1);
                x >>= 1;
            }

            return result;
        }

        public void Initialize()
        {
            try
            {
                crcspec.width = settings.Width;
                crcspec.polynomial = (ulong)Convert.ToInt64(settings.Polynomial, 16);
                crcspec.polynomial_reversed = revert(crcspec.polynomial, crcspec.width);
                crcspec.polynomial_reversedreciprocal = revert(revert(crcspec.polynomial >> 1, crcspec.width) | 1, crcspec.width);
                crcspec.init = (ulong)Convert.ToInt64(settings.Init, 16);
                crcspec.xorout = (ulong)Convert.ToInt64(settings.XorOut, 16);
                crcspec.refin = settings.RefIn;
                crcspec.refout = settings.RefOut;
                crcspec.mask = 0;
                for (int i = 0; i < crcspec.width; i++)
                {
                    crcspec.mask = (crcspec.mask << 1) + 1;
                }
            }
            catch (Exception)
            {
            };
        }

        public void PostExecution()
        {
        }

        public void PreExecution()
        {
        }

        // print infos about the used CRC method
        public void printInfo()
        {
            string fmt = "x" + ((crcspec.width + 3) / 4);
            string msg = string.Format("generator polynomial: (normal=0x{0} reversed=0x{1} reversed reciprocal=0x{2}) init=0x{3} xorout=0x{4} refin={5} refout={6}",
                crcspec.polynomial.ToString(fmt), crcspec.polynomial_reversed.ToString(fmt), crcspec.polynomial_reversedreciprocal.ToString(fmt),
                crcspec.init.ToString(fmt), crcspec.xorout.ToString(fmt), crcspec.refin, crcspec.refout);
            GuiLogMessage(msg, NotificationLevel.Info);
        }

        public void Stop()
        {
        }

        public System.Windows.Controls.UserControl Presentation => null;


        #endregion

        #region INotifyPropertyChanged Members

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string p)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(p));
        }

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        #endregion
    }
}