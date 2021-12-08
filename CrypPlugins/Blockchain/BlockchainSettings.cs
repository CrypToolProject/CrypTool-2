/*
   Copyright CrypTool 2 Team <ct2contact@CrypTool.org>

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
using System.ComponentModel;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;

namespace CrypTool.Plugins.Blockchain
{
    public enum HashAlgorithms
    {
        SHA1,
        SHA256,
        SHA512
    }

    public class BlockchainSettings : ISettings
    {
        #region Private Variables
        
        private HashAlgorithms _hashAlgorithm = HashAlgorithms.SHA256;
        private int _hashAlgorithmWidth = 10;
        private int _miningDifficulty = 1;
        private double _miningReward = 1;
        private string _miningAddress = "";        

        #endregion

        #region TaskPane Settings


        [TaskPane("Mining difficulty", "Change Mining Difficulty", null, 0, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, int.MaxValue)]
        public int MiningDifficulty
        {
            get
            {
                return _miningDifficulty;
            }
            set
            {
                if (_miningDifficulty != value)
                {
                    _miningDifficulty = value;
                    OnPropertyChanged("MiningDifficulty");
                }
            }
        }

        [TaskPane("Mining reward (in coins)", "Change the mining reward", null, 0, false, ControlType.TextBox)]
        public double MiningReward
        {
            get
            {
                return _miningReward;
            }
            set
            {
                if (_miningReward != value)
                {
                    _miningReward = value;
                    OnPropertyChanged("MiningReward");
                }
            }
        }

        [TaskPane("Mining address", "Change the mining address",null,0,false,ControlType.TextBox)]
        public string MiningAddress
        {
            get
            {
                return _miningAddress;
            }
            set
            {
                if (_miningAddress != value)
                {
                    _miningAddress = value;
                    OnPropertyChanged("Mining_Address");
                }
            }
        }

        [TaskPane("Hash algorithm", "Change the hash algorithm", null, 0, false, ControlType.ComboBox, new string[] {"SHA1","SHA256","SHA512" })]
        public HashAlgorithms HashAlgorithm
        {
            get
            {
                return _hashAlgorithm;
            }
            set
            {
                if (_hashAlgorithm != value)
                {
                    _hashAlgorithm = value;
                    OnPropertyChanged("Hash_Algorithm");
                }
            }
        }

        [TaskPane("Hash algorithm width", "Change the hash algorithm width (number of used bytes)", null, 0, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, int.MaxValue)]
        public int HashAlgorithmWidth
        {
            get
            {
                return _hashAlgorithmWidth;
            }
            set
            {
                if (_hashAlgorithmWidth != value)
                {
                    _hashAlgorithmWidth = value;
                    OnPropertyChanged("Hash_AlgorithmWidth");
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
