/*
   Copyright 2011 CrypTool 2 Team <ct2contact@cryptool.org>

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
using System.Text;
using CrypTool.PluginBase;
using System.ComponentModel;
using System.Windows.Controls;
using CrypTool.PluginBase.Miscellaneous;
using System.Text.RegularExpressions;

namespace CrypTool.Plugins.GrainV1
{
    [Author("Maxim Serebrianski", "ms_1990@gmx.de", "University of Mannheim", "http://www.uni-mannheim.de/1/startseite/index.html")]
    [PluginInfo("GrainV1.Properties.Resources", "PluginCaption", "PluginTooltip", "GrainV1/DetailedDescription/doc.xml", new[] { "GrainV1/Images/grain.jpg" })]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class GrainV1 : ICrypComponent
    {
        #region Private Variables

        private GrainV1Settings settings;

        private byte[] inputData;
        private byte[] outputData;
        private byte[] inputKey;
        private byte[] inputIV;

        #endregion

        #region Public Variables

        public const int STATE_SIZE = 5;
        public uint[] lfsr = new uint[STATE_SIZE];
        public uint[] nfsr = new uint[STATE_SIZE];
        public byte[] outp = new byte[2];
        public uint index;

        #endregion

        public GrainV1()
        {
            this.settings = new GrainV1Settings();
        }

        public ISettings Settings
        {
            get { return (ISettings)this.settings; }
            set { this.settings = (GrainV1Settings)value; }
        }

        #region Data Properties

        [PropertyInfo(Direction.InputData, "InputDataCaption", "InputDataTooltip", true)]
        public byte[] InputData
        {
            get { return this.inputData; }
            set
            {
                this.inputData = value;
                OnPropertyChanged("InputData");
            }
        }

        [PropertyInfo(Direction.InputData, "InputKeyCaption", "InputKeyTooltip", true)]
        public byte[] InputKey
        {
            get { return this.inputKey; }
            set
            {
                this.inputKey = value;
                OnPropertyChanged("InputKey");
            }
        }

        [PropertyInfo(Direction.InputData, "InputIVCaption", "InputIVTooltip", true)]
        public byte[] InputIV
        {
            get { return this.inputIV; }
            set
            {
                this.inputIV = value;
                OnPropertyChanged("InputIV");
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputDataCaption", "OutputDataTooltip", true)]
        public byte[] OutputData
        {
            get { return this.outputData; }
            set
            {
                this.outputData = value;
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

            if (inputKey.Length != 10)
            {
                GuiLogMessage("Wrong key length " + inputKey.Length + " bytes. Key length must be 10 bytes.", NotificationLevel.Error);
                return false;
            }

            if (inputIV.Length != 8)
            {
                GuiLogMessage("Wrong IV length " + inputIV.Length + " bytes. IV length must be 8 bytes.", NotificationLevel.Error);
                return false;
            }

            return true;
        }

        /* Main method for launching the cipher */
        public void Execute()
        {
            ProgressChanged(0, 1);

            if (!checkParameters()) return;

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
            index = 2;

            /* Keysetup */
            for (int i = 0; i < inputKey.Length; i += 2)
                nfsr[i/2] = (uint)(inputKey[i + 1] << 8 | inputKey[i]);

            for (int i = 0; i < inputIV.Length - 1; i += 2)
                lfsr[i/2] = (uint)(inputIV[i + 1] << 8 | inputIV[i]);

            lfsr[4] = 0xffff;

            /* Clocking the cipher 160 times */
            for (int i = 0; i < 10; i++)
            {
                uint output = getOutput();
                nfsr = shift(nfsr, getOutputNFSR() ^ lfsr[0] ^ output);
                lfsr = shift(lfsr, getOutputLFSR() ^ output);
            }
        }

        /* Generate next NFSR bit */
        public uint getOutputNFSR()
        {
            uint b0 = nfsr[0];
            uint b9 = nfsr[0] >> 9 | nfsr[1] << 7;
            uint b14 = nfsr[0] >> 14 | nfsr[1] << 2;
            uint b15 = nfsr[0] >> 15 | nfsr[1] << 1;
            uint b21 = nfsr[1] >> 5 | nfsr[2] << 11;
            uint b28 = nfsr[1] >> 12 | nfsr[2] << 4;
            uint b33 = nfsr[2] >> 1 | nfsr[3] << 15;
            uint b37 = nfsr[2] >> 5 | nfsr[3] << 11;
            uint b45 = nfsr[2] >> 13 | nfsr[3] << 3;
            uint b52 = nfsr[3] >> 4 | nfsr[4] << 12;
            uint b60 = nfsr[3] >> 12 | nfsr[4] << 4;
            uint b62 = nfsr[3] >> 14 | nfsr[4] << 2;
            uint b63 = nfsr[3] >> 15 | nfsr[4] << 1;

            return (b62 ^ b60 ^ b52 ^ b45 ^ b37 ^ b33 ^ b28 ^ b21 ^ b14 ^ b9 ^ b0 ^ b63 & b60 ^ b37 & b33 
                ^ b15 & b9 ^ b60 & b52 & b45 ^ b33 & b28 & b21 ^ b63 & b45 & b28 & b9 ^ b60 & b52 & b37 & b33 
                ^ b63 & b60 & b21 & b15 ^ b63 & b60 & b52 & b45 & b37 ^ b33 & b28 & b21 & b15 & b9 
                ^ b52 & b45 & b37 & b33 & b28 & b21) & 0x0000FFFF;
        }

        /* Generate next LFSR bit */
        public uint getOutputLFSR()
        {
            uint s0 = lfsr[0];
            uint s13 = lfsr[0] >> 13 | lfsr[1] << 3;
            uint s23 = lfsr[1] >> 7 | lfsr[2] << 9;
            uint s38 = lfsr[2] >> 6 | lfsr[3] << 10;
            uint s51 = lfsr[3] >> 3 | lfsr[4] << 13;
            uint s62 = lfsr[3] >> 14 | lfsr[4] << 2;

            return (s0 ^ s13 ^ s23 ^ s38 ^ s51 ^ s62) & 0x0000FFFF;
        }

        /* Generate update function output */
        public uint getOutput()
        {
            uint b1 = nfsr[0] >> 1 | nfsr[1] << 15;
            uint b2 = nfsr[0] >> 2 | nfsr[1] << 14;
            uint b4 = nfsr[0] >> 4 | nfsr[1] << 12;
            uint b10 = nfsr[0] >> 10 | nfsr[1] << 6;
            uint b31 = nfsr[1] >> 15 | nfsr[2] << 1;
            uint b43 = nfsr[2] >> 11 | nfsr[3] << 5;
            uint b56 = nfsr[3] >> 8 | nfsr[4] << 8;
            uint b63 = nfsr[3] >> 15 | nfsr[4] << 1;
            uint s3 = lfsr[0] >> 3 | lfsr[1] << 13;
            uint s25 = lfsr[1] >> 9 | lfsr[2] << 7;
            uint s46 = lfsr[2] >> 14 | lfsr[3] << 2;
            uint s64 = lfsr[4];

            return (s25 ^ b63 ^ s3 & s64 ^ s46 & s64 ^ s64 & b63 ^ s3 & s25 & s46 ^ s3 & s46 & s64 ^ s3 & s46 & b63 
                ^ s25 & s46 & b63 ^ s46 & s64 & b63 ^ b1 ^ b2 ^ b4 ^ b10 ^ b31 ^ b43 ^ b56) & 0x0000FFFF;
        }

        /* Shifting the registers */
        public uint[] shift(uint[] array, uint val)
        {
            array[0] = array[1];
            array[1] = array[2];
            array[2] = array[3];
            array[3] = array[4];
            array[4] = val;

            return array;
        }

        /* One cipher round */
        public void Round()
        {
            uint output = getOutput();
            outp[0] = (byte)output;
            outp[1] = (byte)(output >> 8);

            nfsr = shift(nfsr, getOutputNFSR() ^ lfsr[0]);
            lfsr = shift(lfsr, getOutputLFSR());
        }

        /* Generate key stream byte */
        public byte getKeyStreamByte()
        {
            if (index > 1)
            {
                Round();
                index = 0;
            }
            return outp[index++];
        }

        /* Generate ciphertext */
        public byte[] encrypt(byte[] src)
        {
            byte[] dst = new byte[src.Length];

            for (int i = 0; i < src.Length; i++)
                dst[i] = (byte)(src[i] ^ getKeyStreamByte());

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

        public UserControl Presentation
        {
            get { return null; }
        }

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
