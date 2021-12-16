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
using System.ComponentModel;

namespace CrypTool.Plugins.DECRYPTTools
{
    internal class DECRYPTClustererSettings : ISettings
    {
        private double _matchThreshold;
        public event PropertyChangedEventHandler PropertyChanged;

        [TaskPane("MatchThresholdCaption", "MatchThresholdTooltip", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeDouble, 0, 100.0, 0.5)]
        public double MatchThreshold
        {
            get => _matchThreshold;
            set
            {
                if ((value) != _matchThreshold)
                {
                    _matchThreshold = value;
                    OnPropertyChanged("MatchThreshold");
                }
            }
        }

        public void Initialize()
        {

        }

        protected void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }
    }
}
