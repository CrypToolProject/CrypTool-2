/*
   Copyright 2011 CrypTool 2 Team <ct2contact@cryptool.org>

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
using System.ComponentModel;
using System.Windows.Controls;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System;

namespace CrypTool.Plugins.Rabbit
{
    [Author("Robin Nelle", "rnelle@mail.uni-mannheim.de", "Uni Mannheim - Lehrstuhl Prof. Dr. Armknecht", "http://ls.wim.uni-mannheim.de/")]
    [PluginInfo("Rabbit.Properties.Resources", "PluginCaption", "PluginTooltip", "Rabbit/DetailedDescription/doc.xml", "Rabbit/Images/icon.jpg")]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class Rabbit : ICrypComponent
    {
        #region Private Variables

        private uint[] x = new uint[8];
        private uint[] c = new uint[8];
        private uint carry;

        private byte[] inputData;
        private byte[] inputKey;
        private byte[] inputIV;
        private byte[] outputData;

        private RabbitSettings settings = new RabbitSettings();

        #endregion

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

        #region IPlugin Members

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

            if (inputKey.Length != 16)
            {
                GuiLogMessage("Wrong key length " + inputKey.Length + " bytes. Key length must be 16 bytes.", NotificationLevel.Error);
                return false;
            }
            
            if (inputIV.Length != 8)
            {
                GuiLogMessage("Wrong IV length " + inputIV.Length + " bytes. IV length must be 8 bytes.", NotificationLevel.Error);
                return false;
            }

            return true;
        }

        public void Execute()
        {
            ProgressChanged(0, 1);

            if (!checkParameters()) return;

            init();

            OutputData = encrypt(inputData);

            ProgressChanged(1, 1);
        }

        public void init()
        {
            for (int i = 0; i < 8; i++)
            {
                x[i] = 0;
                c[i] = 0;
            }
            carry = 0;

            keySetup(inputKey);
            iv_setup(inputIV);
        }

        // Square a 32-bit unsigned integer to obtain the 64-bit result 
        //and return the upper 32 bits XOR the lower 32 bits 
        private uint g_func(uint x)
        {
            // Temporary variables
            uint a, b, h, l;

            // Construct high and low argument for squaring
            a = x & 0xffff;
            b = x >> 16;

            // Calculate high and low result of squaring
            h = (((((a * a) >> 17) + (a * b)) >> 15)) + (b * b);
            l = x * x;

            // Return high XOR low
            return h ^ l;
        }

        // Left rotation of a 32-bit
        private uint rotateLeft(uint x, int y)
        {
            return (x << y) | (x >> (32 - y));
        }

        // Calculate the next internal state
        private void next_state()
        {
            // Temporary variables
            uint[] g = new uint[8];
            uint[] c_old = new uint[8];

            // Save old counter values
            for (int i = 0; i < 8; i++) 
                c_old[i] = c[i];

            //Calculate new counter values
            c[0] += 0x4d34d34d + carry;
            c[1] += 0xd34d34d3 + ((c[0] < c_old[0]) ? 1u : 0u);
            c[2] += 0x34d34d34 + ((c[1] < c_old[1]) ? 1u : 0u);
            c[3] += 0x4d34d34d + ((c[2] < c_old[2]) ? 1u : 0u);
            c[4] += 0xd34d34d3 + ((c[3] < c_old[3]) ? 1u : 0u);
            c[5] += 0x34d34d34 + ((c[4] < c_old[4]) ? 1u : 0u);
            c[6] += 0x4d34d34d + ((c[5] < c_old[5]) ? 1u : 0u);
            c[7] += 0xd34d34d3 + ((c[6] < c_old[6]) ? 1u : 0u);
            carry = ((c[7] < c_old[7]) ? 1u : 0u);

            // Calculate the g-functions
            for (int i = 0; i < 8; i++)
                g[i] = g_func(x[i] + c[i]);

            // Calculate new state values
            x[0] = g[0] + rotateLeft(g[7], 16) + rotateLeft(g[6], 16);
            x[1] = g[1] + rotateLeft(g[0], 8) + g[7];
            x[2] = g[2] + rotateLeft(g[1], 16) + rotateLeft(g[0], 16);
            x[3] = g[3] + rotateLeft(g[2], 8) + g[1];
            x[4] = g[4] + rotateLeft(g[3], 16) + rotateLeft(g[2], 16);
            x[5] = g[5] + rotateLeft(g[4], 8) + g[3];
            x[6] = g[6] + rotateLeft(g[5], 16) + rotateLeft(g[4], 16);
            x[7] = g[7] + rotateLeft(g[6], 8) + g[5];
        }

        //transform byte[] to uint
        public static uint b2u(byte[] a, int i)
        {
            return ((uint)a[i + 3] << 24) | ((uint)a[i + 2] << 16) | (uint)(a[i + 1] << 8) | a[i];
        }

        //transform uint to byte[]
        public byte[] u2b(uint x)
        {
            byte[] s = new byte[4];

            s[0] = (byte)x;
            s[1] = (byte)(x >> 8);
            s[2] = (byte)(x >> 16);
            s[3] = (byte)(x >> 24);

            return s;
        }

        // Initialize the cipher instance as a function of the key (p_key)
        public void keySetup(byte[] p_key)
        {
            //Temporary variables
            uint k0, k1, k2, k3, i;

            // Generate four subkeys

            k0 = b2u(p_key, 0);
            k1 = b2u(p_key, 4);
            k2 = b2u(p_key, 8);
            k3 = b2u(p_key, 12);

            // Generate initial state variables
            x[0] = k0;
            x[2] = k1;
            x[4] = k2;
            x[6] = k3;
            x[1] = (k3 << 16) | (k2 >> 16);
            x[3] = (k0 << 16) | (k3 >> 16);
            x[5] = (k1 << 16) | (k0 >> 16);
            x[7] = (k2 << 16) | (k1 >> 16);

            // Generate initial counter values
            c[0] = rotateLeft(k2, 16);
            c[2] = rotateLeft(k3, 16);
            c[4] = rotateLeft(k0, 16);
            c[6] = rotateLeft(k1, 16);
            c[1] = (k0 & 0xffff0000) | (k1 & 0x0000ffff);
            c[3] = (k1 & 0xffff0000) | (k2 & 0x0000ffff);
            c[5] = (k2 & 0xffff0000) | (k3 & 0x0000ffff);
            c[7] = (k3 & 0xffff0000) | (k0 & 0x0000ffff);

            // Clear carry bit 
            carry = 0;

            // Iterate the system four times
            for (i = 0; i < 4; i++) 
                next_state(); 

            // Modify the counters
            for (i = 0; i < 8; i++) 
                c[(i + 4) & 0x7] ^= x[i]; 
        }

        #endregion

        #region iv setup

        // Initialize the cipher instance as a function of the IV (p_iv) 
        void iv_setup(byte[] p_iv)
        {
            // Temporary variables 
            uint i0, i1, i2, i3, i;

            // Generate four subvectors
            i0 = b2u(p_iv, 0);
            i2 = b2u(p_iv, 4);
            i1 = (i0 >> 16) | (i2 & 0xFFFF0000);
            i3 = (i2 << 16) | (i0 & 0x0000FFFF);

            // Modify counter values 
            c[0] ^= i0;
            c[1] ^= i1;
            c[2] ^= i2;
            c[3] ^= i3;
            c[4] ^= i0;
            c[5] ^= i1;
            c[6] ^= i2;
            c[7] ^= i3;

            //Iterate the system four times 
            for (i = 0; i < 4; i++)
                next_state();
        }

        #endregion

        // get 16 bytes of the keystream
        private void getStreamBlock( byte[] buf )
        {
            uint[] k = new uint[4];
            byte[] t = new byte[4];

            next_state();

            k[0] = x[0] ^ (x[5] >> 16) ^ (x[3] << 16);
            k[1] = x[2] ^ (x[7] >> 16) ^ (x[5] << 16);
            k[2] = x[4] ^ (x[1] >> 16) ^ (x[7] << 16);
            k[3] = x[6] ^ (x[3] >> 16) ^ (x[1] << 16);

            // copy keystream bytes to buf
            for (int i = 0; i < 4; i++)
            {
                t = u2b(k[i]);

                for (int j = 0; j < 4; j++)
                    buf[4*i + j] = t[j];
            }
        }

        // Encrypt or decrypt data 
        public byte[] encrypt(byte[] src)
        {
            byte[] dst = new byte[src.Length];
            byte[] keystream = new byte[16];

            for (int i = 0; i < src.Length;)
            {
                getStreamBlock(keystream);
                for (int j = 0; j < 16 && i < src.Length; i++, j++)
                    dst[i] = (byte)(src[i] ^ keystream[j]);
            }

            return dst;
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

        public ISettings Settings
        {
            get { return settings; }
        }

        public UserControl Presentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
            Dispose();
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
    }
}