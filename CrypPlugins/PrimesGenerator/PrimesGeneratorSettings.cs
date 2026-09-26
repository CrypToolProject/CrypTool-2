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

namespace CrypTool.PrimesGenerator
{
    public class PrimesGeneratorSettings : ISettings
    {
        #region Properties

        private int m_SelectedMode = 0;
        [PropertySaveOrder(1)]
        [ContextMenu("ModeCaption", "ModeTooltip", 1, ContextMenuControlType.ComboBox, null, new string[] { "ModeList1", "ModeList2", "ModeList3", "ModeList4", "ModeList5" })]
        [TaskPane("ModeCaption", "ModeTooltip", null, 1, false, ControlType.ComboBox, new string[] { "ModeList1", "ModeList2", "ModeList3", "ModeList4", "ModeList5" })]
        public int Mode
        {
            get => m_SelectedMode;
            set
            {
                if (value != m_SelectedMode)
                {
                    m_SelectedMode = value;
                    FirePropertyChangedEvent("Mode");
                }
            }
        }

        private string m_Input = "100";
        [PropertySaveOrder(2)]
        [TaskPane("InputCaption", "InputTooltip", null, 2, false, ControlType.TextBox, ValidationType.RegEx, "^[0-9]+$")]
        public string Input
        {
            get => m_Input;
            set
            {
                if (value != m_Input)
                {
                    m_Input = value;
                    FirePropertyChangedEvent("Input");
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        private void FirePropertyChangedEvent(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
