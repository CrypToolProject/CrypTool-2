/*
   Copyright 2008 Timm Korte, University of Siegen

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

namespace ClipboardInput
{
    public class ClipboardInputSettings : ISettings
    {
        private int format = 0; //0="Text", 1="Hex", 2="Base64"

        [ContextMenu("FormatCaption", "FormatTooltip", 1, ContextMenuControlType.ComboBox, null, new string[] { "FormatList1", "FormatList2", "FormatList3" })]
        [TaskPane("FormatCaption", "FormatTooltip", null, 1, false, ControlType.ComboBox, new string[] { "FormatList1", "FormatList2", "FormatList3" })]
        public int Format
        {
            get => format;
            set
            {
                if (format != value)
                {
                    format = value;
                    OnPropertyChanged("Format");
                }

            }
        }

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
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
