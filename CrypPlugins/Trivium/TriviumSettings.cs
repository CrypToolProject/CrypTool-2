/*
   Copyright 2009 Sören Rinne, Ruhr-Universität Bochum, Germany

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

namespace CrypTool.Trivium
{
    public class TriviumSettings : ISettings
    {
        #region ISettings Members

        private int keystreamLength = 32;
        [TaskPane("KeystreamLengthCaption", "KeystreamLengthTooltip", null, 0, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
        public int KeystreamLength
        {
            get => keystreamLength;
            set
            {
                keystreamLength = value;
                OnPropertyChanged("KeystreamLength");
            }
        }

        private int initRounds = 1152;
        [TaskPane("InitRoundsCaption", "InitRoundsTooltip", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
        public int InitRounds
        {
            get => initRounds;
            set
            {
                initRounds = value;
                OnPropertyChanged("InitRounds");
            }
        }

        private bool useByteSwapping = false;
        [ContextMenu("UseByteSwappingCaption", "UseByteSwappingTooltip", 1, ContextMenuControlType.CheckBox, null, new string[] { "UseByteSwappingList1" })]
        [TaskPane("UseByteSwappingCaption", "UseByteSwappingTooltip", null, 2, false, ControlType.CheckBox, "", null)]
        public bool UseByteSwapping
        {
            get => useByteSwapping;
            set
            {
                useByteSwapping = value;
                OnPropertyChanged("UseByteSwapping");
            }
        }

        private bool hexOutput = false;
        [ContextMenu("HexOutputCaption", "HexOutputTooltip", 2, ContextMenuControlType.CheckBox, null, new string[] { "HexOutputList1" })]
        [TaskPane("HexOutputCaption", "HexOutputTooltip", null, 3, false, ControlType.CheckBox, "", null)]
        public bool HexOutput
        {
            get => hexOutput;
            set
            {
                hexOutput = value;
                OnPropertyChanged("HexOutput");
            }
        }

        private string inputKey = string.Empty;
        [TaskPane("InputKeySettingsCaption", "InputKeySettingsTooltip", null, 4, false, ControlType.TextBox, null)]
        public string InputKey
        {
            get => inputKey;
            set
            {
                inputKey = value;
                OnPropertyChanged("InputKey");
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        private void OnPropertyChanged(string prop)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, prop);
        }

        #endregion
    }
}
