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
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using CrypTool.Plugins.DECRYPTTools.Util;
using System.Collections.Generic;
using System.ComponentModel;

namespace CrypTool.Plugins.DECRYPTTools
{
    public enum ParserType
    {
        SimpleSingleTokenParser = 0,
        NoNomenclatureParser = 1,
        Nomenclature3DigitsEndingWithNull1DigitsParser = 2,
        Nomenclature3DigitsEndingWithNull2DigitsParser = 3,
        Nomenclature4DigitsWithPrefixParser = 4,
        Francia4Parser = 5,
        Francia6Parser = 6,
        Francia17Parser = 7,
        Francia18Parser = 8,
        VariableLengthHomophonicCipher = 9,
        Francia346 = 10,
        Francia283 = 11,
        ManuallySplittedTextParser = 12
    }

    internal class DECRYPTDeciphererSettings : ISettings
    {
        private ParserType _parserType;
        private string _nulls;
        private string _prefix;
        private bool _useKeyAsPlaintext = false;
        private bool _useOutputSeparators = false;
        private bool _showCommentsPlaintextCleartext = false;
        private bool _showUnknownTranscriptionSymbols = false;

        public event PropertyChangedEventHandler PropertyChanged;

        [TaskPane("ParserTypeCaption", "ParserTypeTooltip", null, 1, false, ControlType.ComboBox, new string[]
        {
            "SimpleSingleTokenParser",
            "NoNomenclatureParser",
            "Nomenclature3DigitsEndingWithNull1DigitsParser",
            "Nomenclature3DigitsEndingWithNull2DigitsParser",
            "Nomenclature4DigitsWithPrefixParser",
            "Francia4Parser",
            "Francia6Parser",
            "Francia17Parser",
            "Francia18Parser",
            "VariableLengthHomophonicCipher",
            "Francia346",
            "Francia383",
            "ManuallySplittedTextParser"
        })]
        public ParserType ParserType
        {
            get => _parserType;
            set
            {
                if ((value) != _parserType)
                {
                    _parserType = value;
                    OnPropertyChanged("ParserType");
                }
            }
        }

        [TaskPane("NullsCaption", "NullsTooltip", null, 2, false, ControlType.TextBox)]
        public string Nulls
        {
            get => _nulls;
            set
            {
                if ((value) != _nulls)
                {
                    _nulls = value;
                    OnPropertyChanged("Nulls");
                }
            }
        }

        [TaskPane("PrefixCaption", "PrefixTooltip", null, 3, false, ControlType.TextBox)]
        public string Prefix
        {
            get => _prefix;
            set
            {
                if ((value) != _prefix)
                {
                    _prefix = value;
                    OnPropertyChanged("Prefix");
                }
            }
        }

        [TaskPane("UseKeyAsPlaintextCaption", "UseKeyAsPlaintextTooltip", null, 4, false, ControlType.CheckBox)]
        public bool UseKeyAsPlaintext
        {
            get => _useKeyAsPlaintext;
            set
            {
                if ((value) != _useKeyAsPlaintext)
                {
                    _useKeyAsPlaintext = value;
                    OnPropertyChanged("UseKeyAsPlaintext");
                }
            }
        }

        [TaskPane("UseOutputSeparatorsCaption", "UseOutputSeparatorsTooltip", null, 5, false, ControlType.CheckBox)]
        public bool UseOutputSeparators
        {
            get => _useOutputSeparators;
            set
            {
                if ((value) != _useOutputSeparators)
                {
                    _useOutputSeparators = value;
                    OnPropertyChanged("UseOutputSeparators");
                }
            }
        }

        [TaskPane("ShowCommentsPlaintextCleartextCaption", "ShowCommentsPlaintextCleartextTooltip", null, 6, false, ControlType.CheckBox)]
        public bool ShowCommentsPlaintextCleartext
        {
            get => _showCommentsPlaintextCleartext;
            set
            {
                if ((value) != _showCommentsPlaintextCleartext)
                {
                    _showCommentsPlaintextCleartext = value;
                    OnPropertyChanged("ShowCommentsPlaintextCleartext");
                }
            }
        }

        [TaskPane("ShowUnknownTranscriptionSymbolsCaption", "ShowUnknownTranscriptionSymbolsTooltip", null, 7, false, ControlType.CheckBox)]
        public bool ShowUnknownTranscriptionSymbols
        {
            get => _showUnknownTranscriptionSymbols;
            set
            {
                if ((value) != _showUnknownTranscriptionSymbols)
                {
                    _showUnknownTranscriptionSymbols = value;
                    OnPropertyChanged("ShowUnknownTranscriptionSymbols");
                }
            }
        }

        public void Initialize()
        {

        }

        public List<Token> GetNulls()
        {
            List<Token> list = new List<Token>();
            string[] nulls = _nulls.Split(',');
            for (int i = 0; i < nulls.Length; i++)
            {
                Token token = new Token(null);
                Symbol symbol = new Symbol(token);
                token.Symbols.Add(symbol);
                symbol.Text = nulls[i].Trim();
                list.Add(token);
            }
            return list;
        }

        public List<Token> GetPrefix()
        {
            List<Token> list = new List<Token>();
            string[] prefix = _prefix.Split(',');
            for (int i = 0; i < prefix.Length; i++)
            {
                Token token = new Token(null);
                Symbol symbol = new Symbol(token);
                token.Symbols.Add(symbol);
                symbol.Text = prefix[i].Trim();
                list.Add(token);
            }
            return list;
        }

        protected void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }
    }
}
