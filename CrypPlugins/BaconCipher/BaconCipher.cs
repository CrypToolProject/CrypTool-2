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

using CrypTool.BaconCipher.Properties;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using static System.String;

namespace CrypTool.BaconCipher
{
    [Author("Niklas Weimann", "niklas.weimann@student.uni-siegen.de", "CrypTool 2 Team", "https://www.cryptool.org")]
    [PluginInfo("CrypTool.BaconCipher.Properties.Resources", "BaconCipher", "BaconTooltip",
        "BaconCipher/userdoc.xml",
        new[] { "BaconCipher/Images/icon.png" })]
    [ComponentCategory(ComponentCategory.Steganography)]
    public class BaconCipher : ICrypComponent
    {
        #region Private Variables

        private readonly BaconCipherSettings _settings = new BaconCipherSettings();
        private string _outputText;
        private Dictionary<char, string> Char2String { get; set; }
        private Dictionary<string, List<char>> String2Char { get; set; }
        private static readonly Random Random = new Random();
        private string _firstSet = Resources.FirstSet;
        private string _secondSet = Resources.SecondSet;
        private bool _running;

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "InputTextCaption", "InputTextTooltip", true)]
        public string InputText { get; set; }

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

        [PropertyInfo(Direction.InputData, "InputCipherCharsCaption", "InputCipherCharsTooltip")]
        public string CipherChars { get; set; }

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

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings => _settings;

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation => null;

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
            if (_settings.OutputMode == BaconCipherSettings.OutputTypes.ExternalInput)
            {
                (string first, string second) = SplitString(CipherChars);
                if (first == null && second == null)
                {
                    return;
                }

                _firstSet = first;
                _secondSet = second;
            }

            if (_settings.Alphabet.Length > Math.Pow(2, _settings.CodeLength) && !_settings.DynamicCodeLength)
            {
                GuiLogMessage(Resources.AlphabetTooLong, NotificationLevel.Error);
                return;
            }

            BuildMapping();

            if (!_running)
            {
                return;
            }

            ProgressChanged(0.5, 1);
            OutputText = _settings.Cipher == BaconCipherSettings.BaconianMode.Encrypt
                ? Encode(InputText)
                : Decode(InputText);
            ProgressChanged(1, 1);
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

        private static string GetEscapedStringByTraversingCharArray(char[] s)
        {
            if (s == null)
            {
                return Empty;
            }

            StringBuilder sb = new StringBuilder();
            for (int index = 0; index < s.Length; index++)
            {
                char ch = s[index];
                switch (ch)
                {
                    case '\t':
                        sb.Append(@"\t");
                        break;
                    case '\n':
                        sb.Append(@"\n");
                        break;
                    case '\r':
                        sb.Append(@"\r");
                        break;
                    case ' ':
                        sb.Append(@"' '");
                        break;
                    default:
                        sb.Append(ch);
                        break;
                }

                if (index < s.Length - 1)
                {
                    sb.Append(", ");
                }
            }

            return sb.ToString();
        }

        private (string First, string Second) SplitString(string src)
        {
            char[] splitCharArray = new[] { '\n', ';', ' ' };

            if (IsNullOrEmpty(src) || src.Length < 3)
            {
                GuiLogMessage(
                    Format(Resources.ErrorSplitCipherAlphabet,
                        GetEscapedStringByTraversingCharArray(splitCharArray)),
                    NotificationLevel.Error);
                return (null, null);
            }

            string[] rowsColumns = src.Split(splitCharArray);
            if (rowsColumns.Length == 2)
            {
                return (rowsColumns[0], rowsColumns[1]);
            }

            return (Resources.FirstSet, Resources.SecondSet);
        }


        private string Encode(string src)
        {
            StringBuilder res = new StringBuilder();
            foreach (char item in src)
            {
                if (_running == false)
                {
                    return string.Empty;
                }
                if (char.IsWhiteSpace(item))
                {
                    res.Append(" ");
                    continue;
                }

                if (!Char2String.ContainsKey(item))
                {
                    GuiLogMessage(Format(Resources.IgnoringChar, item),
                        NotificationLevel.Warning);
                    continue;
                }

                string x = Char2String[item];
                StringBuilder temp = new StringBuilder();
                foreach (char codeChar in x)
                {
                    switch (_settings.OutputMode)
                    {
                        case BaconCipherSettings.OutputTypes.RandomChar:
                            temp.Append(ConvertCodeToChar(codeChar));
                            break;
                        case BaconCipherSettings.OutputTypes.Binary:
                            temp.Append(codeChar);
                            break;
                        case BaconCipherSettings.OutputTypes.ExternalInput:
                            temp.Append(ConvertCodeToChar(codeChar));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(_settings.OutputMode));
                    }
                }


                res.Append(temp);
            }

            return res.ToString();
        }

        private string Decode(string src)
        {
            StringBuilder tempCode = new StringBuilder();
            StringBuilder res = new StringBuilder();
            foreach (char t in src)
            {
                if (_running == false)
                {
                    return Empty;
                }

                if (char.IsWhiteSpace(t))
                {
                    res.Append(" ");
                    continue;
                }

                switch (_settings.OutputMode)
                {
                    case BaconCipherSettings.OutputTypes.Binary:
                        tempCode.Append(t);
                        break;
                    case BaconCipherSettings.OutputTypes.RandomChar:
                        tempCode.Append(ConvertCharToCode(t));
                        break;
                    case BaconCipherSettings.OutputTypes.ExternalInput:
                        tempCode.Append(ConvertCharToCode(t));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(_settings.OutputMode));
                }

                if (tempCode.Length != GetCodeSize())
                {
                    continue;
                }

                if (!String2Char.ContainsKey(tempCode.ToString()))
                {
                    GuiLogMessage(Format(Resources.CodeNotMappable, tempCode),
                        NotificationLevel.Error);
                    return res.ToString();
                }

                res.Append(String2Char[tempCode.ToString()].Count > 1
                    ? $"({Join(", ", String2Char[tempCode.ToString()])})"
                    : String2Char[tempCode.ToString()].First().ToString());
                tempCode.Clear();
            }

            return res.ToString();
        }

        private char ConvertCharToCode(char src)
        {
            return _firstSet.Contains(src) ? '0' : _secondSet.Contains(src) ? '1' : char.MinValue;
        }

        private char ConvertCodeToChar(char src)
        {
            switch (src)
            {
                case '0':
                    return _firstSet[Random.Next(_firstSet.Length)];
                case '1':
                    return _secondSet[Random.Next(_secondSet.Length)];
                default:
                    GuiLogMessage(Format(Resources.CharCouldNotBeDecoded, src), NotificationLevel.Error);
                    return char.MinValue;
            }
        }

        private void BuildMapping()
        {
            Char2String = new Dictionary<char, string>();
            String2Char = new Dictionary<string, List<char>>();
            string alphabet = _settings.Alphabet;
            Stack<bool> isInParentheses = new Stack<bool>();
            int index = 0;
            foreach (char currentChar in alphabet)
            {
                if (_running == false)
                {
                    return;
                }

                if (Char2String.ContainsKey(currentChar))
                {
                    GuiLogMessage(Format(Resources.CharAlreadyMapped, currentChar), NotificationLevel.Error);
                }

                switch (currentChar)
                {
                    case '(':
                        isInParentheses.Push(true);
                        continue;
                    case ')':
                        isInParentheses.Pop();
                        index++;
                        continue;
                }

                if (isInParentheses.Any())
                {
                    string byteCode = Convert.ToString(index, 2).PadLeft(GetCodeSize(), '0');
                    Char2String.Add(currentChar, byteCode);
                    if (String2Char.ContainsKey(byteCode))
                    {
                        String2Char[byteCode].Add(currentChar);
                    }
                    else
                    {
                        String2Char.Add(byteCode, new List<char> { currentChar });
                    }
                }
                else
                {
                    string byteCode = Convert.ToString(index, 2).PadLeft(GetCodeSize(), '0');
                    Char2String.Add(currentChar, byteCode);
                    if (String2Char.ContainsKey(byteCode))
                    {
                        String2Char[byteCode].Add(currentChar);
                    }
                    else
                    {
                        String2Char.Add(byteCode, new List<char> { currentChar });
                    }

                    index++;
                }
            }
        }

        private int GetCodeSize()
        {
            return !_settings.DynamicCodeLength
? _settings.CodeLength
: (int)Math.Ceiling(Math.Log(_settings.Alphabet.Length, 2));
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