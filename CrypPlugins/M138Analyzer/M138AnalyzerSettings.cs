/*
   Copyright 2011 CrypTool 2 Team <ct2contact@CrypTool.org>

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

namespace CrypTool.M138Analyzer
{
    public class M138AnalyzerSettings : ISettings
    {
        #region Private Variables
        private int _analyticMode = 0;
        private int _language = 0;
        private int _keyLength = 25;
        private int _minOffset = 1;
        private int _maxOffset = 25;
        private int _retries = 100;
        private int _killcounter = 1000000;
        private readonly bool _fastConverge = false;

        #endregion

        #region TaskPane Settings

        [TaskPane("MethodCaption", "MethodTooltip", null, 0, false, ControlType.ComboBox, new string[] { "MethodList1", "MethodList2", "MethodList3", "MethodList4" })]
        public int Method
        {
            get => _analyticMode;
            set
            {
                if (_analyticMode != value)
                {
                    _analyticMode = value;
                    UpdateTaskPaneVisibility();
                }
            }
        }

        [TaskPane("LanguageCaption", "LanguageTooltip", null, 4, false, ControlType.LanguageSelector)]
        public int Language
        {
            get => _language;
            set => _language = value;
        }

        [TaskPane("KeyLengthCaption", "KeyLengthTooltip", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 10000000)]
        public int KeyLengthUserSelection
        {
            get => _keyLength;
            set => _keyLength = value;
        }

        [TaskPane("MinOffsetCaption", "MinOffsetTooltip", null, 2, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 10000000)]
        public int MinOffsetUserSelection
        {
            get => _minOffset;
            set => _minOffset = value;
        }

        [TaskPane("MaxOffsetCaption", "MaxOffsetTooltip", null, 3, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 10000000)]
        public int MaxOffsetUserSelection
        {
            get => _maxOffset;
            set => _maxOffset = value;
        }

        [TaskPane("HillClimbRestartsCaption", "HillClimbRestartsTooltip", null, 5, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 10000000)]
        public int HillClimbRestarts
        {
            get => _retries;
            set => _retries = value;
        }

        [TaskPane("FastConvergeCaption", "FastConvergeTooltip", null, 6, false, ControlType.CheckBox)]
        public bool FastConverge
        {
            get; set;
        }

        [TaskPane("KillCounterCaption", "KillCounterTooltip", null, 7, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 10000000)]
        public int KillCounter
        {
            get => _killcounter;
            set => _killcounter = value;
        }

        [TaskPane("HighscoreBeepCaption", "HighscoreBeepTooltip", null, 8, true, ControlType.CheckBox)]
        public bool HighscoreBeep
        {
            get; set;
        }

        #endregion

        #region Events

        public void UpdateTaskPaneVisibility()
        {
            switch (_analyticMode)
            {
                case 0: // Known Plaintext
                    SettingChanged("Language", Visibility.Collapsed);
                    SettingChanged("KeyLengthUserSelection", Visibility.Visible);
                    SettingChanged("MinOffsetUserSelection", Visibility.Visible);
                    SettingChanged("MaxOffsetUserSelection", Visibility.Visible);
                    SettingChanged("HillClimbRestarts", Visibility.Collapsed);
                    SettingChanged("FastConverge", Visibility.Collapsed);
                    SettingChanged("KillCounter", Visibility.Collapsed);
                    SettingChanged("HighscoreBeep", Visibility.Collapsed);
                    break;

                case 1: // Partially Known Plaintext
                    SettingChanged("Language", Visibility.Visible);
                    SettingChanged("KeyLengthUserSelection", Visibility.Visible);
                    SettingChanged("MinOffsetUserSelection", Visibility.Visible);
                    SettingChanged("MaxOffsetUserSelection", Visibility.Visible);
                    SettingChanged("HillClimbRestarts", Visibility.Visible);
                    SettingChanged("FastConverge", Visibility.Visible);
                    SettingChanged("KillCounter", Visibility.Collapsed);
                    SettingChanged("HighscoreBeep", Visibility.Collapsed);
                    break;

                case 2: // Hill Climbing
                    SettingChanged("Language", Visibility.Visible);
                    SettingChanged("KeyLengthUserSelection", Visibility.Visible);
                    SettingChanged("MinOffsetUserSelection", Visibility.Visible);
                    SettingChanged("MaxOffsetUserSelection", Visibility.Visible);
                    SettingChanged("HillClimbRestarts", Visibility.Visible);
                    SettingChanged("FastConverge", Visibility.Visible);
                    SettingChanged("KillCounter", Visibility.Collapsed);
                    SettingChanged("HighscoreBeep", Visibility.Visible);
                    break;

                case 3: // Simulated Annealing
                    SettingChanged("Language", Visibility.Visible);
                    SettingChanged("KeyLengthUserSelection", Visibility.Visible);
                    SettingChanged("MinOffsetUserSelection", Visibility.Visible);
                    SettingChanged("MaxOffsetUserSelection", Visibility.Visible);
                    SettingChanged("HillClimbRestarts", Visibility.Visible);
                    SettingChanged("FastConverge", Visibility.Collapsed);
                    SettingChanged("KillCounter", Visibility.Visible);
                    SettingChanged("HighscoreBeep", Visibility.Visible);
                    break;
            }
        }

        private void SettingChanged(string setting, Visibility vis)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(setting, vis)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;


        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        public void Initialize()
        {

        }

        #endregion
    }
}