/*
   Copyright 2020 Nils Kopal <Nils.Kopal<at>CrypTool.org

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

using System.ComponentModel;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;

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
        private int _wordCountToFind = 5;
        private int _minWordLength = 8;
        private int _maxWordLength = 10;
        private AnalysisMode _analysisMode;
        private int _cycles = 50000;
        private int _restarts = 1000;
        private int _costFunctionMultiplicator = 500000;
        private int _fixedTemperature = 15000;
        private bool _useNulls = false;
        private bool _keepLinebreaks = false;
        private string _letterLimits = string.Empty;
        #endregion

        #region TaskPane Settings      

        [TaskPane("LanguageCaption", "LanguageTooltip", "LanguageSettingsGroup", 0, false, ControlType.LanguageSelector)]
        public int Language
        {
            get 
            { 
                return _language; 
            }
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
            get 
            { 
                return _useSpaces; 
            }
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
            get
            {
                return _useNulls;
            }
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
            get { return _keepLinebreaks; }
            set { _keepLinebreaks = value; }
        }

        [TaskPane("CiphertextFormatCaption", "CiphertextFormatTooltip", "TextFormatGroup", 4, false, ControlType.ComboBox, new[] { "SingleLetters", "NumberGroups", "SymbolSeparated" })]
        public CiphertextFormat CiphertextFormat
        {
            get { return _ciphertextFormat; }
            set { _ciphertextFormat = value; }
        }

        [TaskPane("SeparatorCaption", "SeparatorTooltip", "TextFormatGroup", 5, false, ControlType.ComboBox, new[] { "Comma", "FullStop", "Semicolon", "Space" })]
        public Separator Separator
        {
            get { return _separator; }
            set { _separator = value; }
        }

        [TaskPane("WordCountToFindCaption", "WordCountToFindTooltip", "WordLockerGroup", 6, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 100)]
        public int WordCountToFind
        {
            get { return _wordCountToFind; }
            set { _wordCountToFind = value; }
        }

        [TaskPane("MinWordLengthCaption", "MinWordLengthTooltip", "WordLockerGroup", 7, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 100)]
        public int MinWordLength
        {
            get { return _minWordLength; }
            set { _minWordLength = value; }
        }

        [TaskPane("MaxWordLengthCaption", "MaxWordLengthTooltip", "WordLockerGroup", 8, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 100)]
        public int MaxWordLength
        {
            get { return _maxWordLength; }
            set { _maxWordLength = value; }
        }       

        [TaskPane("AnalysisModeCaption", "AnalaysisModeTooltip", "AlgorithmSettingsGroup", 9, false, ControlType.ComboBox, new string[]{"SemiAutomatic", "FullAutomatic"})]
        public AnalysisMode AnalysisMode
        {
            get { return _analysisMode; }
            set { _analysisMode = value; }
        }

        [TaskPane("CyclesCaption", "CyclesTooltip", "AlgorithmSettingsGroup", 10, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
        public int Cycles
        {
            get { return _cycles; }
            set { _cycles = value; }
        }

        [TaskPane("RestartsCaption", "RestartsTooltip", "AlgorithmSettingsGroup", 11, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, int.MaxValue)]
        public int Restarts
        {
            get { return _restarts; }
            set { _restarts = value; }
        }

        [TaskPane("CostFactorMultiplicatorCaption", "CostFactorMultiplicatorTooltip", "AlgorithmSettingsGroup", 12, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, int.MaxValue)]
        public int CostFactorMultiplicator
        {
            get { return _costFunctionMultiplicator; }
            set { _costFunctionMultiplicator = value; }
        }

        [TaskPane("FixedTemperatureCaption", "FixedTemperatureTooltip", "AlgorithmSettingsGroup", 13, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, int.MaxValue)]
        public int FixedTemperature
        {
            get { return _fixedTemperature; }
            set { _fixedTemperature = value; }
        }

        public string LetterLimits
        {
            get
            {
                return _letterLimits;
            }
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

        public void Initialize()
        {

        }
    }
}
