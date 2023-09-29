/*
   Copyright 2022 Nils Kopal <kopal<AT>cryptool.org>

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

namespace CrypTool.SyllabaryCipher
{
    public enum Action
    {
        Encrypt = 0,
        Decrypt = 1
    }

    public enum TableLanguage
    {
        English = 0,
        Italian = 1,
        French = 2,
        German = 3,
        Spanish = 4,
        Latin = 5
    }

    public enum EncryptionStrategy
    {
        Longest = 0,
        Random = 1
    }

    public enum UnknownSymbolHandlingMode
    {
        Ignore = 0,
        Remove = 1,
        Replace = 2
    }    

    public enum TableOutputFormat
    {
        Formatted = 0,
        CrypToolSubstitutionKey = 1
    }

    public class SyllabaryCipherSettings : ISettings
    {
        private Action _action;        
        private TableLanguage _tableLanguage = TableLanguage.English;
        private EncryptionStrategy _encryptionStrategy = EncryptionStrategy.Longest;
        private UnknownSymbolHandlingMode _unknownSymbolHandlingMode = UnknownSymbolHandlingMode.Ignore;
        private TableOutputFormat _tableOutputFormat = TableOutputFormat.Formatted;

        [TaskPane("ActionCaption", "ActionTooltip", null, 1, false, ControlType.ComboBox, new string[] { "Encrypt", "Decrypt" })]
        public Action Action
        {
            get 
            { 
                return _action; 
            }
            set
            {
                if (value != _action)
                {
                    _action = value;
                    OnPropertyChanged(nameof(Action));                    
                }
            }
        }       

        [TaskPane("TableLanguageCaption", "TableLanguageTooltip", null, 2, false, ControlType.ComboBox, new string[] { "English", "Italian", "French", "German", "Spanish", "Latin" })]
        public TableLanguage TableLanguage
        {
            get => _tableLanguage;
            set
            {
                if (value != _tableLanguage)
                {
                    _tableLanguage = value;
                    OnPropertyChanged(nameof(TableLanguage));
                }
            }
        }

        [TaskPane("EncryptionStrategyCaption", "EncryptionStrategyTooltip", null, 3, false, ControlType.ComboBox, new string[] { "Longest", "Random" })]
        public EncryptionStrategy EncryptionStrategy
        {
            get => _encryptionStrategy;
            set
            {
                if (value != _encryptionStrategy)
                {
                    _encryptionStrategy = value;
                    OnPropertyChanged(nameof(EncryptionStrategy));
                }
            }
        }

        [TaskPane("UnknownSymbolHandlingCaption", "UnknownSymbolHandlingTooltip", null, 4, false, ControlType.ComboBox, new string[] { "UnknownSymbolHandlingList1", "UnknownSymbolHandlingList2", "UnknownSymbolHandlingList3" })]
        public UnknownSymbolHandlingMode UnknownSymbolHandling
        {
            get => _unknownSymbolHandlingMode;
            set
            {
                if (value != _unknownSymbolHandlingMode)
                {
                    _unknownSymbolHandlingMode = value;
                    OnPropertyChanged(nameof(UnknownSymbolHandlingMode));
                }
            }
        }

        [TaskPane("TableOutputFormatCaption", "TableOutputFormatTooltip", null, 4, false, ControlType.ComboBox, new string[] { "Formatted", "CrypToolSubstitutionKey" })]
        public TableOutputFormat TableOutputFormat
        {
            get => _tableOutputFormat;
            set
            {
                if (value != _tableOutputFormat)
                {
                    _tableOutputFormat = value;
                    OnPropertyChanged(nameof(TableOutputFormat));
                }
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {
            
        }

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}