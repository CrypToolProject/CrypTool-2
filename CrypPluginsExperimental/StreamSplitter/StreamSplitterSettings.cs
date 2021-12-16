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

namespace CrypTool.Plugins.StreamSplitter
{
    // HOWTO: rename class (click name, press F2)
    public class StreamSplitterSettings : ISettings
    {
        #region Private Variables

        private int _offset;

        #endregion

        #region TaskPane Settings

        /// <summary>
        /// HOWTO: This is an example for a setting entity shown in the settings pane on the right of the CT2 main window.
        /// This example setting uses a number field input, but there are many more input types available, see ControlType enumeration.
        /// </summary>
        [TaskPane("OffsetCaption", "OffsetTooltip",
            null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, int.MinValue, int.MaxValue)]

        public int Offset //Note that when the name matches an input name, this setting disappear when the input is connected
        {
            get => _offset;
            set
            {
                _offset = value;
                // HOWTO: MUST be called every time a property value changes with correct parameter name
                OnPropertyChanged("Offset");
            }
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion

        public void Initialize()
        {
            Offset = 0;
        }
    }
}