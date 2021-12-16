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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CrypTool.ADFGVX
{
    [Author("Matthäus Wander", "wander@CrypTool.org", "Uni Duisburg-Essen", "http://www.vs.uni-due.de")]
    [PluginInfo("CrypTool.ADFGVX.Properties.Resources", "PluginCaption", "PluginTooltip", "ADFGVX/DetailedDescription/doc.xml", "ADFGVX/Images/icon.png", "ADFGVX/Images/encrypt.png", "ADFGVX/Images/decrypt.png")]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class ADFGVX : ICrypComponent
    {
        #region Variables, properties and constructor

        private ADFGVXSettings settings;
        private Dictionary<char, string> plainCharToCipherBigram; // 'X' -> "FV"
        private Dictionary<string, char> cipherBigramToPlainChar; // "FV" -> 'X'

        /// <summary>
        /// Constructor
        /// </summary>
        public ADFGVX()
        {
            settings = new ADFGVXSettings();
        }

        /// <summary>
        /// Get or set settings for the algorithm
        /// </summary>
        public ISettings Settings
        {
            get => settings;
            set => settings = (ADFGVXSettings)value;
        }

        [PropertyInfo(Direction.InputData, "InputStringCaption", "InputStringTooltip", true)]
        public string InputString
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "SubstitutionPassCaption", "SubstitutionPassTooltip", false)]
        public string SubstitutionPass
        {
            get => settings.SubstitutionPass;
            set
            {
                if (value != null && value != settings.SubstitutionPass)
                {
                    settings.SubstitutionPass = value;
                    OnPropertyChanged("SubstitutionPass");
                }
            }
        }

        [PropertyInfo(Direction.InputData, "TranspositionPassCaption", "TranspositionPassTooltip", false)]
        public string TranspositionPass
        {
            get => settings.TranspositionPass;
            set
            {
                if (value != null && value != settings.TranspositionPass)
                {
                    settings.TranspositionPass = value;
                    OnPropertyChanged("TranspositionPass");
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip", false)]
        public string OutputString
        {
            get;
            set;
        }

        #endregion

        #region Implementation

        private void encrypt()
        {
            if (string.IsNullOrEmpty(InputString))
            {
                return;
            }

            // create empty builder with enough capacity
            StringBuilder substitute = new StringBuilder(InputString.Length * 2);

            // Step 1: Substititon
            foreach (char c in InputString)
            {
                char upChar = char.ToUpperInvariant(c);
                if (plainCharToCipherBigram.ContainsKey(upChar))
                {
                    substitute.Append(plainCharToCipherBigram[upChar]);
                }
            }

            ProgressChanged(50, 100);

            // Step 2: Transposition
            int[] columnOrder = settings.KeyColumnOrder;
            int[] columnOrderInv = new int[columnOrder.Length];
            for (int i = 0; i < columnOrder.Length; i++)
            {
                columnOrderInv[columnOrder[i]] = i;
            }

            StringBuilder transpOut = new StringBuilder(substitute.Length);

            foreach (int order in columnOrderInv)
            {
                for (int j = order; j < substitute.Length; j += columnOrder.Length)
                {
                    transpOut.Append(substitute[j]);
                }

            }

            ProgressChanged(100, 100);
            OutputString = transpOut.ToString();
            OnPropertyChanged("OutputString");
        }

        private void decrypt()
        {
            if (string.IsNullOrEmpty(InputString))
            {
                return;
            }

            string cipherAlphabet = settings.CipherAlphabet;

            // Remove whitespaces, check for invalid characters
            StringBuilder ciphertext = new StringBuilder(InputString.Length);
            foreach (char c in InputString)
            {
                // ignore whitespaces silently
                if (char.IsWhiteSpace(c))
                {
                    continue;
                }

                // abort if unknown character encountered
                char upChar = char.ToUpperInvariant(c);
                if (!cipherAlphabet.Contains(upChar))
                {
                    ADFGVX_LogMessage(string.Format("Ciphertext contains invalid character: {0}", c), NotificationLevel.Error);
                    return;
                }

                ciphertext.Append(upChar);
            }

            if (ciphertext.Length % 2 != 0)
            {
                ADFGVX_LogMessage("Ciphertext length is not multiple of 2", NotificationLevel.Warning);
            }

            // Step 1: Transposition
            int[] columnOrder = settings.KeyColumnOrder;
            int[] columnOrderInv = new int[columnOrder.Length];
            for (int i = 0; i < columnOrder.Length; i++)
            {
                columnOrderInv[columnOrder[i]] = i;
            }

            char[] transpOut = new char[ciphertext.Length];

            int textPointer = 0;
            foreach (int order in columnOrderInv)
            {
                for (int j = order; j < transpOut.Length; j += columnOrderInv.Length)
                {
                    if (textPointer > ciphertext.Length)
                    {
                        ADFGVX_LogMessage("Incorrect decryption of transposition stage: ciphertext too short", NotificationLevel.Error);
                        return; // abort
                    }
                    transpOut[j] = ciphertext[textPointer++];
                }
            }

            ProgressChanged(50, 100);

            if (textPointer < ciphertext.Length)
            {
                ADFGVX_LogMessage("Incorrect decryption of transposition stage: ciphertext too long", NotificationLevel.Warning);
            }

            // Step 2: Substitution
            StringBuilder plaintext = new StringBuilder(ciphertext.Length / 2);
            for (int i = 0; (i + 1) < ciphertext.Length; i += 2)
            {
                string cipherBigram = "" + transpOut[i] + transpOut[i + 1];
                if (!cipherBigramToPlainChar.ContainsKey(cipherBigram))
                {
                    ADFGVX_LogMessage(string.Format("Ciphertext bigram not found in lookup table: {0}", cipherBigram), NotificationLevel.Error);
                    return;
                }

                plaintext.Append(cipherBigramToPlainChar[cipherBigram]);
            }

            ProgressChanged(100, 100);
            OutputString = plaintext.ToString();
            OnPropertyChanged("OutputString");
        }

        #endregion

        #region IPlugin Members

        public void Dispose()
        {
        }

        public void Execute()
        {
            BuildTables();

            switch (settings.Action)
            {
                case ADFGVXSettings.ActionEnum.Encrypt:
                    encrypt();
                    break;
                case ADFGVXSettings.ActionEnum.Decrypt:
                    decrypt();
                    break;
            }
        }

        public void Initialize()
        {
        }

        public void PostExecution()
        {
        }

        public void PreExecution()
        {
        }

        private void BuildTables()
        {
            string substMatrix = settings.SubstitutionMatrix;
            string cipherAlphabet = settings.CipherAlphabet;

            // build lookup tables for encryption/decryption depending on variant choice (ADFGX/ADFGVX)
            plainCharToCipherBigram = new Dictionary<char, string>();
            cipherBigramToPlainChar = new Dictionary<string, char>();
            for (int i = 0; i < substMatrix.Length; i++)
            {
                char plain = substMatrix[i];

                int row = i / cipherAlphabet.Length;
                int column = i % cipherAlphabet.Length;

                string cipher = "" + cipherAlphabet[row] + cipherAlphabet[column];
                plainCharToCipherBigram[plain] = cipher;
                cipherBigramToPlainChar[cipher] = plain;
            }
        }

        public System.Windows.Controls.UserControl Presentation => null;

        public void Stop()
        {
        }

        #endregion

        #region Event handling

        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        private void ADFGVX_LogMessage(string msg, NotificationLevel loglevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(msg, this, loglevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(int curr, int max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(curr, max));
        }

        #endregion
    }


}
