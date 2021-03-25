/*
   Copyright 1995 - 2010 Jörg Drobick
   Copyright 2010 Matthäus Wander, University of Duisburg-Essen

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

namespace CrypTool.Plugins.T310
{
    public enum ModeEnum
    {
        Encrypt,
        Decrypt
    }

    public enum VersionEnum
    {
        Version50,
        Version51
    }

    public enum BitSelectorEnum
    {
        High,
        Low
    }

    public enum LongTermKeyEnum
    {
        LZS14,
        LZS15,
        LZS16,
        LZS17,
        LZS21,
        LZS26,
        LZS29,
        LZS30,
        LZS31,
        LZS32,
        LZS33
    }

    public enum KeyIndex
    {
        S1,
        S2
    }

    public class T310Settings : ISettings
    {
        #region Private Variables

        private ModeEnum mode = ModeEnum.Encrypt;
        private VersionEnum version = VersionEnum.Version51;
        private BitSelectorEnum selector = BitSelectorEnum.High;
        private LongTermKeyEnum key = LongTermKeyEnum.LZS30;

        #endregion

        #region TaskPane Settings

        [TaskPane("ModeCaption", "ModeTooltip", null, 1, false, ControlType.ComboBox, new string[] { "ModeList1", "ModeList2" })]
        public ModeEnum Mode
        {
            get
            {
                return mode;
            }
            set
            {
                if (mode != value)
                {
                    mode = value;
                    OnPropertyChanged("Mode");
                }
            }
        }

        [TaskPane("VersionCaption", "VersionTooltip", null, 1, false, ControlType.ComboBox, new string[] { "VersionList1", "VersionList2" })]
        public VersionEnum Version
        {
            get
            {
                return version;
            }
            set
            {
                if (version != value)
                {
                    version = value;
                    OnPropertyChanged("Version");
                }
            }
        }

        [TaskPane("BitSelectorCaption", "BitSelectorTooltip", null, 1, false, ControlType.ComboBox, new string[] { "BitSelectorList1", "BitSelectorList2" })]
        public BitSelectorEnum Selector
        {
            get
            {
                return selector;
            }
            set
            {
                if (selector != value)
                {
                    selector = value;
                    OnPropertyChanged("Selector");
                }
            }
        }


        [TaskPane("LongTermKeyCaption", "LongTermKeyTooltip", null, 1, false, ControlType.ComboBox, 
            new string[] { "Key14", "Key15", "Key16", "Key17", "Key21", "Key26", "Key29", "Key30", "Key31", "Key32", "Key33" })]
        public LongTermKeyEnum Key
        {
            get
            {
                return key;
            }
            set
            {
                if (key != value)
                {
                    key = value;
                    OnPropertyChanged("Key");
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {
            
        }

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(name));

        }

        #endregion
    }
}
