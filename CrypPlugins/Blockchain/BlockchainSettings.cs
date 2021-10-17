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

        private int mining_difficulty = 1;
        private double miningReward = 1;
        private string miningAddress = "";
        private HashAlgorithms hashAlgorithm = HashAlgorithms.SHA256;

        #endregion

        #region TaskPane Settings


        [TaskPane("Mining difficulty", "Change Mining Difficulty", null, 0, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, Int32.MaxValue)]
        public int MiningDifficulty
        {
            get
            {
                return mining_difficulty;
            }
            set
            {
                if (mining_difficulty != value)
                {
                    mining_difficulty = value;
                    OnPropertyChanged("MiningDifficulty");
                }
            }
        }

        [TaskPane("Mining reward (in coins)", "Change the mining reward", null, 0, false, ControlType.TextBox)]
        public double MiningReward
        {
            get
            {
                return miningReward;
            }
            set
            {
                if (miningReward != value)
                {
                    miningReward = value;
                    OnPropertyChanged("MiningReward");
                }
            }
        }

        [TaskPane("Mining address", "Change the mining address",null,0,false,ControlType.TextBox)]
        public string Mining_Address
        {
            get
            {
                return miningAddress;
            }
            set
            {
                if (miningAddress != value)
                {
                    miningAddress = value;
                    OnPropertyChanged("Mining_Address");
                }
            }
        }

        [TaskPane("Hash algorithm", "Change the hash algorithm", null, 0, false, ControlType.ComboBox, new string[] {"SHA1","SHA256","SHA512" })]
        public HashAlgorithms Hash_Algorithm
        {
            get
            {
                return hashAlgorithm;
            }
            set
            {
                if (hashAlgorithm != value)
                {
                    hashAlgorithm = value;
                    OnPropertyChanged("Hash_Algorithm");
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
