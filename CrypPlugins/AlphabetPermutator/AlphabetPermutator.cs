/*                              
   Copyright 2022 Nils Kopal, CrypTool Project

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
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;

namespace AlphabetPermutator
{
    [Author("Nils Kopal", "Nils.Kopal@cryptool.org", "CrypTool Project", "http://www.cryptool.org")]
    [PluginInfo("AlphabetPermutator.Properties.Resources", "PluginCaption", "PluginTooltip", "AlphabetPermutator/DetailedDescription/doc.xml", "AlphabetPermutator/icon.png")]
    [ComponentCategory(ComponentCategory.ToolsDataflow)]
    public class AlphabetPermutator : ICrypComponent
    {
        private readonly AlphabetPermutatorSettings _settings = new AlphabetPermutatorSettings();
        private const string LATIN_ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        [PropertyInfo(Direction.InputData, "SourceAlphabetCaption", "SourceAlphabetTooltip", false)]
        public string SourceAlphabet
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "KeywordCaption", "KeywordTooltip", false)]
        public string Keyword
        {
            get;
            set;
        }
        [PropertyInfo(Direction.InputData, "ShiftCaption", "ShiftTooltip", false)]
        public int Shift
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "Keyword2Caption", "Keyword2Tooltip", false)]
        public string Keyword2
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "Shift2Caption", "Shift2Tooltip", false)]
        public int Shift2
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "PlaintextAlphabetCaption", "PlaintextAlphabetTooltip")]
        public string PlaintextAlphabet
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "CiphertextAlphabetCaption", "CiphertextAlphabetTooltip")]
        public string CiphertextAlphabet
        {
            get;
            set;
        }

        public void PreExecution()
        {
            SourceAlphabet = string.Empty;
            Keyword = string.Empty;
            Shift = -1;                     
            Keyword2 = string.Empty;
            Shift2 = -1;
        }

        public void PostExecution()
        {

        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public ISettings Settings => _settings;

        public System.Windows.Controls.UserControl Presentation => null;

        public void Execute()
        {
            if (string.IsNullOrEmpty(SourceAlphabet))
            {
                SourceAlphabet = LATIN_ALPHABET;
            }

            try
            {
                //apply keying schemes based on ACA definitions:
                switch (_settings.ACAKeyingScheme)
                {
                    case ACAKeyingScheme.K1:
                        GenerateK1Alphabets();
                        break;
                    case ACAKeyingScheme.K2:
                        GenerateK2Alphabets();
                        break;
                    case ACAKeyingScheme.K3:
                        GenerateK3Alphabets();
                        break;
                    case ACAKeyingScheme.K4:
                        GenerateK4Alphabets();
                        break;
                    default:
                        throw new NotImplementedException(string.Format("KeyingScheme {0} not implemented", _settings.ACAKeyingScheme));
                }
            }
            catch(Exception ex)
            {
                GuiLogMessage(string.Format("Exception occured while applying key schemes: {0}", ex.Message), NotificationLevel.Error);
                return;
            }

            try
            {
                if(_settings.AlphabetsOutputFormat == AlphabetsOutputFormat.Normalized)
                {
                    NormalizeAlphabets();
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Exception occured while normalizing alphabets: {0}", ex.Message), NotificationLevel.Error);
                return;
            }

            //no error occured; so we can forward the resulting alphabets
            OnPropertyChanged("PlaintextAlphabet");
            OnPropertyChanged("CiphertextAlphabet");
        }

        /// <summary>
        /// Normalizes plaintext and ciphertext alphabets, meaning that the plaintext alphabet is sorted 
        /// alphabetically and the ciphertext alphabet is sorted accordingly
        /// </summary>
        private void NormalizeAlphabets()
        {
            string sortedPlaintextAlphabet = Sort(PlaintextAlphabet, 0);
            StringBuilder sortedCiphertextAlphabetStringBuilder = new StringBuilder();

            foreach(char letter in sortedPlaintextAlphabet)
            {
                int index = PlaintextAlphabet.IndexOf(letter);
                sortedCiphertextAlphabetStringBuilder.Append(CiphertextAlphabet[index]);
            }

            PlaintextAlphabet = sortedPlaintextAlphabet;
            CiphertextAlphabet = sortedCiphertextAlphabetStringBuilder.ToString();
        }

        /// <summary>
        /// Generates a new alphabet based on the source alphabet, the Shift, the keyword, and the order
        /// </summary>
        /// <param name="sourceAlphabet"></param>
        /// <param name="shift"></param>
        /// <param name="keyword"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public string GenerateAlphabet(string sourceAlphabet, int shift, string keyword, AlphabetOrder order)
        {
            StringBuilder builder = new StringBuilder();

            string distinctSourceAlphabet = Distinct(sourceAlphabet);
            if ((int)order < 2)
            {
                distinctSourceAlphabet = Sort(distinctSourceAlphabet, (int)order);
            }

            string distinctKeyword = string.Empty;
            if (!string.IsNullOrEmpty(keyword))
            {
                distinctKeyword = Distinct(keyword);
                distinctSourceAlphabet = Regex.Replace(distinctSourceAlphabet, "[" + distinctKeyword + "]", "");
            }            

            builder.Append(distinctKeyword);
            builder.Append(distinctSourceAlphabet);

            string generatedAlphabet = RightCircularShift(builder.ToString(), shift);

            return generatedAlphabet;
        }

        /// <summary>
        /// Circular right shifts a given string by shift positions
        /// </summary>
        /// <param name="str"></param>
        /// <param name="shift"></param>
        /// <returns></returns>
        private string RightCircularShift(string str, int shift)
        {
            shift %= str.Length;
            return str.Substring(str.Length - shift) + str.Substring(0, str.Length - shift);
        }

        /// <summary>
        /// The plaintext alphabet is keyed, the ciphertext alphabet is not
        /// </summary>
        private void GenerateK1Alphabets()
        {
            string plaintextAlphabetKeyword = !string.IsNullOrEmpty(Keyword) ? Keyword : _settings.Keyword;
            int shift = Shift != -1 ? Shift : _settings.Shift;
            int shift2 = Shift2 != -1 ? Shift2 : _settings.Shift2;
            PlaintextAlphabet = GenerateAlphabet(SourceAlphabet, shift, plaintextAlphabetKeyword, _settings.PlaintextAlphabetOrder);
            CiphertextAlphabet = GenerateAlphabet(SourceAlphabet, shift2, string.Empty, _settings.CiphertextAlphabetOrder);
        }

        /// <summary>
        /// The plaintext alphabet is not keyed, the ciphertext alphabet is
        /// </summary>
        private void GenerateK2Alphabets()
        {
            string ciphertextAlphabetKeyword = !string.IsNullOrEmpty(Keyword2) ? Keyword2 : _settings.Keyword2;
            int shift = Shift != -1 ? Shift : _settings.Shift;
            int shift2 = Shift2 != -1 ? Shift2 : _settings.Shift2;
            PlaintextAlphabet = GenerateAlphabet(SourceAlphabet, shift, string.Empty, _settings.PlaintextAlphabetOrder);
            CiphertextAlphabet = GenerateAlphabet(SourceAlphabet, shift2, ciphertextAlphabetKeyword, _settings.CiphertextAlphabetOrder);
        }

        /// <summary>
        /// Both alphabets are keyed using the same keyword
        /// </summary>
        private void GenerateK3Alphabets()
        {
            string plaintextAlphabetKeyword = !string.IsNullOrEmpty(Keyword) ? Keyword : _settings.Keyword;
            string ciphertextAlphabetKeyword = plaintextAlphabetKeyword;
            int shift = Shift != -1 ? Shift : _settings.Shift;
            int shift2 = Shift2 != -1 ? Shift2 : _settings.Shift2;
            PlaintextAlphabet = GenerateAlphabet(SourceAlphabet, shift, plaintextAlphabetKeyword, _settings.PlaintextAlphabetOrder);
            CiphertextAlphabet = GenerateAlphabet(SourceAlphabet, shift2, ciphertextAlphabetKeyword, _settings.CiphertextAlphabetOrder);
        }

        /// <summary>
        /// Both alphabets are keyed using different keywords
        /// </summary>
        private void GenerateK4Alphabets()
        {
            string plaintextAlphabetKeyword = !string.IsNullOrEmpty(Keyword) ? Keyword : _settings.Keyword;
            string ciphertextAlphabetKeyword = !string.IsNullOrEmpty(Keyword2) ? Keyword2 : _settings.Keyword2;
            int shift = Shift != -1 ? Shift : _settings.Shift;
            int shift2 = Shift2 != -1 ? Shift2 : _settings.Shift2;
            PlaintextAlphabet = GenerateAlphabet(SourceAlphabet, shift, plaintextAlphabetKeyword, _settings.PlaintextAlphabetOrder);
            CiphertextAlphabet = GenerateAlphabet(SourceAlphabet, shift2, ciphertextAlphabetKeyword, _settings.CiphertextAlphabetOrder);
        }

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        public void Stop()
        {

        }

        public void Initialize()
        {

        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void Dispose()
        {

        }

        /// <summary>
        /// Sorts the given string
        /// </summary>
        /// <param name="str"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        private string Sort(string str, int direction)
        {
            char[] sortarr = str.ToCharArray();
            Array.Sort(sortarr);
            if (direction == 1)
            {
                Array.Reverse(sortarr);
            }
            return new string(sortarr);
        }

        /// <summary>
        /// "Distincts" the given string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private string Distinct(string str)
        {
            StringBuilder builder = new StringBuilder();
            HashSet<char> chars = new HashSet<char>();

            foreach (char c in str)
            {
                if (!chars.Contains(c))
                {
                    chars.Add(c);
                    builder.Append(c);
                }
            }
            return builder.ToString();
        }

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            OnGuiLogNotificationOccured?.Invoke(this, new GuiLogEventArgs(message, this, logLevel));
        }

    }
}
