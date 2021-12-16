/*
   Copyright 2011 CrypTool 2 Team <ct2contact@CrypTool.org>

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, 
   software distributed under the License is distributed on an 
   "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
   either express or implied. See the License for the specific 
   language governing permissions and limitations under the License.
*/
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System.ComponentModel;
using System.Windows.Controls;

namespace CrypTool.Plugins.Mickey2
{
    [Author("Robin Nelle", "rnelle@mail.uni-mannheim.de", "Uni Mannheim - Lehrstuhl Prof. Dr. Armknecht", "http://ls.wim.uni-mannheim.de/")]
    [PluginInfo("Mickey2.Properties.Resources", "PluginCaption", "PluginTooltip", "Mickey2/DetailedDescription/doc.xml", "Mickey2/Images/icon.png")]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class Mickey2 : ICrypComponent
    {
        #region Private Variables

        private byte[] inputData;
        private byte[] outputData;
        private byte[] inputKey;
        private byte[] inputIV;

        public Mickey2Settings settings = new Mickey2Settings();

        private readonly uint[] registerR = new uint[4];
        private readonly uint[] registerS = new uint[4];

        private readonly uint[] R_Mask = new uint[4] { 0x1279327b, 0xb5546660, 0xdf87818f, 0x00000003 };
        private readonly uint[] S_Mask0 = new uint[4] { 0x9ffa7faf, 0xaf4a9381, 0x9cec5802, 0x00000001 };
        private readonly uint[] S_Mask1 = new uint[4] { 0x4c8cb877, 0x4911b063, 0x40fbc52b, 0x00000008 };
        private readonly uint[] Comp0 = new uint[4] { 0x6aa97a30, 0x7942a809, 0x057ebfea, 0x00000006 };
        private readonly uint[] Comp1 = new uint[4] { 0xdd629e9a, 0xe3a21d63, 0x91c23dd7, 0x00000001 };

        #endregion

        public ISettings Settings
        {
            get => settings;
            set => settings = (Mickey2Settings)value;
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

        #region IPlugin Members

        public UserControl Presentation => null;

        public void PreExecution()
        {
            Dispose();
        }

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
                GuiLogMessage("Wrong key length " + inputKey.Length + " bytes. Key length must be 10 bytes (80 bits).", NotificationLevel.Error);
                return false;
            }

            if (inputIV.Length > 10)
            {
                GuiLogMessage("Wrong IV length " + inputIV.Length + " bytes. IV length must be <= 10 bytes (80 bits).", NotificationLevel.Error);
                return false;
            }

            return true;
        }

        public void Execute()
        {
            ProgressChanged(0, 1);

            if (!checkParameters())
            {
                return;
            }

            initMickey();

            OutputData = encrypt(inputData);

            ProgressChanged(1, 1);
        }

        public void PostExecution()
        {
            Dispose();
        }

        public void Stop()
        {
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
            inputData = null;
            inputKey = null;
            inputIV = null;
            outputData = null;
        }

        #endregion

        /* Generate key stream byte */
        public byte getKeyStreamByte()
        {
            uint result = 0;

            for (int i = 7; i >= 0; i--)
            {
                result |= CLOCK_KG(false, 0) << i;
            }

            return (byte)result;
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

        # region clocking

        ///Clocking the register R
        public void CLOCK_R(uint INPUT_BIT, uint CONTROL_BIT)
        {
            uint FEEDBACK_BIT = ((registerR[3] >> 3) & 1) ^ INPUT_BIT;
            uint Carry0 = (registerR[0] >> 31) & 1;
            uint Carry1 = (registerR[1] >> 31) & 1;
            uint Carry2 = (registerR[2] >> 31) & 1;

            if (CONTROL_BIT == 1)
            {
                /* Shift and xor */
                registerR[0] ^= (registerR[0] << 1);
                registerR[1] ^= (registerR[1] << 1) ^ Carry0;
                registerR[2] ^= (registerR[2] << 1) ^ Carry1;
                registerR[3] ^= (registerR[3] << 1) ^ Carry2;
            }
            else
            {
                /* Shift only */
                registerR[0] = (registerR[0] << 1);
                registerR[1] = (registerR[1] << 1) ^ Carry0;
                registerR[2] = (registerR[2] << 1) ^ Carry1;
                registerR[3] = (registerR[3] << 1) ^ Carry2;
            }

            if (FEEDBACK_BIT == 1)
            {
                registerR[0] ^= R_Mask[0];
                registerR[1] ^= R_Mask[1];
                registerR[2] ^= R_Mask[2];
                registerR[3] ^= R_Mask[3];
            }
        }

        ///Clocking the register S
        public void CLOCK_S(uint INPUT_BIT, uint CONTROL_BIT)
        {
            uint FEEDBACK_BIT = ((registerS[3] >> 3) & 1) ^ INPUT_BIT;
            uint Carry0 = (registerS[0] >> 31) & 1;
            uint Carry1 = (registerS[1] >> 31) & 1;
            uint Carry2 = (registerS[2] >> 31) & 1;

            registerS[0] = (registerS[0] << 1) ^ ((registerS[0] ^ Comp0[0]) & ((registerS[0] >> 1) ^ (registerS[1] << 31) ^ Comp1[0]) & 0xfffffffe);
            registerS[1] = (registerS[1] << 1) ^ ((registerS[1] ^ Comp0[1]) & ((registerS[1] >> 1) ^ (registerS[2] << 31) ^ Comp1[1])) ^ Carry0;
            registerS[2] = (registerS[2] << 1) ^ ((registerS[2] ^ Comp0[2]) & ((registerS[2] >> 1) ^ (registerS[3] << 31) ^ Comp1[2])) ^ Carry1;
            registerS[3] = (registerS[3] << 1) ^ ((registerS[3] ^ Comp0[3]) & ((registerS[3] >> 1) ^ Comp1[3]) & 0x7) ^ Carry2;

            /* Apply suitable feedback from s_99 */
            if (FEEDBACK_BIT == 1)
            {
                if (CONTROL_BIT == 1)
                {
                    registerS[0] ^= S_Mask1[0];
                    registerS[1] ^= S_Mask1[1];
                    registerS[2] ^= S_Mask1[2];
                    registerS[3] ^= S_Mask1[3];
                }
                else
                {
                    registerS[0] ^= S_Mask0[0];
                    registerS[1] ^= S_Mask0[1];
                    registerS[2] ^= S_Mask0[2];
                    registerS[3] ^= S_Mask0[3];
                }
            }
        }

        //Clocking the overall generator
        public uint CLOCK_KG(bool MIXING, uint INPUT_BIT)
        {
            uint Keystream_bit = (registerR[0] ^ registerS[0]) & 1;

            uint CONTROL_BIT_R = ((registerS[1] >> 2) ^ (registerR[2] >> 3)) & 1;
            uint CONTROL_BIT_S = ((registerR[1] >> 1) ^ (registerS[2] >> 3)) & 1;

            if (MIXING)
            {
                CLOCK_R(((registerS[1] >> 18) & 1) ^ INPUT_BIT, CONTROL_BIT_R);
            }
            else
            {
                CLOCK_R(INPUT_BIT, CONTROL_BIT_R);
            }

            CLOCK_S(INPUT_BIT, CONTROL_BIT_S);

            return Keystream_bit;
        }

        # endregion

        public void initMickey()
        {
            //Initialise the registers with all zeros.
            for (int i = 0; i < registerR.Length; i++)
            {
                registerR[i] = 0;
                registerS[i] = 0;
            }

            // IV setup
            for (int i = 0; i < inputIV.Length; i++)
            {
                for (int j = 7; j >= 0; j--)
                {
                    CLOCK_KG(true, (uint)(inputIV[i] >> j) & 1);
                }
            }

            // Key setup
            for (int i = 0; i < InputKey.Length; i++)
            {
                for (int j = 7; j >= 0; j--)
                {
                    CLOCK_KG(true, (uint)(InputKey[i] >> j) & 1);
                }
            }

            // Preclock
            for (int i = 0; i < 100; i++)
            {
                CLOCK_KG(true, 0);
            }
        }

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler
            OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler
            OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, NotificationLevel
            logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured,
                this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this,
                new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged,
                this, new PluginProgressEventArgs(value, max));
        }

        #endregion
    }
}
