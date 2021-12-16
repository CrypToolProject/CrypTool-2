/* HOWTO: Change year, author name and organization.
   Copyright 2010 Your Name, University of Duckburg

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

namespace SolitaireAnalyser
{
    public class SolitaireAnalyserSettings : ISettings
    {
        #region Private Variables

        private int numberOfCards = 54;

        #endregion

        #region TaskPane Settings

        [TaskPane("NumberOfCardsCaption", "NumberOfCardsTooltip", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 3, 54)]
        public int NumberOfCards
        {
            get => numberOfCards;
            set
            {
                if (numberOfCards != value)
                {
                    numberOfCards = value;
                    OnPropertyChanged("NumberOfCards");
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        private void OnPropertyChanged(string property)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, property);
        }

        #endregion
    }
}
