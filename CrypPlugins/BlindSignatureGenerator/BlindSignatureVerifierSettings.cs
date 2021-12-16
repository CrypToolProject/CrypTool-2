/*
   Copyright 2019 Axel Wehage

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

namespace CrypTool.Plugins.BlindSignatureGenerator
{
    public class BlindSignatureVerifierSettings : ISettings
    {

        #region Public Variables
        public enum SigningMode { RSA = 0, Paillier = 1 };
        #endregion

        #region Private Variables
        private SigningMode selectedSigningMode = SigningMode.RSA;
        #endregion

        #region TaskPane Settings

        public BlindSignatureVerifierSettings()
        {
        }

        [TaskPane("SigningAlgorithm", "SigningAlgorithmTooltip", null, 1, false, ControlType.ComboBox, new string[] { "RSA", "Paillier" })]
        public SigningMode SigningAlgorithm
        {
            get => selectedSigningMode;
            set
            {
                if (value != selectedSigningMode)
                {
                    selectedSigningMode = value;
                    OnPropertyChanged("SigningAlgorithm");
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