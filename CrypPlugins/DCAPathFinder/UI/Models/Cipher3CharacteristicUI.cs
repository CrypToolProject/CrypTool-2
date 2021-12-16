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

using DCAPathFinder.UI.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DCAPathFinder.UI.Tutorial3
{
    public class Cipher3CharacteristicUI : CharacteristicUI, INotifyPropertyChanged
    {
        private string _inputDiff;
        private string _inputDiffR1;
        private string _outputDiffR1;
        private string _inputDiffR2;
        private string _outputDiffR2;
        private string _inputDiffR3;
        private string _outputDiffR3;
        private string _inputDiffR4;
        private string _outputDiffR4;
        private string _inputDiffR5;
        private string _outputDiffR5;
        private string _probability;

        private int _inputDiffInt;
        private int _inputDiffR1Int;
        private int _outputDiffR1Int;
        private int _inputDiffR2Int;
        private int _outputDiffR2Int;
        private int _inputDiffR3Int;
        private int _outputDiffR3Int;
        private int _inputDiffR4Int;
        private int _outputDiffR4Int;
        private int _inputDiffR5Int;
        private int _outputDiffR5Int;

        private string _colBackgroundColor;

        #region properties

        /// <summary>
        /// Property for the background color of the displaying row
        /// </summary>
        public string ColBackgroundColor
        {
            get => _colBackgroundColor;
            set
            {
                _colBackgroundColor = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _probability
        /// </summary>
        public string Probability
        {
            get => _probability;
            set
            {
                _probability = value;
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
        /// Property for _inputDiffR3
        /// </summary>
        public string InputDiffR3
        {
            get => _inputDiffR3;
            set
            {
                _inputDiffR3 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _outputDiffR3
        /// </summary>
        public string OutputDiffR3
        {
            get => _outputDiffR3;
            set
            {
                _outputDiffR3 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _inputDiffR4
        /// </summary>
        public string InputDiffR4
        {
            get => _inputDiffR4;
            set
            {
                _inputDiffR4 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _outputDiffR4
        /// </summary>
        public string OutputDiffR4
        {
            get => _outputDiffR4;
            set
            {
                _outputDiffR4 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _inputDiffR5
        /// </summary>
        public string InputDiffR5
        {
            get => _inputDiffR5;
            set
            {
                _inputDiffR5 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _outputDiffR5
        /// </summary>
        public string OutputDiffR5
        {
            get => _outputDiffR5;
            set
            {
                _outputDiffR5 = value;
                OnPropertyChanged();
            }
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
        /// Property for _inputDiffR3Int
        /// </summary>
        public int InputDiffR3Int
        {
            get => _inputDiffR3Int;

            set
            {
                _inputDiffR3Int = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _outputDiffR3Int
        /// </summary>
        public int OutputDiffR3Int
        {
            get => _outputDiffR3Int;
            set
            {
                _outputDiffR3Int = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _inputDiffR4Int
        /// </summary>
        public int InputDiffR4Int
        {
            get => _inputDiffR4Int;
            set
            {
                _inputDiffR4Int = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _outputDiffR4Int
        /// </summary>
        public int OutputDiffR4Int
        {
            get => _outputDiffR4Int;
            set
            {
                _outputDiffR4Int = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _inputDiffR5Int
        /// </summary>
        public int InputDiffR5Int
        {
            get => _inputDiffR5Int;
            set
            {
                _inputDiffR5Int = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _outputDiffR5Int
        /// </summary>
        public int OutputDiffR5Int
        {
            get => _outputDiffR5Int;
            set
            {
                _outputDiffR5Int = value;
                OnPropertyChanged();
            }
        }

        #endregion

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