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
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System.ComponentModel;

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
        private string _miningAddress = string.Empty;

        #endregion

        #region TaskPane Settings


        [TaskPane("MiningDifficulty", "ChangeMiningDifficulty", null, 0, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, int.MaxValue)]
        public int MiningDifficulty
        {
            get => _miningDifficulty;
            set
            {
                if (_miningDifficulty != value)
                {
                    _miningDifficulty = value;
                    OnPropertyChanged("MiningDifficulty");
                }
            }
        }

        [TaskPane("MiningReward", "ChangeReward", null, 0, false, ControlType.TextBox)]
        public double MiningReward
        {
            get => _miningReward;
            set
            {
                if (_miningReward != value)
                {
                    _miningReward = value;
                    OnPropertyChanged("MiningReward");
                }
            }
        }

        [TaskPane("MiningAddress", "ChangeAddress", null, 0, false, ControlType.TextBox)]
        public string MiningAddress
        {
            get => _miningAddress;
            set
            {
                if (_miningAddress != value)
                {
                    _miningAddress = value;
                    OnPropertyChanged("MiningAddress");
                }
            }
        }

        [TaskPane("HashAlgo", "ChangeHashAlgo", null, 0, false, ControlType.ComboBox, new string[] { "SHA1", "SHA256", "SHA512" })]
        public HashAlgorithms HashAlgorithm
        {
            get => _hashAlgorithm;
            set
            {
                if (_hashAlgorithm != value)
                {
                    _hashAlgorithm = value;
                    OnPropertyChanged("HashAlgorithm");
                }
            }
        }

        [TaskPane("HashWidth", "ChangeWidth", null, 0, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, int.MaxValue)]
        public int HashAlgorithmWidth
        {
            get => _hashAlgorithmWidth;
            set
            {
                if (_hashAlgorithmWidth != value)
                {
                    _hashAlgorithmWidth = value;
                    OnPropertyChanged("HashAlgorithmWidth");
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
