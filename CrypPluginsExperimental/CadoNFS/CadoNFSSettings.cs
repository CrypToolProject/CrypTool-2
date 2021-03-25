/*
   Copyright CrypTool 2 Team <ct2contact@cryptool.org>

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
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;

namespace CrypTool.Plugins.CadoNFS
{
    public class CadoNFSSettings : ISettings
    {
        #region Private Variables

        private int coresUsed;

        #endregion

        #region TaskPane Settings

        /// <summary>
        /// Amount of cores to use for the factorization. Index into list "CoresAvailable".
        /// </summary>
        [TaskPane("CoresUsedCaption", "CoresUsedTooltip", null, 1, false, ControlType.DynamicComboBox, new string[] { nameof(CoresAvailable) })]
        public int CoresUsedIndex
        {
            get { return this.coresUsed; }
            set
            {
                if (value != this.coresUsed)
                {
                    this.coresUsed = value;
                    OnPropertyChanged("CoresUsed");
                }
            }
        }

        /// <summary>
        /// Available amount of cores of this system
        /// </summary>
        public ObservableCollection<string> CoresAvailable { get; }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion

        public CadoNFSSettings()
        {
            CoresAvailable = new ObservableCollection<string>();
            //Implementation has trouble right now to run more than one worker, so limit it here to one:
            CoresAvailable.Add("1");
            CoresUsedIndex = 0;
            /*
            for (int i = 0; i < Environment.ProcessorCount; i++)
                CoresAvailable.Add((i + 1).ToString());
            CoresUsed = Environment.ProcessorCount - 1;
            */
        }

        public void Initialize()
        {
        }
    }
}
