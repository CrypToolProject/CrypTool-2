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
using CrypTool.LorenzSZ42.SZ42Machine;
using CrypTool.PluginBase;
using System.ComponentModel;
using System.Windows;

namespace CrypTool.LorenzSZ42
{
    public class LorenzSZ42Settings : ISettings
    {
        #region INotifyPropertyChanged Members        

        private Action _action = Action.Encrypt;
        private Limitation _limitation = Limitation.NO_LIMITATION;
        private BaudotNotation _inputBaudotNotation = BaudotNotation.Raw;
        private BaudotNotation _outputBaudotNotation = BaudotNotation.Raw;

        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {
        }

        [TaskPane("ActionCaption", "ActionTooltip", null, 1, false, ControlType.ComboBox, new string[] { "Encrypt", "Decrypt", "GenerateKey" })]
        public Action Action
        {
            get
            {
                return _action;
            }
            set
            {
                if (_action != value)
                {
                    _action = value;
                    OnPropertyChanged(nameof(Action));
                }
            }
        }

        [TaskPane("LimitationCaption", "LimitationTooltip", null, 2, false, ControlType.ComboBox, new string[] { "NO_LIMITATION", "CHI2_1BACK", "PSI1_1BACK", "P5_2BACK", "PSI1_1BACK_P5_2BACK" })]
        public Limitation Limitation
        {
            get
            {
                return _limitation;
            }
            set
            {
                if (_limitation != value)
                {
                    _limitation = value;
                    OnPropertyChanged(nameof(Limitation));
                }
            }
        }

        [TaskPane("InputBaudotNotationAction", "InputBaudotNotationTooltip", "BaudotNotationGroup", 3, false, ControlType.ComboBox, new string[] { "Raw", "Readable", "British" })]
        public BaudotNotation InputBaudotNotation
        {
            get
            {
                return _inputBaudotNotation;
            }
            set
            {
                if (_inputBaudotNotation != value)
                {
                    _inputBaudotNotation = value;
                    OnPropertyChanged(nameof(InputBaudotNotation));
                }
            }
        }

        [TaskPane("OutputBaudotNotationAction", "OutputBaudotNotationTooltip", "BaudotNotationGroup", 4, false, ControlType.ComboBox, new string[] { "Raw", "Readable", "British" })]
        public BaudotNotation OutputBaudotNotation
        {
            get
            {
                return _outputBaudotNotation;
            }
            set
            {
                if (_outputBaudotNotation != value)
                {
                    _outputBaudotNotation = value;
                    OnPropertyChanged(nameof(OutputBaudotNotation));
                }
            }
        }

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

        /// <summary>
        /// This event is needed in order to render settings elements visible/invisible
        /// </summary>
        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        private void ShowSettingsElement(string element)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Visible)));
            }
        }

        private void HideSettingsElement(string element)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Collapsed)));
            }
        }
    }
}