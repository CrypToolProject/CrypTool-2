/*
   Copyright 2019 Nils Kopal <Nils.Kopal<at>CrypTool.org

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
using System.Windows.Controls;

namespace CrypTool.Plugins.FEAL
{
    /// <summary>
    /// Interaktionslogik für FEAL4Presentation.xaml
    /// </summary>
    [PluginBase.Attributes.Localization("CrypTool.Plugins.FEAL.Properties.Resources")]
    public partial class FEAL4Presentation : UserControl
    {
        public FEAL4Presentation()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Visualizes the encryption of the given block
        /// </summary>
        /// <param name="inputblock"></param>
        /// <param name="key"></param>
        public void VisualizeEncryptBlock(byte[] inputblock, byte[] key)
        {
            byte[][] roundkeys = FEAL_Algorithms.K(key, 6);
            byte[] ciphertextBlock = FEAL_Algorithms.FEAL4_EncryptBlock(inputblock, key);

            KeyRound1.RoundKeyNames = "(K0,K1)";
            KeyRound2.RoundKeyNames = "(K2,K3)";
            KeyRound3.RoundKeyNames = "(K4,K5)";
            KeyRound4.RoundKeyNames = "(K6,K7)";
            KeyRound5.RoundKeyNames = "(K8,K9)";
            KeyRound6.RoundKeyNames = "(Ka,Kb)";

            KeyRound1.RoundKey = ByteArrayToHexString(FEAL_Algorithms.Concat(roundkeys[0], roundkeys[1]));
            KeyRound2.RoundKey = ByteArrayToHexString(FEAL_Algorithms.Concat(roundkeys[2], roundkeys[3]));
            KeyRound3.RoundKey = ByteArrayToHexString(FEAL_Algorithms.Concat(roundkeys[4], roundkeys[5]));
            KeyRound4.RoundKey = ByteArrayToHexString(FEAL_Algorithms.Concat(roundkeys[6], roundkeys[7]));
            KeyRound5.RoundKey = ByteArrayToHexString(FEAL_Algorithms.Concat(roundkeys[8], roundkeys[9]));
            KeyRound6.RoundKey = ByteArrayToHexString(FEAL_Algorithms.Concat(roundkeys[10], roundkeys[11]));

            InputBlock.Content = ByteArrayToHexString(inputblock);
            Key.Content = ByteArrayToHexString(key);

            FirstInputKeyNames.Content = "(K4,K5,K6,K7)";
            CryptRound1.RoundKeyName = "K0";
            CryptRound2.RoundKeyName = "K1";
            CryptRound3.RoundKeyName = "K2";
            CryptRound4.RoundKeyName = "K3";
            LastInputKeyNames.Content = "(K8,K9,Ka,Kb)";

            FirstInputKeys.Content = (ByteArrayToHexString(FEAL_Algorithms.Concat(roundkeys[4], FEAL_Algorithms.Concat(roundkeys[5], FEAL_Algorithms.Concat(roundkeys[6], roundkeys[7])))));
            CryptRound1.RoundKey = ByteArrayToHexString(roundkeys[0]);
            CryptRound2.RoundKey = ByteArrayToHexString(roundkeys[1]);
            CryptRound3.RoundKey = ByteArrayToHexString(roundkeys[2]);
            CryptRound4.RoundKey = ByteArrayToHexString(roundkeys[3]);
            LastInputKeys.Content = ByteArrayToHexString(FEAL_Algorithms.Concat(roundkeys[8], FEAL_Algorithms.Concat(roundkeys[9], FEAL_Algorithms.Concat(roundkeys[10], roundkeys[11]))));

            OutputBlock.Content = ByteArrayToHexString(ciphertextBlock);

        }

        /// <summary>
        /// Visualizes the decryption of the given block
        /// </summary>
        /// <param name="inputblock"></param>
        /// <param name="key"></param>
        public void VisualizeDecryptBlock(byte[] inputblock, byte[] key)
        {
            byte[][] roundkeys = FEAL_Algorithms.K(key, 6);
            byte[] ciphertextBlock = FEAL_Algorithms.FEAL4_DecryptBlock(inputblock, key);

            KeyRound1.RoundKeyNames = "(K0,K1)";
            KeyRound2.RoundKeyNames = "(K2,K3)";
            KeyRound3.RoundKeyNames = "(K4,K5)";
            KeyRound4.RoundKeyNames = "(K6,K7)";
            KeyRound5.RoundKeyNames = "(K8,K9)";
            KeyRound6.RoundKeyNames = "(Ka,Kb)";

            KeyRound1.RoundKey = ByteArrayToHexString(FEAL_Algorithms.Concat(roundkeys[0], roundkeys[1]));
            KeyRound2.RoundKey = ByteArrayToHexString(FEAL_Algorithms.Concat(roundkeys[2], roundkeys[3]));
            KeyRound3.RoundKey = ByteArrayToHexString(FEAL_Algorithms.Concat(roundkeys[4], roundkeys[5]));
            KeyRound4.RoundKey = ByteArrayToHexString(FEAL_Algorithms.Concat(roundkeys[6], roundkeys[7]));
            KeyRound5.RoundKey = ByteArrayToHexString(FEAL_Algorithms.Concat(roundkeys[8], roundkeys[9]));
            KeyRound6.RoundKey = ByteArrayToHexString(FEAL_Algorithms.Concat(roundkeys[10], roundkeys[11]));

            InputBlock.Content = ByteArrayToHexString(inputblock);
            Key.Content = ByteArrayToHexString(key);

            FirstInputKeyNames.Content = "(K8,K9,Ka,Kb)";
            CryptRound1.RoundKeyName = "K3";
            CryptRound2.RoundKeyName = "K2";
            CryptRound3.RoundKeyName = "K1";
            CryptRound4.RoundKeyName = "K0";
            LastInputKeyNames.Content = "(K4,K5,K6,K7)";

            FirstInputKeys.Content = (ByteArrayToHexString(FEAL_Algorithms.Concat(roundkeys[8], FEAL_Algorithms.Concat(roundkeys[9], FEAL_Algorithms.Concat(roundkeys[10], roundkeys[11])))));
            CryptRound1.RoundKey = ByteArrayToHexString(roundkeys[3]);
            CryptRound2.RoundKey = ByteArrayToHexString(roundkeys[2]);
            CryptRound3.RoundKey = ByteArrayToHexString(roundkeys[1]);
            CryptRound4.RoundKey = ByteArrayToHexString(roundkeys[0]);
            LastInputKeys.Content = ByteArrayToHexString(FEAL_Algorithms.Concat(roundkeys[4], FEAL_Algorithms.Concat(roundkeys[5], FEAL_Algorithms.Concat(roundkeys[6], roundkeys[7]))));

            OutputBlock.Content = ByteArrayToHexString(ciphertextBlock);
        }

        /// <summary>
        /// Converts a byte array to a hex string
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private string ByteArrayToHexString(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "").ToUpper();
        }
    }
}
