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
using System.ComponentModel;
using System.Windows;
using CrypTool.PluginBase;
using CrypTool.Plugins.SymbolCipher.CipherImplementations;

namespace CrypTool.Plugins.SymbolCipher
{
    public enum SymbolCipherType
    {
        Pigpen,
        Musical
    }

    public class SymbolCipherSettings : ISettings
    {       

        public event PropertyChangedEventHandler PropertyChanged;
        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        private SymbolCipherType _symbolCipherType = SymbolCipherType.Pigpen;
        private MusicalCipherType _musicalCipherType = MusicalCipherType.DanielSchwenter;

        public void Initialize()
        {
            UpdateSettingsVisibility();
        }

        [TaskPane("SymbolCipherTypeCaption", "SymbolCipherTypeTooltip", null, 0, false, ControlType.ComboBox, new string[] { "Pigpen", "Musical"})]
        public SymbolCipherType SymbolCipherType
        {
            get => _symbolCipherType;
            set
            {
                if (value != _symbolCipherType)
                {
                    _symbolCipherType = value;
                    UpdateSettingsVisibility();
                    OnPropertyChanged("SymbolCipherType");
                }
            }
        }

        [TaskPane("MusicalCipherTypeCaption", "MusicalCipherTypeTooltip", "MusicalCipherGroup", 1, false, ControlType.ComboBox, new string[] { "Daniel Schwenter", "John Wilkins" })]
        public MusicalCipherType MusicalCipherType
        {
            get => _musicalCipherType;
            set
            {
                if (value != _musicalCipherType)
                {
                    _musicalCipherType = value;
                    OnPropertyChanged("MusicalCipherType");
                }
            }
        }

        private void UpdateSettingsVisibility()
        {
            ShowHideSetting("MusicalCipherType", false);

            if (_symbolCipherType == SymbolCipherType.Musical)
            {
                ShowHideSetting("MusicalCipherType", true);
            }
        }

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void ShowHideSetting(string propertyName, bool show)
        {
            if (TaskPaneAttributeChanged == null)
            {
                return;
            }

            TaskPaneAttribteContainer container = new TaskPaneAttribteContainer(propertyName, show == true ? Visibility.Visible : Visibility.Collapsed);
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(container));
        }
    }
}
