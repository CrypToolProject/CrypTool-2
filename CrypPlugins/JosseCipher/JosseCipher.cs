/*
   Copyright CrypTool 2 Team <ct2contact@cryptool.org>

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
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;
using CrypTool.JosseCipher.Properties;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;

namespace CrypTool.JosseCipher
{
    [Author("Niklas Weimann", "niklas.weimann@student.uni-siegen.de", "CrypTool 2 Team", "https://niklas-weimann.de")]
    [PluginInfo("CrypTool.JosseCipher.Properties.Resources", "PluginCaption", "PluginTooltip", "JosseCipher/userdoc.xml",
        new[] {"JosseCipher/Images/icon.png"})]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class JosseCipher : ICrypComponent
    {
        #region Private Variables

        private readonly JosseCipherSettings _settings = new JosseCipherSettings();
        private Dictionary<char, int> _char2Int = new Dictionary<char, int>();
        private Dictionary<int, char> _int2Char = new Dictionary<int, char>();
        private readonly JosseCipherPresentation _josseCipherPresentation;
        private double _process;
        private string _outputText;
        private string _calculationStepOutput;
        private bool _running;

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "InputTextCaption", "InputTextTooltip", true)]
        public string InputText { get; set; }

        [PropertyInfo(Direction.InputData, "Keyword", "KeywordToolTip")]
        public string Keyword
        {
            get => _settings.Keyword;
            set
            {
                if (value == null || value == _settings.Keyword)
                {
                    return;
                }

                _settings.Keyword = value;
                OnPropertyChanged(nameof(Keyword));
            }
        }

        [PropertyInfo(Direction.InputData, "InputAlphabetCaption", "InputAlphabetTooltip")]
        public string Alphabet
        {
            get => _settings.Alphabet;
            set
            {
                if (value == null || value == _settings.Alphabet)
                {
                    return;
                }

                _settings.Alphabet = value;
                OnPropertyChanged(nameof(Alphabet));
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputTextCaption", "OutputTextTooltip")]
        public string OutputText
        {
            get => _outputText;
            set
            {
                _outputText = value;
                OnPropertyChanged(nameof(OutputText));
            }
        }

        [PropertyInfo(Direction.OutputData, "CalculationStepOutputCaption", "CalculationStepOutputTooltip")]
        public string CalculationStepOutput
        {
            get => _calculationStepOutput;
            set
            {
                _calculationStepOutput = value;
                OnPropertyChanged(nameof(CalculationStepOutput));
            }
        }

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings => _settings;

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation => _josseCipherPresentation;

        public JosseCipher()
        {
            _josseCipherPresentation = new JosseCipherPresentation();
        }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            _running = true;
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);
            if (string.IsNullOrEmpty(Keyword) && Keyword.Length < 1)
            {
                GuiLogMessage(Resources.KeywordTooShort, NotificationLevel.Warning);
            }

            BuildDictionaries(Keyword);
            ProgressChanged(0.5, 1);
            var result = _settings.Cipher == JosseCipherSettings.JosseCipherMode.Encrypt
                ? Encipher(InputText)
                : Decipher(InputText);

            if (_running)
            {
                OutputText = result;
            }
            ProgressChanged(1, 1);
        }

        private double UpdateProcess(int cipherLength)
        {
            var oldProcess = _process;
            _process += GetStepSize(cipherLength);
            return oldProcess;
        }

        private double GetStepSize(int count) => (1 - _process) / count;

        private void BuildDictionaries(string key)
        {
            _char2Int = new Dictionary<char, int>();
            _int2Char = new Dictionary<int, char>();

            var alphabet = Alphabet;
            var keyChars = key.ToCharArray().Where(x => !char.IsWhiteSpace(x)).Distinct().ToList();
            int keyLength;
            if (keyChars.Count == 0 && alphabet.Length >= 5)
            {
                keyLength = 5;
            }
            else if(keyChars.Count == 0 && alphabet.Length < 5)
            {
                keyLength = alphabet.Length;
            }
            else
            {
                keyLength = keyChars.Count;
                if (keyLength != key.Length)
                {
                    GuiLogMessage(Resources.DuplicateCharsRemoved, NotificationLevel.Warning);
                }
            }



            // Remove chars in key from alphabet and add key at the beginning
            alphabet = string.Concat(keyChars) + string.Concat(alphabet.Where(c => !keyChars.Contains(c)));

            // Build internal representation
            var count = 1;
            var charIndexList = new List<(char, int)>();
            var replacementTable = new DataTable();
            var characterMappingList = new List<(int, char)>();
            for (var i = 0; i < keyLength && _running; i++)
            {
                replacementTable.Columns.Add(i.ToString());
                for (var index = i; index < alphabet.Length; index += keyLength)
                {
                    characterMappingList.Add((count, alphabet[index]));
                    _char2Int.Add(alphabet[index], count);
                    _int2Char.Add(count, alphabet[index]);
                    charIndexList.Add((alphabet[index], count));
                    count++;
                }
            }

            // Build replacementTable
            var tableIndex = 0;
            for (var i = 0; i < charIndexList.Count; i += keyLength)
            {
                var row = replacementTable.NewRow();
                for (var j = 0; j < keyLength && alphabet.Length > tableIndex; j++)
                {
                    row[j] = $"{alphabet[tableIndex]}";
                    tableIndex++;
                }

                replacementTable.Rows.Add(row);
            }

            ShowReplacementTable(replacementTable);
            var mappingTable = BuildCharacterMappingList(characterMappingList);
            ShowCharacterMapping(mappingTable);
        }

        private static DataTable BuildCharacterMappingList(IEnumerable<(int, char)> mappingList)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add(Resources.Char);
            dataTable.Columns.Add(Resources.Index);
            var sortedMappingList = mappingList.OrderBy(x => x.Item2).ToList();
            for (var i = 0; i < sortedMappingList.Count; i++)
            {
                var row = dataTable.NewRow();
                row[0] = sortedMappingList[i].Item2;
                row[1] = sortedMappingList[i].Item1;
                dataTable.Rows.Add(row);
            }

            return dataTable;
        }

        private void ShowCharacterMapping(DataTable dataTable)
        {
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal,
                (SendOrPostCallback) delegate { _josseCipherPresentation.BuildCharacterMappingTable(dataTable); }, null);
        }

        private void ShowReplacementTable(DataTable tableContent)
        {
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal,
                (SendOrPostCallback) delegate { _josseCipherPresentation.BuildReplacementTable(tableContent); }, null);
        }

        public string Encipher(string unparsedPlainText)
        {
            var res = string.Empty;
            var count = 0;
            var plainText = unparsedPlainText.Where(x => _char2Int.ContainsKey(x)).ToArray();
            if (plainText.Length != unparsedPlainText.Length)
            {
                GuiLogMessage(
                    string.Format(Resources.UnknownChar,
                        new string(unparsedPlainText.Where(x => !_char2Int.ContainsKey(x)).Distinct().ToArray())),
                    NotificationLevel.Warning);
            }

            var steps = new string[plainText.Length + 1, 5];
            steps[0, 0] = Resources.Plaintext;
            steps[0, 1] = Resources.NumericalRepresentation;
            steps[0, 2] = Resources.Calculation;
            steps[0, 3] = Resources.Result;
            steps[0, 4] = Resources.Ciphertext;
            for (var i = 0; i < plainText.Length && _running; i++)
            {
                if (!_char2Int.ContainsKey(plainText[i]))
                {
                    continue;
                }
                var charToInt = _char2Int[plainText[i]];
                steps[i + 1, 0] = char.ToString(plainText[i]);
                steps[i + 1, 1] = charToInt.ToString();
                if (i == 0)
                {
                    steps[i + 1, 2] = $"{_char2Int.Count} - {charToInt}";
                    steps[i + 1, 3] = (_char2Int.Count - charToInt).ToString();
                    steps[i + 1, 4] = _int2Char[_char2Int.Count - charToInt].ToString();

                    res += _int2Char[_char2Int.Count - charToInt];
                    count += _char2Int[plainText[i]];
                    continue;
                }

                steps[i + 1, 2] = $"{count} + {charToInt} mod {_char2Int.Count}";
                count += charToInt;
                if (count % _char2Int.Count == 0)
                {
                    res += _int2Char[_char2Int.Count];
                    steps[i + 1, 2] = $"{count} mod {_char2Int.Count}";
                    steps[i + 1, 3] = (count % _char2Int.Count).ToString();
                    steps[i + 1, 4] = _int2Char[_char2Int.Count].ToString();
                }
                else
                {
                    res += _int2Char[count % _char2Int.Count];
                    steps[i + 1, 2] = $"{count} mod {_char2Int.Count}";
                    steps[i + 1, 3] = (count % _char2Int.Count).ToString();
                    steps[i + 1, 4] = _int2Char[count % _char2Int.Count].ToString();
                }

                
                ProgressChanged(UpdateProcess(plainText.Length), 1);
            }

            BuildOutputString(steps, JosseCipherSettings.JosseCipherMode.Encrypt, new string(plainText), res);
            return res;
        }

        public string Decipher(string unparsedCypherText)
        {
            var cypherText = unparsedCypherText.Where(x => _char2Int.ContainsKey(x)).ToArray();
            if (cypherText.Length != unparsedCypherText.Length)
            {
                GuiLogMessage(
                    string.Format(Resources.UnknownChar,
                        new string(unparsedCypherText.Where(x => !_char2Int.ContainsKey(x)).ToArray())),
                    NotificationLevel.Warning);
            }

            var res = string.Empty;
            var steps = new string[cypherText.Length + 1, 5];
            steps[0, 0] = Resources.Ciphertext;
            steps[0, 1] = Resources.NumericalRepresentation;
            steps[0, 2] = Resources.Calculation;
            steps[0, 3] = Resources.Result;
            steps[0, 4] = Resources.Plaintext;

            for (var i = 0; i < cypherText.Length && _running; i++)
            {
                var charToInt = _char2Int[cypherText[i]];
                steps[i + 1, 0] = char.ToString(cypherText[i]);
                steps[i + 1, 1] = charToInt.ToString();
                switch (i)
                {
                    case 0:
                        var plainText = _int2Char[_char2Int.Count - charToInt];
                        res += plainText;
                        steps[i + 1, 2] = $"{_char2Int.Count} - {charToInt}";
                        steps[i + 1, 3] = (_char2Int.Count - charToInt).ToString();
                        steps[i + 1, 4] = char.ToString(plainText);
                        continue;
                    case 1:
                        var index = (charToInt + _char2Int[cypherText[i - 1]]) % _char2Int.Count;
                        var plaintext = _int2Char[index];
                        res += plaintext;
                        steps[i + 1, 2] = $"{charToInt} + {_char2Int[cypherText[i - 1]]} mod {_char2Int.Count}";
                        steps[i + 1, 3] = index.ToString();
                        steps[i + 1, 4] = char.ToString(plaintext);
                        break;
                    default:
                        var charNum = Mod(charToInt - _char2Int[cypherText[i - 1]], _char2Int.Count);
                        if (charNum == 0)
                        {
                            charNum = _char2Int.Count;
                        }

                        res += _int2Char[charNum];
                        steps[i + 1, 2] = $"{charToInt} - {_char2Int[cypherText[i - 1]]} mod {_char2Int.Count}";
                        steps[i + 1, 3] = charNum.ToString();
                        steps[i + 1, 4] = _int2Char[charNum].ToString();
                        break;
                }

                ProgressChanged(UpdateProcess(cypherText.Length), 1);
            }

            BuildOutputString(steps, JosseCipherSettings.JosseCipherMode.Decrypt, new string(cypherText), res);

            return res;
        }

        private void BuildOutputString(string[,] steps, JosseCipherSettings.JosseCipherMode josseCipherMode,
            string fromText,
            string toText)
        {
            var stepOutput = new StringBuilder();
            if (josseCipherMode == JosseCipherSettings.JosseCipherMode.Decrypt)
            {
                stepOutput.Append(string.Format(Resources.CalculationStepOutputExplanationDeciphering, fromText,
                    toText));
            }
            else
            {
                stepOutput.Append(string.Format(Resources.CalculationStepOutputExplanationEnciphering, fromText,
                    toText));
            }

            stepOutput.Append(Environment.NewLine);
            stepOutput.Append(ArrayPrinter.PrintToConsole(steps));
            if (string.IsNullOrEmpty(fromText) && string.IsNullOrEmpty(toText))
            {
                return;
            }

            CalculationStepOutput = stepOutput.ToString();
        }

        private static int Mod(int x, int m)
        {
            return (x % m + m) % m;
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
            _running = false;
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