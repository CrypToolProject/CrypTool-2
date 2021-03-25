/*
   Copyright 2009 Sören Rinne, Ruhr-Universität Bochum, Germany

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
using System.Text;

using CrypTool.PluginBase;
using System.ComponentModel;
using System.Windows.Controls;
using CrypTool.PluginBase.Miscellaneous;
// for IControl
using CrypTool.PluginBase.Control;

namespace CrypTool.Trivium
{
    [Author("Soeren Rinne, David Oruba & Daehyun Strobel", "soeren.rinne@CrypTool.org", "Ruhr-Universitaet Bochum, Chair for Embedded Security (EmSec)", "http://www.trust.ruhr-uni-bochum.de/")]
    [PluginInfo("Trivium.Properties.Resources", "PluginCaption", "PluginTooltip", "Trivium/DetailedDescription/doc.xml", "Trivium/icon.png", "Trivium/Images/encrypt.png", "Trivium/Images/decrypt.png")]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class Trivium : ICrypComponent
    {
        #region IPlugin Members

        private TriviumSettings settings;
        private string inputString = null;
        private string outputString;
        private string inputKey;
        private string inputIV;
        private bool stop = false;

        const int lengthA = 93;
        const int lengthB = 84;
        const int lengthC = 111;

        #endregion

        #region Public Variables
        public List<uint> a = new List<uint>(new uint[lengthA]);
        public List<uint> b = new List<uint>(new uint[lengthB]);
        public List<uint> c = new List<uint>(new uint[lengthC]);
        #endregion

        public Trivium()
        {
            this.settings = new TriviumSettings();
        }

        public ISettings Settings
        {
            get { return (ISettings)this.settings; }
            set { this.settings = (TriviumSettings)value; }
        }

        [PropertyInfo(Direction.InputData, "InputStringCaption", "InputStringTooltip", true)]
        public string InputString
        {
            get { return this.inputString; }
            set
            {
                this.inputString = value;
                OnPropertyChanged("InputString");
            }
        }

        [PropertyInfo(Direction.InputData, "InputKeyCaption", "InputKeyTooltip", true)]
        public string InputKey
        {
            get { return this.inputKey; }
            set
            {
                this.inputKey = value;
                OnPropertyChanged("InputKey");
            }
        }

        [PropertyInfo(Direction.InputData, "InputIVCaption", "InputIVTooltip", true)]
        public string InputIV
        {
            get { return this.inputIV; }
            set
            {
                this.inputIV = value;
                OnPropertyChanged("InputIV");
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip", true)]
        public string OutputString
        {
            get { return this.outputString; }
            set
            {
                this.outputString = value;
                OnPropertyChanged("OutputString");
            }
        }

        public void Dispose()
        {
            stop = false;
            inputKey = null;
            outputString = null;
            inputString = null;
        }

        public int[] hextobin(string hex)
        {
            if (hex.Length % 2 == 1) hex += '0';
            List<int> bin = new List<int>();

            for (int i = 0; i < hex.Length; i+=2)
            {
                int z = Convert.ToInt32(hex.Substring(i, 2), 16);
                for (int j = 0; j < 8; j++, z >>= 1) bin.Insert(0, z & 1);
            }

            return bin.ToArray();
        }

        public string bintohex(string bin)
        {
            StringBuilder hex = new StringBuilder(bin.Length>>2);

            for (int i = 0; i < bin.Length; i += 8)
                hex.Append(String.Format("{0:X02}", Convert.ToByte( reverseString( bin.Substring(i,8) ), 2), 16));

            return hex.ToString();
        }

        public string ByteSwap(string s)
        {
            StringBuilder res = new StringBuilder(s.Length);

            for (int i = 0; i < s.Length; i += 8)
                res.Append( reverseString(s.Substring(i, 8)) );

            return res.ToString();
        }

        public string reverseString(string s)
        {
            char[] c = s.ToCharArray();
            Array.Reverse(c);
            return new string(c);
        }

        public void Execute()
        {
            try
            {
                int[] IV = hextobin( inputIV );
                int[] key = hextobin( inputKey );
                
                if (IV.Length > 80)
                {
                    GuiLogMessage("Initialization vector is too long (" + IV.Length + " bits). It can be at most 80 bits long!", NotificationLevel.Error);
                    return;
                }

                if (key.Length != 80)
                {
                    GuiLogMessage("Invalid key length (" + key.Length + " bits). It must be 80 bits long!", NotificationLevel.Error);
                    return;
                }

                GuiLogMessage("length of IV: " + IV.Length, NotificationLevel.Info);
                GuiLogMessage("length of key: " + key.Length, NotificationLevel.Info);
                
                int bitsToGenerate;

                if (settings.KeystreamLength > 0)
                {
                    if (settings.KeystreamLength % 32 != 0)
                    {
                        GuiLogMessage("Keystream length must be a multiple of 32. " + settings.KeystreamLength + " mod 32 = " + (settings.KeystreamLength % 32), NotificationLevel.Error);
                        return;
                    }
                    bitsToGenerate = settings.KeystreamLength;
                }
                else
                {
                    int bitsToPad = (32 - inputString.Length % 32) % 32;
                    GuiLogMessage("Bits to pad: " + bitsToPad, NotificationLevel.Info);
                    bitsToGenerate = inputString.Length;
                }

                bitsToGenerate = ((bitsToGenerate + 31) >> 5) << 5; // round to next bigger multiple of 32

                ProgressChanged(0, bitsToGenerate-1);

                // generate keystream
                DateTime startTime = DateTime.Now;

                GuiLogMessage("Starting encryption [Keysize=80 Bits]", NotificationLevel.Info);

                initTrivium(IV, key);
                string keystream = keystreamTrivium(bitsToGenerate);

                TimeSpan duration = DateTime.Now - startTime;

                if (settings.UseByteSwapping) keystream = ByteSwap(keystream);
                OutputString = settings.HexOutput ? bintohex(keystream) : keystream;

                if (stop)
                    GuiLogMessage("Aborted!", NotificationLevel.Info);
                else
                    GuiLogMessage("Encryption complete in " + duration + "! (input length : " + inputString.Length + ", keystream/output length: " + keystream.Length + " bit)", NotificationLevel.Info);

            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
            finally
            {
                ProgressChanged(1, 1);
            }
        }

        public void initTrivium(int[] IV, int[] key)
        {
            int i;

            for (i = 0; i < lengthA; i++) a[i] = 0;
            for (i = 0; i < lengthB; i++) b[i] = 0;
            for (i = 0; i < lengthC; i++) c[i] = 0;

            for (i = 0; i < 80; i++) a[i] = (uint)key[i];       // initialize 'a' with key bits
            for (i = 0; i < IV.Length; i++) b[i] = (uint)IV[i]; // initialize 'b' with IV bits
            for (i = 0; i < 3; i++) c[lengthC - 1 - i] = 1;     // initialize 'c'

            for (i = 0; i < settings.InitRounds; i++) Round();  // default: 1152 = 4 * 288
        }

        private uint Round()
        {
            uint result = (c[65] ^ c[110]) ^ (a[65] ^ a[92]) ^ (b[68] ^ b[83]);

            uint aa = (c[65] ^ c[110]) ^ (c[109] & c[108]) ^ a[68];
            uint bb = (a[65] ^ a[92]) ^ (a[91] & a[90]) ^ b[77];
            uint cc = (b[68] ^ b[83]) ^ (b[82] & b[81]) ^ c[86];

            a.Insert(0, aa);
            b.Insert(0, bb);
            c.Insert(0, cc);

            a.RemoveAt(lengthA);
            b.RemoveAt(lengthB);
            c.RemoveAt(lengthC);
            
            return result;
        }

        public string keystreamTrivium(int nBits)
        {
            StringBuilder builder = new StringBuilder(nBits);

            for (int i = 0; i < nBits; i++)
            {
                builder.Append(Round());
                if (stop) break;
                ProgressChanged(i, nBits - 1);
            }

            return builder.ToString();
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
            this.stop = true;
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

        #region IControl

        private IControlCubeAttack triviumSlave;
        [PropertyInfo(Direction.ControlSlave, "TriviumSlaveCaption", "TriviumSlaveTooltip")]
        public IControlCubeAttack TriviumSlave
        {
            get
            {
                if (triviumSlave == null)
                    triviumSlave = new CubeAttackControl(this);
                return triviumSlave;
            }
        }

        #endregion
    }

    #region TriviumControl : IControlCubeAttack

    public class CubeAttackControl : IControlCubeAttack
    {
        public event IControlStatusChangedEventHandler OnStatusChanged;
        private Trivium plugin;
        
        public CubeAttackControl(Trivium Plugin)
        {
            this.plugin = Plugin;
        }

        #region IControlEncryption Members

        public int GenerateBlackboxOutputBit(int[] IV, int[] key, int length)
        {
            if (key == null) // Online phase
                plugin.initTrivium(IV, plugin.hextobin(((TriviumSettings)plugin.Settings).InputKey));
            else // Preprocessing phase
                plugin.initTrivium(IV, key);
            return Int32.Parse(plugin.keystreamTrivium(length).Substring(plugin.keystreamTrivium(length).Length - 1, 1));
        }

        #endregion
    }

    #endregion
}