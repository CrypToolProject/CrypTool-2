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
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace CrypTool.Plugins.RSA
{
    /// <summary>
    /// Settings class for the RSA plugin
    /// </summary>
    internal class RSASettings : ISettings
    {
        #region private members

        private int action;
        private int coresUsed;
        private int inputBlocksize = 8;
        private int outputBlocksize = 8;
        private bool overrideBlocksizes = false;
        private ObservableCollection<string> coresAvailable = new ObservableCollection<string>();

        #endregion

        #region events

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {
            UpdateTaskPaneVisibility();
        }

        #endregion

        #region public

        /// <summary>
        /// Constructs a new RSASettings
        /// detects the number of cores of the system and sets those to maximum number of cores
        /// which can be used
        /// </summary>
        public RSASettings()
        {
            CoresAvailable.Clear();
            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                CoresAvailable.Add((i + 1).ToString());
            }

            CoresUsed = Environment.ProcessorCount - 1;
        }

        /// <summary>
        /// Get the number of cores in a collection, used for the selection of cores
        /// </summary>
        public ObservableCollection<string> CoresAvailable
        {
            get => coresAvailable;
            set
            {
                if (value != coresAvailable)
                {
                    coresAvailable = value;
                    OnPropertyChanged("CoresAvailable");
                }
            }
        }

        /// <summary>
        /// Getter/Setter for the number of cores which should be used by RSA
        /// </summary>
        [TaskPane("CoresUsedCaption", "CoresUsedTooltip", null, 1, false, ControlType.DynamicComboBox, new string[] { "CoresAvailable" })]
        public int CoresUsed
        {
            get => coresUsed;
            set
            {
                if (value != coresUsed)
                {
                    coresUsed = value;
                    OnPropertyChanged("CoresUsed");
                }
            }
        }

        /// <summary>
        /// Getter/Setter for the action (encryption or decryption)
        /// </summary>
        [ContextMenu("ActionCaption", "ActionTooltip", 1, ContextMenuControlType.ComboBox, new int[] { 1, 2 }, "ActionList1", "ActionList2")]
        [TaskPane("ActionCaption", "ActionTooltip", null, 1, false, ControlType.ComboBox, new string[] { "ActionList1", "ActionList2" })]
        public int Action
        {
            get => action;
            set
            {
                if (action != value)
                {
                    action = value;
                    OnPropertyChanged("Action");
                }
            }
        }

        /// <summary>
        /// Getter/Setter for the override blocksize checkbox
        /// </summary>
        [TaskPane("OverrideBlocksizesCaption", "OverrideBlocksizesTooltip", "OverrideBlocksizesGroup", 2, false, ControlType.CheckBox)]
        public bool OverrideBlocksizes
        {
            get => overrideBlocksizes;
            set
            {
                if (overrideBlocksizes != value)
                {
                    overrideBlocksizes = value;
                    UpdateTaskPaneVisibility();
                    OnPropertyChanged("OverrideBlocksizes");
                }
            }
        }

        /// <summary>
        /// Getter/Setter for the input blocksize
        /// </summary>
        [TaskPane("InputBlocksizeCaption", "InputBlocksizeTooltip", "OverrideBlocksizesGroup", 3, false, ControlType.TextBox, ValidationType.RegEx, "^[0-9]{1,}$")]
        public int InputBlocksize
        {
            get => inputBlocksize;
            set
            {
                if (inputBlocksize != value)
                {
                    inputBlocksize = value;
                    OnPropertyChanged("InputBlocksize");
                }
            }
        }

        /// <summary>
        /// Getter/Setter for the input blocksize
        /// </summary>
        [TaskPane("OutputBlocksizeCaption", "OutputBlocksizeTooltip", "OverrideBlocksizesGroup", 4, false, ControlType.TextBox, ValidationType.RegEx, "^[0-9]{1,}$")]
        public int OutputBlocksize
        {
            get => outputBlocksize;
            set
            {
                if (outputBlocksize != value)
                {
                    outputBlocksize = value;
                    OnPropertyChanged("OutputBlocksize");
                }
            }
        }

        #endregion

        #region private

        /// <summary>
        /// The property p has changes
        /// </summary>
        /// <param name="p">p</param>
        private void OnPropertyChanged(string p)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(p));
            }
        }

        #endregion

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        private void settingChanged(string setting, Visibility vis)
        {
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(setting, vis)));
        }

        internal void UpdateTaskPaneVisibility()
        {
            if (TaskPaneAttributeChanged == null)
            {
                return;
            }

            if (OverrideBlocksizes)
            {
                settingChanged("InputBlocksize", Visibility.Visible);
                settingChanged("OutputBlocksize", Visibility.Visible);
            }
            else
            {
                settingChanged("InputBlocksize", Visibility.Collapsed);
                settingChanged("OutputBlocksize", Visibility.Collapsed);
            }
        }


    }//end RSASettings

}//end CrypTool.Plugins.RSA
