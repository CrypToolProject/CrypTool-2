/*
   Copyright (C) CrypTool 2 Team

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

namespace CrypTool.WEPAttacks
{
    /// <summary>
    /// Some settings for the <see cref="WEPAttacks"/> plugin.
    /// </summary>
    public class WEPAttacksSettings : ISettings
    {
        #region Private variables

        /// <summary>
        /// Kind of attack. 0 ==> FMS, 1 ==> KoreK, 2 ==> PTW
        /// </summary>
        private int action = 0;
        private bool fileOrNot = false;
        private readonly string dataSource = string.Empty;

        /// <summary>
        /// Action. 0 => FMS, 1 => KoreK, 2 => PTW
        /// </summary>
        [ContextMenu("ActionCaption",
            "ActionTooltip",
            1,
            ContextMenuControlType.ComboBox,
            null,
            new string[] { "ActionList1", "ActionList2", "ActionList3" })]
        [TaskPane("ActionCaption",
            "ActionTooltip",
            null,
            1,
            false,
            ControlType.ComboBox,
            new string[] { "ActionList1", "ActionList2", "ActionList3" })]
        public int Action
        {
            get => action;
            set
            {
                if (value != action)
                {
                    action = value;
                    OnPropertyChanged("Action");
                }
            }
        }

        /// <summary>
        /// true => source is data, false => source is another plugin (NOT "File Input"!!!)
        /// </summary>
        [TaskPane("FileOrNotCaption",
            "FileOrNotTooltip",
            null,
            2,
            false,
            ControlType.CheckBox,
            new string[] { "FileOrNotList1" })]
        public bool FileOrNot
        {
            get => fileOrNot;
            set
            {
                if (value != fileOrNot)
                {
                    fileOrNot = value;
                    OnPropertyChanged("FileOrNot");
                }
            }
        }

        /*/// <summary>
        /// Indicates whether data comes from file or from another plugin (most propably the "Internet frame generator" plugin).
        /// Needed to react if attack was not successful.
        /// </summary>
         
        // Radiobuttons are not implemented yet, so I coment them out (11-25-2008)

        [TaskPane("Dealing with end of given data",
            "Dealing with end of given data",
            "groupRadiobutton",
            3,
            false,
            ControlType.RadioButton,
            new string[]{"Data comes from file (finish, if attack is not successful)",
                "Data comes from IFG (wait for further package, if attack is not successful yet)"})]
        public string DataSource
        {
            get
            {
                return this.dataSource;
            }
            set
            {
                if ((string)value != dataSource)
                {
                    this.dataSource = (string)value;
                    OnPropertyChanged("DataSource");
                }
            }
        }*/

        #endregion

        #region ISettings Member

        public event StatusChangedEventHandler OnPluginStatusChanged;
        private void ChangePluginIncon(int Icon)
        {
            if (OnPluginStatusChanged != null)
            {
                OnPluginStatusChanged(null, new StatusEventArgs(StatusChangedMode.ImageUpdate, Icon));
            }
        }

        #endregion

        #region INotifyPropertyChanged Member

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

        #endregion
    }
}
