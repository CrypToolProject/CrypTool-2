/*                              
   Copyright 2009 Team CrypTool (Sven Rech,Dennis Nolte,Raoul Falk,Nils Kopal), Uni Duisburg-Essen

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

namespace CrypTool.Plugins.Comparators
{
    public class ComparatorsSettings : ISettings
    {
        #region private variables
        private int comparator = 0; // 0 ==, 1 !=, 2 <, 3 >, 4 <=, 5 >=
        #endregion

        #region taskpane
        [TaskPane("ComparatorCaption", "ComparatorTooltip", null, 1, false, ControlType.ComboBox, new string[] { "ComparatorList1", "ComparatorList2", "ComparatorList3", "ComparatorList4", "ComparatorList5", "ComparatorList6" })]
        public int Comparator
        {
            get => comparator;
            set
            {
                if (value != comparator)
                {
                    comparator = value;
                    OnPropertyChanged("Comparator");

                    ChangePluginIcon(comparator);
                }
            }
        }
        #endregion

        #region INotifyPropertyChanged Member

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        protected void OnPropertyChanged(string name)
        {

        }
        public event StatusChangedEventHandler OnPluginStatusChanged;
        internal void ChangePluginIcon(int iconIndex)
        {
            if (OnPluginStatusChanged != null)
            {
                OnPluginStatusChanged(null, new StatusEventArgs(StatusChangedMode.ImageUpdate, iconIndex));
            }
        }
        #endregion
    }
}
