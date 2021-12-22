/*
   Copyright 2021 Nils Kopal, CrypTool project

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
using System.ComponentModel;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;

namespace CrypTool.PBKDF
{
    public enum KeyDerivationFunctions
    {
        PBKDF1,
        PBKDF2
    }

    public enum HashAlgorithms
    {
        SHA1,
        SHA256,
        SHA384,
        SHA512
    }

    public class PBKDFSettings : ISettings
    {
        private KeyDerivationFunctions _keyDerivationFunction = KeyDerivationFunctions.PBKDF1;
        private HashAlgorithms _hashAlgorithm = HashAlgorithms.SHA256;
        private int _iterations = 1;
        private int _outputLength = 32;
        public event PropertyChangedEventHandler PropertyChanged;

        public void Initialize()
        {

        }

        [TaskPane("KDFCaption", "KDFTooltip", null, 0, false, ControlType.ComboBox, new string[] { "PBKDF1", "PBKDF2" })]
        public KeyDerivationFunctions KeyDerivationFunction
        {
            get => _keyDerivationFunction;
            set
            {
                if (_keyDerivationFunction != value)
                {
                    _keyDerivationFunction = value;
                    OnPropertyChanged("KeyDerivationFunction");
                }
            }
        }

        [TaskPane("HashAlgorithmCaption", "HashAlgorithmTooltip", null, 1, false, ControlType.ComboBox, new string[] { "SHA1", "SHA256", "SHA384", "SHA512" })]
        public HashAlgorithms HashAlgorithm
        {
            get => _hashAlgorithm;
            set
            {
                if (_hashAlgorithm != value)
                {
                    _hashAlgorithm = value;
                    OnPropertyChanged("HashAlgorithm");

                    if (_keyDerivationFunction == KeyDerivationFunctions.PBKDF1)
                    {
                        switch (value)
                        {
                            case HashAlgorithms.SHA1:
                                if (_outputLength < 20)
                                {
                                    return;
                                }
                                _outputLength = 20;
                                break;
                            case HashAlgorithms.SHA256:
                                if (_outputLength < 32)
                                {
                                    return;
                                }
                                _outputLength = 32;
                                break;
                            case HashAlgorithms.SHA384:
                                if (_outputLength < 48)
                                {
                                    return;
                                }
                                _outputLength = 48;
                                break;
                            case HashAlgorithms.SHA512:
                                if (_outputLength < 64)
                                {
                                    return;
                                }
                                _outputLength = 64;
                                break;
                        }
                        OnPropertyChanged("OutputLength");
                    }
                }
            }
        }

        [TaskPane("IterationsCaption", "IterationsTooltip", null, 2, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, int.MaxValue)]
        public int Iterations
        {
            get => _iterations;
            set
            {
                if (_iterations != value)
                {
                    _iterations = value;
                    OnPropertyChanged("HashAlgorithm");
                }
            }
        }

        [TaskPane("OutputLengthCaption", "OutputLengthTooltip", null, 3, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, int.MaxValue)]
        public int OutputLength
        {
            get => _outputLength;
            set
            {
                if (_outputLength != value)
                {
                    _outputLength = value;
                    OnPropertyChanged("OutputLength");
                }
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }
    }
}
