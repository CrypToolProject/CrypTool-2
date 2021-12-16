/*
   Copyright 2019 Axel Wehage

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
using System.Numerics;
using System.Windows.Controls;

namespace CrypTool.Plugins.BlindSignatureGenerator
{
    [Author("Axel Wehage", "axel.wehage@unibw.de", "Universität der Bundeswehr München", "https://www.unibw.de")]
    [PluginInfo("BlindSignatureGenerator.Properties.Resources", "BlindSignatureVerifierCaption", "BlindSignatureVerifierTooltip", "BlindSignatureGenerator/userdoc.xml", new[] { "BlindSignatureGenerator/Images/Icon2.png" })]
    [ComponentCategory(ComponentCategory.CiphersModernAsymmetric)]
    public class BlindSignatureVerifier : ICrypComponent
    {
        #region Private Variables
        private readonly BlindSignatureVerifierSettings settings = new BlindSignatureVerifierSettings();
        private BigInteger modulo;
        private BigInteger publickey;
        private BigInteger privatekey;
        private BigInteger blindsignaturenumber = 0;
        private BigInteger[] paillierSignature = null;
        private byte[] blindsignature = null;
        private byte[] signature;
        #endregion

        #region Data Properties
        [PropertyInfo(Direction.InputData, "ModuloN", "ModuloNTooltip")]
        public BigInteger Modulo
        {
            get => modulo;
            set => modulo = value;
        }

        [PropertyInfo(Direction.InputData, "PublicKey", "PublicKeyTooltip")]
        public BigInteger PublicKey
        {
            get => publickey;
            set => publickey = value;
        }

        [PropertyInfo(Direction.InputData, "BlindSignatureIn", "BlindSignatureInTooltip")]
        public byte[] BlindSignatureIn
        {
            get => blindsignature;
            set => blindsignature = value;
        }

        [PropertyInfo(Direction.InputData, "BlindSignatureAsNumberIn", "BlindSignatureAsNumberInTooltip")]
        public BigInteger BlindSignatureNumberIn
        {
            get => blindsignaturenumber;
            set => blindsignaturenumber = value;
        }

        [PropertyInfo(Direction.InputData, "BlindSignaturePaillierIn", "BlindSignaturePaillierInTooltip")]
        public BigInteger[] BlindSignaturePaillierIn
        {
            get => paillierSignature;
            set => paillierSignature = value;
        }

        [PropertyInfo(Direction.OutputData, "DecryptedBlindSignature", "DecryptedBlindSignatureTooltip")]
        public byte[] BlindSignature
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "DecryptedBlindSignatureAsNumber", "DecryptedBlindSignatureAsNumberTooltip")]
        public BigInteger BlindSignatureNumber
        {
            get;
            set;
        }
        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings => settings;

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation => null;

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            blindsignaturenumber = 0;
            paillierSignature = null;
            blindsignature = null;
        }

        public static string ByteArrayToHexString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex;
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 100);
            BigInteger temp = 0;
            if (BlindSignatureIn != null)
            {
                temp = new BigInteger(BlindSignatureIn);
            }
            if (BlindSignatureNumberIn != 0)
            {
                temp = BlindSignatureNumberIn;
            }

            BigInteger s1 = 0;
            BigInteger s2 = 0;
            if (BlindSignaturePaillierIn != null && BlindSignaturePaillierIn[0] != 0 && BlindSignaturePaillierIn[1] != 0)
            {
                s1 = BlindSignaturePaillierIn[0];
                s2 = BlindSignaturePaillierIn[1];
            }
            switch (settings.SigningAlgorithm)
            {
                case BlindSignatureVerifierSettings.SigningMode.RSA:
                    temp = BigInteger.ModPow(temp, PublicKey, Modulo);
                    signature = temp.ToByteArray();
                    break;
                case BlindSignatureVerifierSettings.SigningMode.Paillier:
                    BigInteger Modulo2 = BigInteger.Pow(Modulo, 2);
                    temp = (BigInteger.ModPow(PublicKey, s1, Modulo2) * BigInteger.ModPow(s2, Modulo, Modulo2)) % Modulo2;
                    signature = temp.ToByteArray();
                    break;
            }
            BlindSignature = signature;
            OnPropertyChanged("BlindSignature");

            BlindSignatureNumber = new BigInteger(signature);
            OnPropertyChanged("BlindSignatureNumber");


            if (settings.SigningAlgorithm < 0)
            {
                GuiLogMessage("You broke something about the SigningAlgorithm", NotificationLevel.Debug);
            }
            ProgressChanged(100, 100);
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
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

        #endregion
    }
}
