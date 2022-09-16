/*
   Copyright 2022 Nils Kopal <Nils.Kopal<at>CrypTool.org

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

namespace CrypTool.Nihilist
{
    public enum Action
    {
        Encrypt = 0,
        Decrypt = 1
    }

    public enum AlphabetVersion
    {
        Twentyfive,
        Thirtysix
    }

    public enum UnknownSymbolHandlingMode
    {
        Ignore = 0,
        Remove = 1,
        Replace = 2
    }

    public class NihilistSettings : ISettings
    {
        private Action _action = Action.Encrypt;
        private AlphabetVersion _alphabetVersion = AlphabetVersion.Twentyfive;
        private UnknownSymbolHandlingMode unknownSymbolHandling = UnknownSymbolHandlingMode.Ignore;

        public NihilistSettings()
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
                    OnPropertyChanged(nameof(Action));
                }
            }
        }

        [TaskPane("AlphabetVersionCaption", "AlphabetVersionTooltip", null, 1, false, ControlType.ComboBox, new string[] { "Twentyfive", "Thirtysix" })]
        public AlphabetVersion AlphabetVersion
        {
            get => _alphabetVersion;
            set
            {
                if (value != _alphabetVersion)
                {
                    _alphabetVersion = value;
                    OnPropertyChanged(nameof(AlphabetVersion));
                }
            }
        }

        [TaskPane("UnknownSymbolHandlingCaption", "UnknownSymbolHandlingTooltip", null, 3, false, ControlType.ComboBox, new string[] { "UnknownSymbolHandlingList1", "UnknownSymbolHandlingList2", "UnknownSymbolHandlingList3" })]
        public UnknownSymbolHandlingMode UnknownSymbolHandling
        {
            get => unknownSymbolHandling;
            set
            {
                if (value != unknownSymbolHandling)
                {
                    unknownSymbolHandling = value;
                    OnPropertyChanged(nameof(UnknownSymbolHandling));
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
