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

namespace DCAPathFinder.UI.Tutorial2
{
    public class DifferenceDistribution : INotifyPropertyChanged
    {
        private string _inVal;

        private string _zeroOutVal;
        private string _oneOutVal;
        private string _twoOutVal;
        private string _threeOutVal;

        private string _fourOutVal;
        private string _fiveOutVal;
        private string _sixOutVal;
        private string _sevenOutVal;

        private string _eightOutVal;
        private string _nineOutVal;
        private string _tenOutVal;
        private string _elevenOutVal;

        private string _twelveOutVal;
        private string _thirteenOutVal;
        private string _fourteenOutVal;
        private string _fifteenOutVal;

        /// <summary>
        /// Constructor
        /// </summary>
        public DifferenceDistribution()
        {
        }

        /// <summary>
        /// Property for _inVal
        /// </summary>
        public string InVal
        {
            get => _inVal;
            set
            {
                _inVal = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _zeroOutVal
        /// </summary>
        public string ZeroOutVal
        {
            get => _zeroOutVal;
            set
            {
                _zeroOutVal = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _oneOutVal
        /// </summary>
        public string OneOutVal
        {
            get => _oneOutVal;
            set
            {
                _oneOutVal = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _twoOutVal
        /// </summary>
        public string TwoOutVal
        {
            get => _twoOutVal;
            set
            {
                _twoOutVal = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _threeOutVal
        /// </summary>
        public string ThreeOutVal
        {
            get => _threeOutVal;
            set
            {
                _threeOutVal = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _fourOutVal
        /// </summary>
        public string FourOutVal
        {
            get => _fourOutVal;
            set
            {
                _fourOutVal = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _fiveOutVal
        /// </summary>
        public string FiveOutVal
        {
            get => _fiveOutVal;
            set
            {
                _fiveOutVal = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _sixOutVal
        /// </summary>
        public string SixOutVal
        {
            get => _sixOutVal;
            set
            {
                _sixOutVal = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _sevenOutVal
        /// </summary>
        public string SevenOutVal
        {
            get => _sevenOutVal;
            set
            {
                _sevenOutVal = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _eightOutVal
        /// </summary>
        public string EightOutVal
        {
            get => _eightOutVal;
            set
            {
                _eightOutVal = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _nineOutVal
        /// </summary>
        public string NineOutVal
        {
            get => _nineOutVal;
            set
            {
                _nineOutVal = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _tenOutVal
        /// </summary>
        public string TenOutVal
        {
            get => _tenOutVal;
            set
            {
                _tenOutVal = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _elevenOutVal
        /// </summary>
        public string ElevenOutVal
        {
            get => _elevenOutVal;
            set
            {
                _elevenOutVal = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _twelveOutVal
        /// </summary>
        public string TwelveOutVal
        {
            get => _twelveOutVal;
            set
            {
                _twelveOutVal = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _thirteenOutVal
        /// </summary>
        public string ThirteenOutVal
        {
            get => _thirteenOutVal;
            set
            {
                _thirteenOutVal = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _fourteenOutVal
        /// </summary>
        public string FourteenOutVal
        {
            get => _fourteenOutVal;
            set
            {
                _fourteenOutVal = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _fifteenOutVal
        /// </summary>
        public string FifteenOutVal
        {
            get => _fifteenOutVal;
            set
            {
                _fifteenOutVal = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Method to call if data changes
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}