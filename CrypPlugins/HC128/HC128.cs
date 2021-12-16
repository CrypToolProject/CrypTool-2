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

using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.ComponentModel;
using System.Windows.Controls;

namespace CrypTool.Plugins.HC128
{
    [Author("Maxim Serebrianski", "ms_1990@gmx.de", "University of Mannheim", "http://www.uni-mannheim.de/1/startseite/index.html")]
    [PluginInfo("HC128.Properties.Resources", "PluginCaption", "PluginTooltip", "HC128/DetailedDescription/doc.xml", new[] { "HC128/Images/hc128.jpg" })]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class HC128 : ICrypComponent
    {
        #region Private Variables

        private HC128Settings settings;

        private byte[] inputData;
        private byte[] outputData;
        private byte[] inputKey;
        private byte[] inputIV;

        #endregion

        #region Public Variables

        public uint[] p = new uint[512];
        public uint[] q = new uint[512];
        public uint count = 0;
        public byte[] buffer = new byte[4];
        public int idx = 0;

        #endregion

        public HC128()
        {
            settings = new HC128Settings();
        }

        public ISettings Settings
        {
            get => settings;
            set => settings = (HC128Settings)value;
        }

        #region Data Properties

        [PropertyInfo(Direction.InputData, "InputDataCaption", "InputDataTooltip", true)]
        public byte[] InputData
        {
            get => inputData;
            set
            {
                inputData = value;
                OnPropertyChanged("InputData");
            }
        }

        [PropertyInfo(Direction.InputData, "InputKeyCaption", "InputKeyDataTooltip", true)]
        public byte[] InputKey
        {
            get => inputKey;
            set
            {
                inputKey = value;
                OnPropertyChanged("InputKey");
            }
        }

        [PropertyInfo(Direction.InputData, "InputIVCaption", "InputIVTooltip", true)]
        public byte[] InputIV
        {
            get => inputIV;
            set
            {
                inputIV = value;
                OnPropertyChanged("InputIV");
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputDataCaption", "OutputDataTooltip", true)]
        public byte[] OutputData
        {
            get => outputData;
            set
            {
                outputData = value;
                OnPropertyChanged("OutputData");
            }
        }

        #endregion

        private bool checkParameters()
        {
            if (inputData == null)
            {
                GuiLogMessage("No input given. Aborting.", NotificationLevel.Error);
                return false;
            }

            if (inputKey == null)
            {
                GuiLogMessage("No key given. Aborting.", NotificationLevel.Error);
                return false;
            }

            if (inputIV == null)
            {
                GuiLogMessage("No IV given. Aborting.", NotificationLevel.Error);
                return false;
            }

            if (inputIV.Length != 16)
            {
                GuiLogMessage("Wrong IV length " + inputIV.Length + " bytes. IV length must be 16 bytes.", NotificationLevel.Error);
                return false;
            }

            if (inputKey.Length != 16)
            {
                GuiLogMessage("Wrong key length " + inputKey.Length + " bytes. Key length must be 16 bytes.", NotificationLevel.Error);
                return false;
            }
            return true;
        }

        /* Main method for launching the cipher */
        public void Execute()
        {
            ProgressChanged(0, 1);

            if (!checkParameters())
            {
                return;
            }

            init();

            OutputData = encrypt(inputData);

            ProgressChanged(1, 1);
        }

        /* Reset method */
        public void Dispose()
        {
            inputData = null;
            inputKey = null;
            inputIV = null;
            outputData = null;
        }

        /* Main initialization method */
        public void init()
        {
            idx = 0;
            count = 0;

            uint[] w = new uint[1280];

            for (int i = 0; i < 16; i++)
            {
                w[i >> 2] |= (uint)(inputKey[i] & 0xff) << (8 * (i & 0x3));
            }
            Array.Copy(w, 0, w, 4, 4);

            for (int i = 0; i < inputIV.Length && i < 16; i++)
            {
                w[(i >> 2) + 8] |= (uint)(inputIV[i] & 0xff) << (8 * (i & 0x3));
            }
            Array.Copy(w, 8, w, 12, 4);

            for (int i = 16; i < 1280; i++)
            {
                w[i] = (uint)(f2(w[i - 2]) + w[i - 7] + f1(w[i - 15]) + w[i - 16] + i);
            }

            Array.Copy(w, 256, p, 0, 512);
            Array.Copy(w, 768, q, 0, 512);

            for (int i = 0; i < 512; i++)
            {
                p[i] = Round();
            }
            for (int i = 0; i < 512; i++)
            {
                q[i] = Round();
            }

            count = 0;
        }

        /* (x - y) % 512 */
        public uint minus(uint x, uint y)
        {
            return (x - y) & 0x1FF;
        }

        /* Left rotation function */
        public uint leftRotation(uint x, int bits)
        {
            return (x << bits) | (x >> -bits);
        }

        /* Right rotation function */
        public uint rightRotation(uint x, int bits)
        {
            return (x >> bits) | (x << -bits);
        }

        /* f1 function */
        public uint f1(uint x)
        {
            return rightRotation(x, 7) ^ rightRotation(x, 18) ^ (x >> 3);
        }

        /* f2 function */
        public uint f2(uint x)
        {
            return rightRotation(x, 17) ^ rightRotation(x, 19) ^ (x >> 10);
        }

        /* g1 function */
        public uint g1(uint x, uint y, uint z)
        {
            return (rightRotation(x, 10) ^ rightRotation(z, 23)) + rightRotation(y, 8);
        }

        /* g2 function */
        public uint g2(uint x, uint y, uint z)
        {
            return (leftRotation(x, 10) ^ leftRotation(z, 23)) + leftRotation(y, 8);
        }

        /* h1 function */
        public uint h1(uint x)
        {
            return q[x & 0xFF] + q[((x >> 16) & 0xFF) + 256];
        }

        /* h2 function */
        public uint h2(uint x)
        {
            return p[x & 0xFF] + p[((x >> 16) & 0xFF) + 256];
        }

        /* One cipher round */
        public uint Round()
        {
            uint j = count & 0x1FF;
            uint result;
            if (count < 512)
            {
                p[j] += g1(p[minus(j, 3)], p[minus(j, 10)], p[minus(j, 511)]);
                result = h1(p[minus(j, 12)]) ^ p[j];
            }
            else
            {
                q[j] += g2(q[minus(j, 3)], q[minus(j, 10)], q[minus(j, 511)]);
                result = h2(q[minus(j, 12)]) ^ q[j];
            }
            count = (count + 1) & 0x3FF;
            return result;
        }

        /* Generate key stream byte */
        public byte getKeyStreamByte()
        {
            if (idx == 0)
            {
                uint step = Round();
                buffer[0] = (byte)step;
                buffer[1] = (byte)(step >> 8);
                buffer[2] = (byte)(step >> 16);
                buffer[3] = (byte)(step >> 24);
            }

            byte result = buffer[idx];
            idx = (idx + 1) & 0x3;

            return result;
        }

        /* Generate ciphertext */
        public byte[] encrypt(byte[] src)
        {
            byte[] dst = new byte[src.Length];

            for (int i = 0; i < src.Length; i++)
            {
                dst[i] = (byte)(src[i] ^ getKeyStreamByte());
            }

            return dst;
        }

        public void Initialize()
        {
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public void PostExecution()
        {
            Dispose();
        }

        public void PreExecution()
        {
            Dispose();
        }

        public UserControl Presentation => null;

        public void Stop()
        {
        }

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void StatusChanged(int imageIndex)
        {
            EventsHelper.StatusChanged(OnPluginStatusChanged, this, new StatusEventArgs(StatusChangedMode.ImageUpdate, imageIndex));
        }

        #endregion

    }
}
