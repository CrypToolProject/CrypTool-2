/*
   Copyright 2019 Christian Bender christian1.bender@student.uni-siegen.de

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
using System.Runtime.CompilerServices;

namespace DCAPathVisualiser.UI.Models
{
    public class Cipher2CharacteristicUI : INotifyPropertyChanged
    {
        private string _inputDiff;
        private string _inputDiffR1;
        private string _outputDiffR1;
        private string _inputDiffR2;
        private string _outputDiffR2;
        private string _expectedDiff;

        private int _inputDiffInt;
        private int _inputDiffR1Int;
        private int _outputDiffR1Int;
        private int _inputDiffR2Int;
        private int _outputDiffR2Int;
        private int _expectedDiffInt;

        /// <summary>
        /// Constructor
        /// </summary>
        public Cipher2CharacteristicUI()
        {

        }

        /// <summary>
        /// Property for _inputDiffInt
        /// </summary>
        public int InputDiffInt
        {
            get => _inputDiffInt;
            set
            {
                _inputDiffInt = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _inputDiffR1Int
        /// </summary>
        public int InputDiffR1Int
        {
            get => _inputDiffR1Int;
            set
            {
                _inputDiffR1Int = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _outputDiffR1Int
        /// </summary>
        public int OutputDiffR1Int
        {
            get => _outputDiffR1Int;
            set
            {
                _outputDiffR1Int = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _inputDiffR2Int
        /// </summary>
        public int InputDiffR2Int
        {
            get => _inputDiffR2Int;
            set
            {
                _inputDiffR2Int = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _outputDiffR2Int
        /// </summary>
        public int OutputDiffR2Int
        {
            get => _outputDiffR2Int;
            set
            {
                _outputDiffR2Int = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _expectedDiffInt
        /// </summary>
        public int ExpectedDiffInt
        {
            get => _expectedDiffInt;
            set
            {
                _expectedDiffInt = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _inputDiff
        /// </summary>
        public string InputDiff
        {
            get => _inputDiff;
            set
            {
                _inputDiff = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _inputDiffR1
        /// </summary>
        public string InputDiffR1
        {
            get => _inputDiffR1;
            set
            {
                _inputDiffR1 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _outputDiffR1
        /// </summary>
        public string OutputDiffR1
        {
            get => _outputDiffR1;
            set
            {
                _outputDiffR1 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _inputDiffR2
        /// </summary>
        public string InputDiffR2
        {
            get => _inputDiffR2;
            set
            {
                _inputDiffR2 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _outputDiffR2
        /// </summary>
        public string OutputDiffR2
        {
            get => _outputDiffR2;
            set
            {
                _outputDiffR2 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for 
        /// </summary>
        public string ExpectedDiff
        {
            get => _expectedDiff;
            set
            {
                _expectedDiff = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// OnPropertyChanged-method for INotifyPropertyChanged
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
