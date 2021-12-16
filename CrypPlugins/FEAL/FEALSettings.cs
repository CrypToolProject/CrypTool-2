/*
   Copyright 2019 Nils Kopal <Nils.Kopal<at>CrypTool.org

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

namespace CrypTool.Plugins.FEAL
{
    public enum FealAlgorithmType
    {
        FEAL4,
        FEAL8
    }

    public class FEALSettings : ISettings
    {
        #region Private Variables

        private FealAlgorithmType _fealAlgorithmType = FealAlgorithmType.FEAL8;
        private CipherAction _action = CipherAction.Encrypt;
        private BlockMode _blockMode = BlockMode.ECB;
        private BlockCipherHelper.PaddingType _padding = BlockCipherHelper.PaddingType.None;
        private bool _enableKeyParityBits = false;
        #endregion

        #region TaskPane Settings


        [TaskPane("AlgorithmTypeCaption", "AlgorithmTypeTooltip", null, 1, false, ControlType.ComboBox,
            new string[] { "FEAL4", "FEAL8" })]
        public FealAlgorithmType FealAlgorithmType
        {
            get => _fealAlgorithmType;
            set
            {
                if (_fealAlgorithmType != value)
                {
                    _fealAlgorithmType = value;
                    OnPropertyChanged("FealAlgorithmType");
                }
            }
        }

        [TaskPane("ActionCaption", "ActionTooltip", null, 2, false, ControlType.ComboBox,
            new string[] { "ActionList1", "ActionList2" })]
        public CipherAction Action
        {
            get => _action;
            set
            {
                if (_action != value)
                {
                    _action = value;
                    OnPropertyChanged("Action");
                }
            }
        }

        [TaskPane("EnableKeyParityBitsCaption", "EnableKeyParityBitsTooltip", null, 3, false, ControlType.CheckBox)]
        public bool EnableKeyParityBits
        {
            get => _enableKeyParityBits;
            set
            {
                if (_enableKeyParityBits != value)
                {
                    _enableKeyParityBits = value;
                    OnPropertyChanged("EnableKeyParityBits");
                }
            }
        }

        [TaskPane("BlockModeCaption", "BlockModeTooltip", null, 4, false, ControlType.ComboBox,
            new string[] { "BlockModeList1", "BlockModeList2", "BlockModeList3", "BlockModeList4" })]
        public BlockMode BlockMode
        {
            get => _blockMode;
            set
            {
                if (_blockMode != value)
                {
                    _blockMode = value;
                    OnPropertyChanged("BlockMode");
                }
            }
        }

        [TaskPane("PaddingCaption", "PaddingTooltip", null, 5, false, ControlType.ComboBox, new string[] { "PaddingList1", "PaddingList2", "PaddingList3", "PaddingList4", "PaddingList5", "PaddingList6" })]
        public BlockCipherHelper.PaddingType Padding
        {
            get => _padding;
            set
            {
                if (_padding != value)
                {
                    _padding = value;
                    OnPropertyChanged("Padding");
                }
            }
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion

        public void Initialize()
        {

        }
    }
}
