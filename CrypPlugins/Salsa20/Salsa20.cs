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
using System.Text;
using System.Windows.Controls;

namespace CrypTool.Plugins.Salsa20
{
    [Author("Maxim Serebrianski", "ms_1990@gmx.de", "University of Mannheim", "http://www.uni-mannheim.de/1/startseite/index.html")]
    [PluginInfo("Salsa20.Properties.Resources", "PluginCaption", "PluginTooltip", "Salsa20/DetailedDescription/doc.xml", new[] { "Salsa20/Images/salsa20.jpg" })]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class Salsa20 : ICrypComponent
    {
        #region Private Variables

        private Salsa20Settings settings;

        private byte[] inputData;
        private byte[] outputData;
        private byte[] inputKey;
        private byte[] inputIV;

        private int rounds;

        #endregion

        #region Public Variables

        public static byte[] sigma = Encoding.ASCII.GetBytes("expand 32-byte k");
        public static byte[] tau = Encoding.ASCII.GetBytes("expand 16-byte k");
        public static int[] Rounds = new int[] { 8, 12, 20 };

        public static int stateSize = 16; // 16, 32 bit ints = 64 bytes
        public int index = 0;
        public uint[] engineState = new uint[stateSize]; // state
        public uint[] x = new uint[stateSize]; // internal buffer
        public byte[] keyStream = new byte[stateSize * 4];
        public int cW0, cW1, cW2;

        #endregion

        public Salsa20()
        {
            settings = new Salsa20Settings();
        }

        public ISettings Settings
        {
            get => settings;
            set => settings = (Salsa20Settings)value;
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

        [PropertyInfo(Direction.InputData, "InputKeyCaption", "InputKeyTooltip", true)]
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

            if (inputIV.Length != 8)
            {
                GuiLogMessage("Wrong IV length " + inputIV.Length + " bytes. IV length must be 8 bytes.", NotificationLevel.Error);
                return false;
            }

            if (inputKey.Length != 16 && inputKey.Length != 32)
            {
                GuiLogMessage("Wrong key length " + inputKey.Length + " bytes. Key length must be 16 or 32 bytes.", NotificationLevel.Error);
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
            rounds = Rounds[settings.Rounds] / 2;
            setKey(inputKey, inputIV);
        }

        /* Keysetup */
        public void setKey(byte[] keyBytes, byte[] ivBytes)
        {
            index = 0;
            resetCounter();
            int offset = 0;
            byte[] constants;

            // Key
            engineState[1] = byteToIntLE(keyBytes, 0);
            engineState[2] = byteToIntLE(keyBytes, 4);
            engineState[3] = byteToIntLE(keyBytes, 8);
            engineState[4] = byteToIntLE(keyBytes, 12);

            if (keyBytes.Length == 32)
            {
                constants = sigma;
                offset = 16;
            }
            else
            {
                constants = tau;
            }

            engineState[11] = byteToIntLE(keyBytes, offset);
            engineState[12] = byteToIntLE(keyBytes, offset + 4);
            engineState[13] = byteToIntLE(keyBytes, offset + 8);
            engineState[14] = byteToIntLE(keyBytes, offset + 12);
            engineState[0] = byteToIntLE(constants, 0);
            engineState[5] = byteToIntLE(constants, 4);
            engineState[10] = byteToIntLE(constants, 8);
            engineState[15] = byteToIntLE(constants, 12);

            // IV
            engineState[6] = byteToIntLE(ivBytes, 0);
            engineState[7] = byteToIntLE(ivBytes, 4);
            engineState[8] = engineState[9] = 0;
        }

        /* Left rotation */
        public uint rotateLeft(uint x, int y)
        {
            return (x << y) | (x >> -y);
        }

        /* Convert Integer to little endian byte array */
        public byte[] intToByteLE(uint x, byte[] output, int off)
        {
            output[off] = (byte)x;
            output[off + 1] = (byte)(x >> 8);
            output[off + 2] = (byte)(x >> 16);
            output[off + 3] = (byte)(x >> 24);
            return output;
        }

        /* Convert little endian byte array to Integer */
        public uint byteToIntLE(byte[] x, int offset)
        {
            return (uint)(((x[offset] & 255)) | ((x[offset + 1] & 255) << 8) | ((x[offset + 2] & 255) << 16) | (x[offset + 3] << 24));
        }

        /* Reset the counter */
        public void resetCounter()
        {
            cW0 = 0;
            cW1 = 0;
            cW2 = 0;
        }

        /* Check the limit of 2^70 bytes */
        public bool limitExceeded(int length)
        {
            if (cW0 >= 0)
            {
                cW0 += length;
            }
            else
            {
                cW0 += length;
                if (cW0 >= 0)
                {
                    cW1++;
                    if (cW1 == 0)
                    {
                        cW2++;
                        return (cW2 & 0x20) != 0;   // 2^(32 + 32 + 6)
                    }
                }

            }
            return false;
        }

        /* Generate key stream */
        public void generateKeyStream(uint[] input, byte[] output)
        {
            int offset = 0;

            Array.Copy(input, 0, x, 0, input.Length);

            for (int i = 0; i < rounds; i++)
            {
                x[4] ^= rotateLeft((x[0] + x[12]), 7);
                x[8] ^= rotateLeft((x[4] + x[0]), 9);
                x[12] ^= rotateLeft((x[8] + x[4]), 13);
                x[0] ^= rotateLeft((x[12] + x[8]), 18);
                x[9] ^= rotateLeft((x[5] + x[1]), 7);
                x[13] ^= rotateLeft((x[9] + x[5]), 9);
                x[1] ^= rotateLeft((x[13] + x[9]), 13);
                x[5] ^= rotateLeft((x[1] + x[13]), 18);
                x[14] ^= rotateLeft((x[10] + x[6]), 7);
                x[2] ^= rotateLeft((x[14] + x[10]), 9);
                x[6] ^= rotateLeft((x[2] + x[14]), 13);
                x[10] ^= rotateLeft((x[6] + x[2]), 18);
                x[3] ^= rotateLeft((x[15] + x[11]), 7);
                x[7] ^= rotateLeft((x[3] + x[15]), 9);
                x[11] ^= rotateLeft((x[7] + x[3]), 13);
                x[15] ^= rotateLeft((x[11] + x[7]), 18);
                x[1] ^= rotateLeft((x[0] + x[3]), 7);
                x[2] ^= rotateLeft((x[1] + x[0]), 9);
                x[3] ^= rotateLeft((x[2] + x[1]), 13);
                x[0] ^= rotateLeft((x[3] + x[2]), 18);
                x[6] ^= rotateLeft((x[5] + x[4]), 7);
                x[7] ^= rotateLeft((x[6] + x[5]), 9);
                x[4] ^= rotateLeft((x[7] + x[6]), 13);
                x[5] ^= rotateLeft((x[4] + x[7]), 18);
                x[11] ^= rotateLeft((x[10] + x[9]), 7);
                x[8] ^= rotateLeft((x[11] + x[10]), 9);
                x[9] ^= rotateLeft((x[8] + x[11]), 13);
                x[10] ^= rotateLeft((x[9] + x[8]), 18);
                x[12] ^= rotateLeft((x[15] + x[14]), 7);
                x[13] ^= rotateLeft((x[12] + x[15]), 9);
                x[14] ^= rotateLeft((x[13] + x[12]), 13);
                x[15] ^= rotateLeft((x[14] + x[13]), 18);
            }
            for (int i = 0; i < stateSize; i++)
            {
                intToByteLE(x[i] + input[i], output, offset);
                offset += 4;
            }
            for (int i = stateSize; i < x.Length; i++)
            {
                intToByteLE(x[i], output, offset);
                offset += 4;
            }
        }

        /* Generate key stream byte */
        public byte getKeyStreamByte()
        {
            if (index == 0)
            {
                generateKeyStream(engineState, keyStream);
                engineState[8]++;
                if (engineState[8] == 0)
                {
                    engineState[9]++;
                }
            }

            byte result = keyStream[index];
            index = (index + 1) & 63;

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

