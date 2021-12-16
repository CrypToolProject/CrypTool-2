/*
   Copyright 2013 Nils Kopal <Nils.Kopal@Uni-Kassel.de>

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

namespace CrypTool.Plugins.MorseCode
{
    public class MorseCodeSettings : ISettings
    {
        public enum CodeType
        {
            International_ITU,
            American_Morse,
            Continental,
            Navy,
            TapCode
        }
        public enum ActionType
        {
            Encode = 0,
            Decode = 1,
            Play = 2
        }
        public event PropertyChangedEventHandler PropertyChanged;

        #region Private Variables

        private CodeType _codeType = CodeType.International_ITU;
        private ActionType _action;
        private int _frequency = 600;
        private int _tickDuration = 50;
        private double _volume = 1.0;

        #endregion

        #region TaskPane Settings

        [TaskPane("CodeCaption", "CodeTooltip", null, 0, false, ControlType.ComboBox, new string[] { "International (ITU)", "American", "Continental", "Navy", "Tap Code" })]
        public CodeType Code
        {
            get => _codeType;
            set
            {
                if (value != _codeType)
                {
                    _codeType = value;
                    OnPropertyChanged("CodeType");
                    Initialize(); // we call this to show and hide settings
                }
            }
        }

        [TaskPane("ActionCaption", "ActionTooltip", null, 1, false, ControlType.ComboBox, new string[] { "Encode", "Decode", "Play" })]
        public ActionType Action
        {
            get => _action;
            set
            {
                if (value != _action)
                {
                    _action = value;
                    OnPropertyChanged("Action");
                    Initialize(); // we call this to show and hide settings
                }
            }
        }

        [TaskPane("FrequencyCaption", "FrequencyTooltip", null, 2, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 300, 1200)]
        public int Frequency
        {
            get => _frequency;
            set
            {
                if (value != _frequency)
                {
                    _frequency = value;
                    OnPropertyChanged("Frequency");
                }
            }
        }

        [TaskPane("TickDurationCaption", "TickDurationTooltip", null, 3, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 50, 250)]
        public int TickDuration
        {
            get => _tickDuration;
            set
            {
                if (value != _tickDuration)
                {
                    _tickDuration = value;
                    OnPropertyChanged("TickDuration");
                }
            }
        }

        [TaskPane("VolumeCaption", "VolumeTooltip", null, 4, false, ControlType.Slider, 0, 1.0)]
        public double Volume
        {
            get => _volume;
            set
            {
                if (value != _volume)
                {
                    _volume = value;
                    OnPropertyChanged("Volume");
                }
            }
        }

        #endregion

        #region Events

        public void Initialize()
        {
            if (Action == ActionType.Play)
            {
                showSettingsElement("Frequency");
                showSettingsElement("TickDuration");
                showSettingsElement("Volume");
            }
            else
            {
                hideSettingsElement("Frequency");
                hideSettingsElement("TickDuration");
                hideSettingsElement("Volume");
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        /// <summary>
        /// This event is needed in order to render settings elements visible/invisible
        /// </summary>
        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        private void showSettingsElement(string element)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Visible)));
            }
        }

        private void hideSettingsElement(string element)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Collapsed)));
            }
        }

        #endregion
    }
}
