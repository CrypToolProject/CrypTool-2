/*
   Copyright 2022 Nils Kopal, CrypTool project

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
using static CrypTool.PluginBase.Miscellaneous.BlockCipherHelper;

namespace CrypTool.Plugins.Magma
{
    public enum SBoxes
    {
        GOST_R_34_12_2015N = 0,
        CENTRAL_BANK_OF_RUSSIAN_FEDERATION = 1
    }

    public class MagmaSettings : ISettings
    {
        private CipherAction _action = CipherAction.Encrypt;
        private SBoxes _sboxes = SBoxes.GOST_R_34_12_2015N;
        private BlockMode _blockMode = BlockMode.ECB;
        private PaddingType _padding = PaddingType.None;
        
        [TaskPane("ActionCaption", "ActionTooltip", null, 0, false, ControlType.ComboBox, new string[] { "ActionList1", "ActionList2" })]
        public CipherAction Action
        {
            get => _action;
            set
            {
                if (_action != value)
                {
                    _action = value;
                    OnPropertyChanged(nameof(Action));
                }
            }
        }

        [TaskPane("SBoxesCaption", "SBoxesTooltip", null, 1, false, ControlType.ComboBox, new string[] { "GOST_R_34_12_2015N", "CENTRAL_BANK_OF_RUSSIAN_FEDERATION" })]
        public SBoxes SBoxes
        {
            get => _sboxes;
            set
            {
                if (_sboxes != value)
                {
                    _sboxes = value;
                    OnPropertyChanged(nameof(SBoxes));
                }
            }
        }

        [TaskPane("BlockModeCaption", "BlockModeTooltip", null, 2, false, ControlType.ComboBox, new string[] { "BlockModeList1", "BlockModeList2", "BlockModeList3", "BlockModeList4" })]
        public BlockMode BlockMode
        {
            get => _blockMode;
            set
            {
                if (_blockMode != value)
                {
                    _blockMode = value;
                    OnPropertyChanged(nameof(BlockMode));
                }
            }
        }

        [TaskPane("PaddingCaption", "PaddingTooltip", null, 3, false, ControlType.ComboBox, new string[] { "PaddingList1", "PaddingList2", "PaddingList3", "PaddingList4", "PaddingList5", "PaddingList6" })]
        public BlockCipherHelper.PaddingType Padding
        {
            get => _padding;
            set
            {
                if (_padding != value)
                {
                    _padding = value;
                    OnPropertyChanged(nameof(Padding));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        public void Initialize()
        {

        }
    }
}
