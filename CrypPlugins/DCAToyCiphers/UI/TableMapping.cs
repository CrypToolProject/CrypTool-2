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

namespace DCAToyCiphers.UI
{
    /// <summary>
    /// represents a four bit SBox for the UI
    /// </summary>
    public class TableMapping : INotifyPropertyChanged
    {
        private string _direction;
        private int _zeroOutput;
        private int _oneOutput;
        private int _twoOutput;
        private int _threeOutput;
        private int _fourOutput;
        private int _fiveOutput;
        private int _sixOutput;
        private int _sevenOutput;
        private int _eightOutput;
        private int _nineOutput;
        private int _tenOutput;
        private int _elevenOutput;
        private int _twelveOutput;
        private int _thirteenOutput;
        private int _fourteenOutput;
        private int _fifteenOutput;

        /// <summary>
        /// Property for _direction
        /// </summary>
        public string Direction
        {
            get => _direction;
            set
            {
                _direction = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _zeroOutput
        /// </summary>
        public int ZeroOutput
        {
            get => _zeroOutput;
            set
            {
                _zeroOutput = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _oneOutput
        /// </summary>
        public int OneOutput
        {
            get => _oneOutput;
            set
            {
                _oneOutput = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _twoOutput
        /// </summary>
        public int TwoOutput
        {
            get => _twoOutput;
            set
            {
                _twoOutput = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _threeOutput
        /// </summary>
        public int ThreeOutput
        {
            get => _threeOutput;
            set
            {
                _threeOutput = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _fourOutput
        /// </summary>
        public int FourOutput
        {
            get => _fourOutput;
            set
            {
                _fourOutput = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _fiveOutput
        /// </summary>
        public int FiveOutput
        {
            get => _fiveOutput;
            set
            {
                _fiveOutput = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _sixOutput
        /// </summary>
        public int SixOutput
        {
            get => _sixOutput;
            set
            {
                _sixOutput = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _sevenOutput
        /// </summary>
        public int SevenOutput
        {
            get => _sevenOutput;
            set
            {
                _sevenOutput = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _eightOutput
        /// </summary>
        public int EightOutput
        {
            get => _eightOutput;
            set
            {
                _eightOutput = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _nineOutput
        /// </summary>
        public int NineOutput
        {
            get => _nineOutput;
            set
            {
                _nineOutput = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _tenOutput
        /// </summary>
        public int TenOutput
        {
            get => _tenOutput;
            set
            {
                _tenOutput = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _evelenOutput
        /// </summary>
        public int ElevenOutput
        {
            get => _elevenOutput;
            set
            {
                _elevenOutput = value;
                OnPropertyChanged();
            }
        }

        public int TwelveOutput
        {
            get => _twelveOutput;
            set
            {
                _twelveOutput = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _thirteenOutput
        /// </summary>
        public int ThirteenOutput
        {
            get => _thirteenOutput;
            set
            {
                _thirteenOutput = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _fourteenOutput
        /// </summary>
        public int FourteenOutput
        {
            get => _fourteenOutput;
            set
            {
                _fourteenOutput = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _fifteenOutput
        /// </summary>
        public int FifteenOutput
        {
            get => _fifteenOutput;
            set
            {
                _fifteenOutput = value;
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
