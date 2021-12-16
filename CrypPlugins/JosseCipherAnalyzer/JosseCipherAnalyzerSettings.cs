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

using CrypTool.JosseCipherAnalyzer.Enum;
using CrypTool.JosseCipherAnalyzer.Properties;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using CrypTool.PluginBase.Utils;
using System.ComponentModel;

namespace CrypTool.JosseCipherAnalyzer
{
    public class JosseCipherAnalyzerSettings : ISettings
    {
        #region Private Variables

        private AnalyzerMode _analyzerMode = AnalyzerMode.Hillclimbing;
        private int _restarts = 50;
        private CostFunctionTypes _costFunctionTypes = CostFunctionTypes.Tetragrams;
        private string _languageCode = "en";
        private int _keyLengthFrom = 1;
        private int _keyLengthTo = 1;
        private bool _useSpaces;
        private string _alphabet = Resources.BaseAlphabet;

        #endregion

        #region TaskPane Settings

        [TaskPane("AnalyzerMode", "AnalyzerModeTooltip", "AttackType", 10, false,
            ControlType.ComboBox, new string[] { "ModeAnalyzerModeHillclimbing", "ModeAnalyzerModeSimulatedAnnealing" })]
        public AnalyzerMode AnalyzerMode
        {
            get => _analyzerMode;
            set
            {
                if (_analyzerMode == value)
                {
                    return;
                }

                _analyzerMode = value;
                OnPropertyChanged(nameof(AnalyzerMode));
            }
        }

        [TaskPane("Restarts", "RestartsTooltip", "AttackType", 20, false,
            ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 10000)]
        public int Restarts
        {
            get => _restarts;
            set
            {
                if (_restarts == value)
                {
                    return;
                }

                _restarts = value;
                OnPropertyChanged(nameof(Restarts));
            }
        }

        [TaskPane("CostFunctionType", "CostFunctionTypeTooltip", "AttackType", 30, false,
            ControlType.ComboBox,
            new string[]
            {
                "CFUnigrams",
                "CFBigrams",
                "CFTrigrams",
                "CFTetragrams",
                "CFPentragrams",
                "CFHexagrams",
                "CFIoC",
                "CFEntropy"
            })]
        public CostFunctionTypes CostFunctionTypes
        {
            get => _costFunctionTypes;
            set
            {
                if (_costFunctionTypes == value)
                {
                    return;
                }

                _costFunctionTypes = value;
                OnPropertyChanged(nameof(CostFunctionTypes));
            }
        }

        [TaskPane("UseSpacesCaption", "UseSpacesTooltip", "AttackType", 40, false, ControlType.CheckBox)]
        public bool UseSpaces
        {
            get => _useSpaces;
            set
            {
                if (value == _useSpaces)
                {
                    return;
                }

                _useSpaces = value;
                OnPropertyChanged(nameof(UseSpaces));
            }
        }

        [TaskPane("LanguageCaption", "LanguageTooltip", null, 100, false, ControlType.LanguageSelector)]
        public int Language
        {
            get => LanguageStatistics.LanguageId(_languageCode);
            set
            {
                if (value == LanguageStatistics.LanguageId(_languageCode))
                {
                    return;
                }
                _languageCode = LanguageStatistics.LanguageCode(value);
                OnPropertyChanged(nameof(Language));
            }
        }

        [TaskPane("KeylengthFromCaption", "KeylengthFromTooltip", "Key", 200, false, ControlType.NumericUpDown,
            ValidationType.RangeInteger, 1, 25)]
        public int KeyLengthFrom
        {
            get => _keyLengthFrom;
            set
            {
                if (value == _keyLengthFrom)
                {
                    return;
                }

                _keyLengthFrom = value;
                OnPropertyChanged(nameof(KeyLengthFrom));
            }
        }

        [TaskPane("KeylengthToCaption", "KeylengthToTooltip", "Key", 200, false, ControlType.NumericUpDown,
            ValidationType.RangeInteger, 1, 25)]
        public int KeyLengthTo
        {
            get => _keyLengthTo;
            set
            {
                if (value == _keyLengthTo)
                {
                    return;
                }

                _keyLengthTo = value;
                OnPropertyChanged(nameof(KeyLengthTo));
            }
        }

        [PropertySaveOrder(20)]
        [TaskPane("Alphabet", "AlphabetTooltip", "Key", 210, false, ControlType.TextBox)]
        public string Alphabet
        {
            get => _alphabet ?? string.Empty;
            set
            {
                if (_alphabet == value)
                {
                    return;
                }

                _alphabet = value;
                OnPropertyChanged(nameof(Alphabet));
            }
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion

        public void Initialize()
        {
        }
    }
}