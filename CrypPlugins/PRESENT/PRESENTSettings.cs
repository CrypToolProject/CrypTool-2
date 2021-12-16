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
using CrypTool.PluginBase.Miscellaneous;
using System.ComponentModel;

namespace CrypTool.PRESENT
{
    public class PRESENTSettings : ISettings
    {
        private int action = 0; // 0=encrypt, 1=decrypt
        private int mode = 0; // 0="ECB", 1="CBC", 2="CFB", 3="OFB"
        private int padding = 1; // 0="None", 1="Zeros"=default, 2="PKCS7" , 3="ANSIX923", 4="ISO10126", 5=1-0-Padding

        public BlockCipherHelper.PaddingType[] padmap = new BlockCipherHelper.PaddingType[6] {
            BlockCipherHelper.PaddingType.None, BlockCipherHelper.PaddingType.Zeros, BlockCipherHelper.PaddingType.PKCS7,
            BlockCipherHelper.PaddingType.ANSIX923, BlockCipherHelper.PaddingType.ISO10126, BlockCipherHelper.PaddingType.OneZeros
        };

        [ContextMenu("ActionCaption", "ActionTooltip", 1, ContextMenuControlType.ComboBox, new int[] { 1, 2 }, new string[] { "ActionList1", "ActionList2" })]
        [TaskPane("ActionCaption", "ActionTooltip", null, 1, false, ControlType.ComboBox, new string[] { "ActionList1", "ActionList2" })]
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

        [ContextMenu("ModeCaption", "ModeTooltip", 2, ContextMenuControlType.ComboBox, null, new string[] { "ModeList1", "ModeList2", "ModeList3", "ModeList4" })]
        [TaskPane("ModeCaption", "ModeTooltip", null, 2, false, ControlType.ComboBox, new string[] { "ModeList1", "ModeList2", "ModeList3", "ModeList4" })]
        public int Mode
        {
            get => mode;
            set
            {
                if (value != mode)
                {
                    mode = value;
                    OnPropertyChanged("Mode");
                }
            }
        }

        [ContextMenu("PaddingCaption", "PaddingTooltip", 3, ContextMenuControlType.ComboBox, null, new string[] { "PaddingList1", "PaddingList2", "PaddingList3", "PaddingList4", "PaddingList5", "PaddingList6" })]
        [TaskPane("PaddingCaption", "PaddingTooltip", null, 3, false, ControlType.ComboBox, new string[] { "PaddingList1", "PaddingList2", "PaddingList3", "PaddingList4", "PaddingList5", "PaddingList6" })]
        public int Padding
        {
            get => padding;
            set
            {
                if (value != padding)
                {
                    padding = value;
                    OnPropertyChanged("Padding");
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
