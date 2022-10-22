/*
   Copyright 2022 Nils Kopal, CrypTool project

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
using System.ComponentModel;

namespace CrypTool.BookCipher
{
    public enum Action
    {
        Encrypt = 0,
        Decrypt = 1
    }

    public enum EncodingMode
    {
        FirstLetter = 0,
        CompleteWord = 1
    }

    public class BookCipherSettings : ISettings
    {
        private const string DEFAULT_PAGE_SEPARATOR = "<newpage>";
        private Action _action = Action.Encrypt;
        private EncodingMode _encodingMode = EncodingMode.FirstLetter;
        private bool _encodePages = false;
        private bool _encodeLines = false;
        private string _pageSeparator = DEFAULT_PAGE_SEPARATOR;

        private int _pageDigits = 2;
        private int _lineDigits = 2;        
        private int _wordDigits = 3;

        public BookCipherSettings()
        {

        }

        [TaskPane("ActionCaption", "ActionTooltip", null, 1, false, ControlType.ComboBox, new string[] { "Encrypt", "Decrypt" })]
        public Action Action
        {
            get => _action;
            set
            {
                if (value != _action)
                {
                    _action = value;
                    OnPropertyChanged("Action");
                }
            }
        }       

        [TaskPane("EncodingModeCaption", "EncodingModeTooltip", null, 2, false, ControlType.ComboBox, new string[] { "FirstLetter", "CompleteWord" })]
        public EncodingMode EncodingMode
        {
            get => _encodingMode;
            set
            {
                if (value != _encodingMode)
                {
                    _encodingMode = value;
                    OnPropertyChanged(nameof(EncodingMode));
                }
            }
        }

        [TaskPane("EncodePagesCaption", "EncodePagesTooltip", null, 3, false, ControlType.CheckBox)]
        public bool EncodePages
        {
            get => _encodePages;
            set
            {
                if (value != _encodePages)
                {
                    _encodePages = value;
                    OnPropertyChanged(nameof(EncodePages));
                }
            }
        }

        [TaskPane("EncodeLinesCaption", "EncodeLinesTooltip", null, 4, false, ControlType.CheckBox)]
        public bool EncodeLines
        {
            get => _encodeLines;
            set
            {
                if (value != _encodeLines)
                {
                    _encodeLines = value;
                    OnPropertyChanged(nameof(EncodeLines));
                }
            }
        }

        [TaskPane("PageSeparatorCaption", "PageSeparatorTooltip", null, 5, false, ControlType.TextBox,ValidationType.RegEx, "^(?!\\s*$).+")]
        public string PageSeparator
        {
            get
            {
                if(!string.IsNullOrEmpty(_pageSeparator) && !string.IsNullOrWhiteSpace(_pageSeparator))
                {
                    return _pageSeparator;
                }
                return DEFAULT_PAGE_SEPARATOR;
            }
            set
            {
                if (value != _pageSeparator)
                {
                    _pageSeparator = value;
                    OnPropertyChanged(nameof(PageSeparator));
                }
            }
        }

        [TaskPane("PageDigitsCaption", "PageDigitsTooltip", null, 6, false, ControlType.NumericUpDown, ValidationType.RangeInteger,1,10)]
        public int PageDigits
        {
            get
            {
                return _pageDigits;
            }
            set
            {
                if (value != _pageDigits)
                {
                    _pageDigits = value;
                    OnPropertyChanged(nameof(PageDigits));
                }
            }
        }

        [TaskPane("LineDigitsCaption", "LineDigitsTooltip", null, 7, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 10)]
        public int LineDigits
        {
            get
            {
                return _lineDigits;
            }
            set
            {
                if (value != _lineDigits)
                {
                    _lineDigits = value;
                    OnPropertyChanged(nameof(LineDigits));
                }
            }
        }

        [TaskPane("WordDigitsCaption", "WordDigitsTooltip", null, 8, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 10)]
        public int WordDigits
        {
            get
            {
                return _wordDigits;
            }
            set
            {
                if (value != _wordDigits)
                {
                    _wordDigits = value;
                    OnPropertyChanged(nameof(WordDigits));
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
