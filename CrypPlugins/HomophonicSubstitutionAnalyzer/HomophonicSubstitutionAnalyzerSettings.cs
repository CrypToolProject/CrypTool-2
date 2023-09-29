/*
   Copyright 2023 Nils Kopal <Nils.Kopal<at>CrypTool.org

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
using System.ComponentModel;
using System.Windows;

namespace CrypTool.Plugins.HomophonicSubstitutionAnalyzer
{
    /// <summary>
    /// SingleLetter <=> symbol is a single UTF-8 letter
    /// NumberGroups <=> symbol is a group of numbers, e.g. 01 121 1112, defined by length of groups
    /// CommaSepartated <=> symbol is everything between two "commas", e.g. BLA,FU,BAR
    /// </summary>
    public enum CiphertextFormat
    {
        SingleLetters,
        NumberGroups,
        CommaSeparated
    }

    /// <summary>
    /// Separator for "CommaSeparated" CiphertextFormat
    /// </summary>
    public enum Separator
    {
        Comma,
        FullStop,
        Semicolon,
        Space
    }

    /// <summary>
    /// Mode of cryptanalysis
    /// </summary>
    public enum AnalysisMode
    {
        SemiAutomatic,
        FullAutomatic
    }

    public enum KeyLetterDistributionType
    {
        Uniform,
        LanguageBasted
    }

    /// <summary>
    /// Settings class for Homophonic Substitution Analyzer
    /// </summary>
    public class HomophonicSubstitutionAnalyzerSettings : ISettings
    {
        #region Private Variables

        private int _language;
        private bool _useSpaces;
        private CiphertextFormat _ciphertextFormat = CiphertextFormat.SingleLetters;
        private Separator _separator = Separator.Space;
        private KeyLetterDistributionType _keyLetterDistributionType = KeyLetterDistributionType.LanguageBasted;
        private int _homophonicity = 2;
        private int _wordCountToFind = 5;
        private int _minWordLength = 8;
        private int _maxWordLength = 10;
        private int _nomenclatureElementsThreshold = 1;
        private AnalysisMode _analysisMode;
        private int _steps = 10_000_000;
        private int _restarts = 1000;
        private int _startTemperature = 15_000;
        private bool _useNulls = false;
        private bool _keepLinebreaks = false;
        private string _letterLimits = string.Empty;

        #endregion

        /// <summary>
        /// This event is needed in order to render settings elements visible/invisible
        /// </summary>
        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;        

        #region TaskPane Settings      

        [TaskPane("LanguageCaption", "LanguageTooltip", "LanguageSettingsGroup", 0, false, ControlType.LanguageSelector)]
        public int Language
        {
            get => _language;
            set
            {
                if (value != _language)
                {
                    _language = value;
                    OnPropertyChanged("Language");
                }
            }
        }

        [TaskPane("UseSpacesCaption", "UseSpacesTooltip", "LanguageSettingsGroup", 1, false, ControlType.CheckBox)]
        public bool UseSpaces
        {
            get => _useSpaces;
            set
            {
                if (value != _useSpaces)
                {
                    _useSpaces = value;
                    OnPropertyChanged("UseSpaces");
                }
            }
        }

        [TaskPane("UseNullsCaption", "UseNullsTooltip", "LanguageSettingsGroup", 2, false, ControlType.CheckBox)]
        public bool UseNulls
        {
            get => _useNulls;
            set
            {
                if (value != _useNulls)
                {
                    _useNulls = value;
                    OnPropertyChanged("UseNulls");
                }
            }
        }

        [TaskPane("KeepLinebreaksCaption", "KeepLinebreaksTooltip", "LanguageSettingsGroup", 3, false, ControlType.CheckBox)]
        public bool KeepLinebreaks
        {
            get => _keepLinebreaks;
            set => _keepLinebreaks = value;
        }

        [TaskPane("CiphertextFormatCaption", "CiphertextFormatTooltip", "TextFormatGroup", 4, false, ControlType.ComboBox, new[] { "SingleLetters", "NumberGroups", "SymbolSeparated" })]
        public CiphertextFormat CiphertextFormat
        {
            get => _ciphertextFormat;
            set => _ciphertextFormat = value;
        }

        [TaskPane("SeparatorCaption", "SeparatorTooltip", "TextFormatGroup", 5, false, ControlType.ComboBox, new[] { "Comma", "FullStop", "Semicolon", "Space" })]
        public Separator Separator
        {
            get => _separator;
            set => _separator = value;
        }

        [TaskPane("KeyLetterDistributionTypeCaption", "KeyLetterDistributionTypeTooltip", "KeyLetterDistributionGroup", 6, false, ControlType.ComboBox, new[] { "Uniform", "LanguageBased" })]
        public KeyLetterDistributionType KeyLetterDistributionType
        {
            get => _keyLetterDistributionType;
            set
            {
                _keyLetterDistributionType = value;
                UpdateSettingsVisibilities();
                OnPropertyChanged(nameof(KeyLetterDistributionType));
            }
        }

        [TaskPane("HomophonicityCaption", "HomophonicityTooltip", "KeyLetterDistributionGroup", 7, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 100 )]
        public int Homophonicity
        {
            get => _homophonicity;
            set
            {
                _homophonicity = value;
                OnPropertyChanged(nameof(Homophonicity));
            }
        }

        [TaskPane("WordCountToFindCaption", "WordCountToFindTooltip", "WordLockerGroup", 8, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 100)]
        public int WordCountToFind
        {
            get => _wordCountToFind;
            set => _wordCountToFind = value;
        }

        [TaskPane("MinWordLengthCaption", "MinWordLengthTooltip", "WordLockerGroup", 9, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 100)]
        public int MinWordLength
        {
            get => _minWordLength;
            set => _minWordLength = value;
        }

        [TaskPane("MaxWordLengthCaption", "MaxWordLengthTooltip", "WordLockerGroup", 10, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 100)]
        public int MaxWordLength
        {
            get => _maxWordLength;
            set => _maxWordLength = value;
        }

        [TaskPane("NomenclatureElementsThresholdCaption", "NomenclatureElementsThresholdTooltip", "NomenclatureElementsThresholdGroup", 11, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 100)]
        public int NomenclatureElementsThreshold
        {
            get => _nomenclatureElementsThreshold;
            set => _nomenclatureElementsThreshold = value;
        }

        [TaskPane("AnalysisModeCaption", "AnalaysisModeTooltip", "AlgorithmSettingsGroup", 12, false, ControlType.ComboBox, new string[] { "SemiAutomatic", "FullAutomatic" })]
        public AnalysisMode AnalysisMode
        {
            get => _analysisMode;
            set => _analysisMode = value;
        }

        [TaskPane("StepsCaption", "StepsTooltip", "AlgorithmSettingsGroup", 13, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
        public int Steps
        {
            get => _steps;
            set => _steps = value;
        }

        [TaskPane("RestartsCaption", "RestartsTooltip", "AlgorithmSettingsGroup", 14, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, int.MaxValue)]
        public int Restarts
        {
            get => _restarts;
            set => _restarts = value;
        }

        [TaskPane("StartTemperatureCaption", "StartTemperatureTooltip", "AlgorithmSettingsGroup", 15, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, int.MaxValue)]
        public int StartTemperature
        {
            get => _startTemperature;
            set => _startTemperature = value;
        }

        public string LetterLimits
        {
            get => _letterLimits;
            set
            {
                if (value != _letterLimits)
                {
                    _letterLimits = value;
                    OnPropertyChanged("LetterLimits");
                }
            }
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ShowSettingsElement(string element)
        {
            TaskPaneAttributeChanged?.Invoke(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Visible)));
        }

        private void HideSettingsElement(string element)
        {
            TaskPaneAttributeChanged?.Invoke(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Collapsed)));
        }

        private void UpdateSettingsVisibilities()
        {
            if (_keyLetterDistributionType == KeyLetterDistributionType.Uniform)
            {
                ShowSettingsElement(nameof(Homophonicity));
            }
            else
            {
                HideSettingsElement(nameof(Homophonicity));
            }
        }

        public void Initialize()
        {
            UpdateSettingsVisibilities();
        }
    }
}
