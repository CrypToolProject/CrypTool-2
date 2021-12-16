/* HOWTO: Set year, author name and organization.
   Copyright 2011 CrypTool 2 Team

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
using System.ComponentModel;

namespace CrypTool.Plugins.Paillier
{
    public class PaillierSettings : ISettings
    {
        #region Private Variables

        private int action;

        #endregion

        #region TaskPane Settings

        /// <summary>
        /// Getter/Setter for the action (encryption or decryption)
        /// </summary>
        [ContextMenu("ActionCaption", "ActionTooltip", 1, ContextMenuControlType.ComboBox, null, "ActionList1", "ActionList2", "ActionList3", "ActionList4")]
        [TaskPane("ActionCaption", "ActionTooltip", null, 1, false, ControlType.ComboBox, new string[] { "ActionList1", "ActionList2", "ActionList3", "ActionList4" })]
        public int Action
        {
            get => action;
            set
            {
                if (action != value)
                {
                    action = value;
                    ChangePluginIcon(action);
                    OnPropertyChanged("Action");
                }
            }
        }

        public void ChangePluginIcon(int index)
        {
            OnPluginStatusChanged(null, new StatusEventArgs(StatusChangedMode.ImageUpdate, index));
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        #endregion

    }
}
