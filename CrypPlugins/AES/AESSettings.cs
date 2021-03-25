/*
   Copyright 2008-2012 Arno Wacker, University of Kassel

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
using System.Windows;
using System.ComponentModel;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;

namespace CrypTool.Plugins.Cryptography.Encryption
{
    public class AESSettings : ISettings
    {
        private int action = 0; // 0=encrypt, 1=decrypt
        private int cryptoAlgorithm = 0; // 0=AES, 1=Rijndael
        private int blocksize = 0; // 0=128, 1=192, 2=256
        private int keysize = 0; // 0=128, 1=192, 2=256
        private int mode = 0; // 0="ECB", 1="CBC", 2="CFB", 3="OFB"
        private int padding = 1; // 0="None", 1="Zeros"=default, 2="PKCS7" , 3="ANSIX923", 4="ISO10126", 5=1-0-Padding

        public BlockCipherHelper.PaddingType[] padmap = new BlockCipherHelper.PaddingType[6] { 
            BlockCipherHelper.PaddingType.None, BlockCipherHelper.PaddingType.Zeros, BlockCipherHelper.PaddingType.PKCS7,
            BlockCipherHelper.PaddingType.ANSIX923, BlockCipherHelper.PaddingType.ISO10126, BlockCipherHelper.PaddingType.OneZeros
        };

        [ContextMenu("CryptoAlgorithmCaption", "CryptoAlgorithmTooltip", 1, ContextMenuControlType.ComboBox, null, "CryptoAlgorithmList1", "CryptoAlgorithmList2")]
        [TaskPane("CryptoAlgorithmCaption", "CryptoAlgorithmTooltip", null, 0, false, ControlType.ComboBox, new string[] { "CryptoAlgorithmList1", "CryptoAlgorithmList2" })]
        public int CryptoAlgorithm
        {
            get { return this.cryptoAlgorithm; }
            set
            {
                if (((int)value) != cryptoAlgorithm)
                {
                    this.cryptoAlgorithm = (int)value;
                    if (cryptoAlgorithm == 0)
                    {
                        blocksize = 0;
                        OnPropertyChanged("Blocksize");
                    }
                    OnPropertyChanged("CryptoAlgorithm");

                    switch (cryptoAlgorithm)
                    {
                        case 0:
                            ChangePluginIcon(0);
                            break;
                        case 1:
                            ChangePluginIcon(3);
                            break;
                        default:
                            break;
                    }

                    UpdateTaskPaneVisibility();
                }
            }
        }

        [ContextMenu("ActionCaption", "ActionTooltip", 2, ContextMenuControlType.ComboBox, new int[] { 1, 2 }, "ActionList1", "ActionList2")]
        [TaskPane("ActionCaption", "ActionTooltip", null, 2, false, ControlType.ComboBox, new string[] { "ActionList1", "ActionList2" })]
        public int Action
        {
            get { return this.action; }
            set
            {
                if (((int)value) != action)
                {
                    this.action = (int)value;
                    OnPropertyChanged("Action");
                }
            }
        }


        [ContextMenu("KeysizeCaption", "KeysizeTooltip", 3, ContextMenuControlType.ComboBox, null, "KeysizeList1", "KeysizeList2", "KeysizeList3")]
        [TaskPane("KeysizeCaption", "KeysizeTooltip", null, 3, false, ControlType.ComboBox, new String[] { "KeysizeList1", "KeysizeList2", "KeysizeList3" })]
        public int Keysize
        {
            get { return this.keysize; }
            set
            {
                if (((int)value) != keysize)
                {
                    this.keysize = (int)value;
                    OnPropertyChanged("Keysize");
                }
            }
        }

        public int KeysizeAsBytes
        {
            get
            {
                switch (this.keysize)
                {
                    case 0:
                        return 16;
                    case 1:
                        return 24;
                    case 2:
                        return 32;
                    default:
                        throw new InvalidOperationException("Selected keysize entry unknown: " + this.keysize);
                }
            }
        }

        public int KeysizeAsBits
        {
            get
            {
                return KeysizeAsBytes * 8;
            }
        }

        [ContextMenu("BlocksizeCaption", "BlocksizeTooltip", 4, ContextMenuControlType.ComboBox, null, "BlocksizeList1", "BlocksizeList2", "BlocksizeList3")]
        [TaskPane("BlocksizeCaption", "BlocksizeTooltip", null, 4, false, ControlType.ComboBox, new String[] { "BlocksizeList1", "BlocksizeList2", "BlocksizeList3" })]
        public int Blocksize
        {
            get { return this.blocksize; }
            set
            {
                if (((int)value) != blocksize)
                {
                    this.blocksize = (int)value;
                    if (blocksize > 0)
                    {
                        cryptoAlgorithm = 1;
                        OnPropertyChanged("CryptoAlgorithm");
                    }
                    OnPropertyChanged("Blocksize");
                }
            }
        }

        public int BlocksizeAsBytes
        {
            get
            {
                switch (this.cryptoAlgorithm)
                {
                    case 0:
                        return 16;
                    case 1:
                        switch (this.blocksize)
                        {
                            case 0:
                                return 16;
                            case 1:
                                return 24;
                            case 2:
                                return 32;
                            default:
                                throw new InvalidOperationException("Selected blocksize entry unknown: " + this.blocksize);
                        }
                    default:
                        throw new InvalidOperationException("Selected algorithm entry unknown: " + this.cryptoAlgorithm);
                }
            }
        }

        [ContextMenu("ModeCaption", "ModeTooltip", 5, ContextMenuControlType.ComboBox, null, new String[] { "ModeList1", "ModeList2", "ModeList3", "ModeList4", "ModeList5" })]
        [TaskPane("ModeCaption", "ModeTooltip", null, 5, false, ControlType.ComboBox, new String[] { "ModeList1", "ModeList2", "ModeList3", "ModeList4", "ModeList5" })]
        public int Mode
        {
            get { return this.mode; }
            set
            {
                if (((int)value) != mode)
                {
                    this.mode = (int)value;
                    OnPropertyChanged("Mode");
                }
            }
        }

        [ContextMenu("PaddingCaption", "PaddingTooltip", 6, ContextMenuControlType.ComboBox, null, "PaddingList1", "PaddingList2", "PaddingList3", "PaddingList4", "PaddingList5", "PaddingList6")]
        [TaskPane("PaddingCaption", "PaddingTooltip", null, 6, false, ControlType.ComboBox, new String[] { "PaddingList1", "PaddingList2", "PaddingList3", "PaddingList4", "PaddingList5", "PaddingList6" })]
        public int Padding
        {
            get { return this.padding; }
            set
            {
                if (((int)value) != padding)
                {
                    this.padding = (int)value;
                    OnPropertyChanged("Padding");
                }
            }
        }

        #region events

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        #endregion

        internal void UpdateTaskPaneVisibility()
        {
            if (TaskPaneAttributeChanged == null)
                return;

            switch (CryptoAlgorithm)
            {
                case 0: // AES
                    TaskPaneAttribteContainer tba = new TaskPaneAttribteContainer("Blocksize", Visibility.Collapsed);
                    TaskPaneAttributeChangedEventArgs tbac = new TaskPaneAttributeChangedEventArgs(tba);
                    TaskPaneAttributeChanged(this, tbac);
                    break;
                case 1: // Rijndael
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Blocksize", Visibility.Visible)));
                    break;
            }
        }

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {
            UpdateTaskPaneVisibility();
        }

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

        public event StatusChangedEventHandler OnPluginStatusChanged;

        private void ChangePluginIcon(int Icon)
        {
            if (OnPluginStatusChanged != null) OnPluginStatusChanged(null, new StatusEventArgs(StatusChangedMode.ImageUpdate, Icon));
        }
    }
}