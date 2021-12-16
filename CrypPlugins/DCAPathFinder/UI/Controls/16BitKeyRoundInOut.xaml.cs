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
using System.Windows.Controls;

namespace DCAPathFinder.UI.Controls
{
    /// <summary>
    /// Interaktionslogik für _16BitKeyRound.xaml
    /// </summary>
    public partial class _16BitKeyRoundInOut : UserControl, INotifyPropertyChanged
    {
        private bool[] _coloredBits;
        private readonly string _activeColor = "Red";
        private readonly string _inActiveColor = "Black";
        private readonly int _activeThickness = 4;
        private readonly int _inActiveThickness = 2;

        /// <summary>
        /// Constructor
        /// </summary>
        public _16BitKeyRoundInOut()
        {
            _coloredBits = new bool[16];
            DataContext = this;
            InitializeComponent();
        }

        /// <summary>
        /// Property for _coloredBits
        /// </summary>
        public bool[] ColoredBits
        {
            get => _coloredBits;
            set
            {
                _coloredBits = value;
                OnPropertyChanged();

                //call all input bits
                OnPropertyChanged("InputBitZeroColor");
                OnPropertyChanged("InputBitOneColor");
                OnPropertyChanged("InputBitTwoColor");
                OnPropertyChanged("InputBitThreeColor");
                OnPropertyChanged("InputBitFourColor");
                OnPropertyChanged("InputBitFiveColor");
                OnPropertyChanged("InputBitSixColor");
                OnPropertyChanged("InputBitSevenColor");
                OnPropertyChanged("InputBitEightColor");
                OnPropertyChanged("InputBitNineColor");
                OnPropertyChanged("InputBitTenColor");
                OnPropertyChanged("InputBitElevenColor");
                OnPropertyChanged("InputBitTwelveColor");
                OnPropertyChanged("InputBitThirteenColor");
                OnPropertyChanged("InputBitFourteenColor");
                OnPropertyChanged("InputBitFifteenColor");

                //call all output bits
                OnPropertyChanged("OutputBitZeroColor");
                OnPropertyChanged("OutputBitOneColor");
                OnPropertyChanged("OutputBitTwoColor");
                OnPropertyChanged("OutputBitThreeColor");
                OnPropertyChanged("OutputBitFourColor");
                OnPropertyChanged("OutputBitFiveColor");
                OnPropertyChanged("OutputBitSixColor");
                OnPropertyChanged("OutputBitSevenColor");
                OnPropertyChanged("OutputBitEightColor");
                OnPropertyChanged("OutputBitNineColor");
                OnPropertyChanged("OutputBitTenColor");
                OnPropertyChanged("OutputBitElevenColor");
                OnPropertyChanged("OutputBitTwelveColor");
                OnPropertyChanged("OutputBitThirteenColor");
                OnPropertyChanged("OutputBitFourteenColor");
                OnPropertyChanged("OutputBitFifteenColor");

                //Thickness
                OnPropertyChanged("InputBitZeroThickness");
                OnPropertyChanged("InputBitOneThickness");
                OnPropertyChanged("InputBitTwoThickness");
                OnPropertyChanged("InputBitThreeThickness");
                OnPropertyChanged("InputBitFourThickness");
                OnPropertyChanged("InputBitFiveThickness");
                OnPropertyChanged("InputBitSixThickness");
                OnPropertyChanged("InputBitSevenThickness");
                OnPropertyChanged("InputBitEightThickness");
                OnPropertyChanged("InputBitNineThickness");
                OnPropertyChanged("InputBitTenThickness");
                OnPropertyChanged("InputBitElevenThickness");
                OnPropertyChanged("InputBitTwelveThickness");
                OnPropertyChanged("InputBitThirteenThickness");
                OnPropertyChanged("InputBitFourteenThickness");
                OnPropertyChanged("InputBitFifteenThickness");

                OnPropertyChanged("OutputBitZeroThickness");
                OnPropertyChanged("OutputBitOneThickness");
                OnPropertyChanged("OutputBitTwoThickness");
                OnPropertyChanged("OutputBitThreeThickness");
                OnPropertyChanged("OutputBitFourThickness");
                OnPropertyChanged("OutputBitFiveThickness");
                OnPropertyChanged("OutputBitSixThickness");
                OnPropertyChanged("OutputBitSevenThickness");
                OnPropertyChanged("OutputBitEightThickness");
                OnPropertyChanged("OutputBitNineThickness");
                OnPropertyChanged("OutputBitTenThickness");
                OnPropertyChanged("OutputBitElevenThickness");
                OnPropertyChanged("OutputBitTwelveThickness");
                OnPropertyChanged("OutputBitThirteenThickness");
                OnPropertyChanged("OutputBitFourteenThickness");
                OnPropertyChanged("OutputBitFifteenThickness");
            }
        }

        #region thickness

        /// <summary>
        /// Property for InputBitZeroThickness
        /// </summary>
        public int InputBitZeroThickness
        {
            get
            {
                if (ColoredBits[0])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for InputBitOneThickness
        /// </summary>
        public int InputBitOneThickness
        {
            get
            {
                if (ColoredBits[1])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for InputBitTwoThickness
        /// </summary>
        public int InputBitTwoThickness
        {
            get
            {
                if (ColoredBits[2])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for InputBitThreeThickness
        /// </summary>
        public int InputBitThreeThickness
        {
            get
            {
                if (ColoredBits[3])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for InputBitFourThickness
        /// </summary>
        public int InputBitFourThickness
        {
            get
            {
                if (ColoredBits[4])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for InputBitFiveThickness
        /// </summary>
        public int InputBitFiveThickness
        {
            get
            {
                if (ColoredBits[5])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for InputBitSixThickness
        /// </summary>
        public int InputBitSixThickness
        {
            get
            {
                if (ColoredBits[6])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for InputBitSevenThickness
        /// </summary>
        public int InputBitSevenThickness
        {
            get
            {
                if (ColoredBits[7])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for InputBitEightThickness
        /// </summary>
        public int InputBitEightThickness
        {
            get
            {
                if (ColoredBits[8])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for InputBitNineThickness
        /// </summary>
        public int InputBitNineThickness
        {
            get
            {
                if (ColoredBits[9])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for InputBitTenThickness
        /// </summary>
        public int InputBitTenThickness
        {
            get
            {
                if (ColoredBits[10])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for InputBitElevenThickness
        /// </summary>
        public int InputBitElevenThickness
        {
            get
            {
                if (ColoredBits[11])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for InputBitTwelveThickness
        /// </summary>
        public int InputBitTwelveThickness
        {
            get
            {
                if (ColoredBits[12])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for InputBitThirteenThickness
        /// </summary>
        public int InputBitThirteenThickness
        {
            get
            {
                if (ColoredBits[13])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for InputBitFourteenThickness
        /// </summary>
        public int InputBitFourteenThickness
        {
            get
            {
                if (ColoredBits[14])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for InputBitFifteenThickness
        /// </summary>
        public int InputBitFifteenThickness
        {
            get
            {
                if (ColoredBits[15])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for OutputBitZeroThickness
        /// </summary>
        public int OutputBitZeroThickness
        {
            get
            {
                if (ColoredBits[0])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for OutputBitOneThickness
        /// </summary>
        public int OutputBitOneThickness
        {
            get
            {
                if (ColoredBits[1])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for OutputBitTwoThickness
        /// </summary>
        public int OutputBitTwoThickness
        {
            get
            {
                if (ColoredBits[2])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for OutputBitThreeThickness
        /// </summary>
        public int OutputBitThreeThickness
        {
            get
            {
                if (ColoredBits[3])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for OutputBitFourThickness
        /// </summary>
        public int OutputBitFourThickness
        {
            get
            {
                if (ColoredBits[4])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for OutputBitFiveThickness
        /// </summary>
        public int OutputBitFiveThickness
        {
            get
            {
                if (ColoredBits[5])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for OutputBitSixThickness
        /// </summary>
        public int OutputBitSixThickness
        {
            get
            {
                if (ColoredBits[6])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for OutputBitSevenThickness
        /// </summary>
        public int OutputBitSevenThickness
        {
            get
            {
                if (ColoredBits[7])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for OutputBitEightThickness
        /// </summary>
        public int OutputBitEightThickness
        {
            get
            {
                if (ColoredBits[8])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for OutputBitNineThickness
        /// </summary>
        public int OutputBitNineThickness
        {
            get
            {
                if (ColoredBits[9])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for OutputBitTenThickness
        /// </summary>
        public int OutputBitTenThickness
        {
            get
            {
                if (ColoredBits[10])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for OutputBitElevenThickness
        /// </summary>
        public int OutputBitElevenThickness
        {
            get
            {
                if (ColoredBits[11])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for OutputBitTwelveThickness
        /// </summary>
        public int OutputBitTwelveThickness
        {
            get
            {
                if (ColoredBits[12])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for OutputBitThirteenThickness
        /// </summary>
        public int OutputBitThirteenThickness
        {
            get
            {
                if (ColoredBits[13])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for OutputBitFourteenThickness
        /// </summary>
        public int OutputBitFourteenThickness
        {
            get
            {
                if (ColoredBits[14])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for OutputBitFifteenThickness
        /// </summary>
        public int OutputBitFifteenThickness
        {
            get
            {
                if (ColoredBits[15])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        #endregion

        #region inputbits

        /// <summary>
        /// Property for InputBitZeroColor
        /// </summary>
        public string InputBitZeroColor
        {
            get
            {
                if (ColoredBits[0])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for InputBitOneColor
        /// </summary>
        public string InputBitOneColor
        {
            get
            {
                if (ColoredBits[1])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for InputBitTwoColor
        /// </summary>
        public string InputBitTwoColor
        {
            get
            {
                if (ColoredBits[2])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for InputBitThreeColor
        /// </summary>
        public string InputBitThreeColor
        {
            get
            {
                if (ColoredBits[3])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for InputBitFourColor
        /// </summary>
        public string InputBitFourColor
        {
            get
            {
                if (ColoredBits[4])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for InputBitFiveColor
        /// </summary>
        public string InputBitFiveColor
        {
            get
            {
                if (ColoredBits[5])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for InputBitSixColor
        /// </summary>
        public string InputBitSixColor
        {
            get
            {
                if (ColoredBits[6])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for InputBitSevenColor
        /// </summary>
        public string InputBitSevenColor
        {
            get
            {
                if (ColoredBits[7])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for InputBitEightColor
        /// </summary>
        public string InputBitEightColor
        {
            get
            {
                if (ColoredBits[8])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for InputBitNineColor
        /// </summary>
        public string InputBitNineColor
        {
            get
            {
                if (ColoredBits[9])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for InputBitTenColor
        /// </summary>
        public string InputBitTenColor
        {
            get
            {
                if (ColoredBits[10])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for InputBitSevenColor
        /// </summary>
        public string InputBitElevenColor
        {
            get
            {
                if (ColoredBits[11])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for InputBitTwelveColor
        /// </summary>
        public string InputBitTwelveColor
        {
            get
            {
                if (ColoredBits[12])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for InputBitThirteenColor
        /// </summary>
        public string InputBitThirteenColor
        {
            get
            {
                if (ColoredBits[13])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for InputBitFourteenColor
        /// </summary>
        public string InputBitFourteenColor
        {
            get
            {
                if (ColoredBits[14])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for InputBitSevenColor
        /// </summary>
        public string InputBitFifteenColor
        {
            get
            {
                if (ColoredBits[15])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        #endregion

        #region outputbits

        /// <summary>
        /// Property for OutputBitZeroColor
        /// </summary>
        public string OutputBitZeroColor
        {
            get
            {
                if (ColoredBits[0])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for OutputBitOneColor
        /// </summary>
        public string OutputBitOneColor
        {
            get
            {
                if (ColoredBits[1])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for OutputBitTwoColor
        /// </summary>
        public string OutputBitTwoColor
        {
            get
            {
                if (ColoredBits[2])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for OutputBitThreeColor
        /// </summary>
        public string OutputBitThreeColor
        {
            get
            {
                if (ColoredBits[3])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for OutputBitFourColor
        /// </summary>
        public string OutputBitFourColor
        {
            get
            {
                if (ColoredBits[4])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for OutputBitFiveColor
        /// </summary>
        public string OutputBitFiveColor
        {
            get
            {
                if (ColoredBits[5])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for OutputBitSixColor
        /// </summary>
        public string OutputBitSixColor
        {
            get
            {
                if (ColoredBits[6])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for OutputBitSevenColor
        /// </summary>
        public string OutputBitSevenColor
        {
            get
            {
                if (ColoredBits[7])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for OutputBitEightColor
        /// </summary>
        public string OutputBitEightColor
        {
            get
            {
                if (ColoredBits[8])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for OutputBitNineColor
        /// </summary>
        public string OutputBitNineColor
        {
            get
            {
                if (ColoredBits[9])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for OutputBitTenColor
        /// </summary>
        public string OutputBitTenColor
        {
            get
            {
                if (ColoredBits[10])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for OutputBitElevenColor
        /// </summary>
        public string OutputBitElevenColor
        {
            get
            {
                if (ColoredBits[11])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for OutputBitTwelveColor
        /// </summary>
        public string OutputBitTwelveColor
        {
            get
            {
                if (ColoredBits[12])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for OutputBitThirteenColor
        /// </summary>
        public string OutputBitThirteenColor
        {
            get
            {
                if (ColoredBits[13])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for OutputBitFourteenColor
        /// </summary>
        public string OutputBitFourteenColor
        {
            get
            {
                if (ColoredBits[14])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for OutputBitFifteenColor
        /// </summary>
        public string OutputBitFifteenColor
        {
            get
            {
                if (ColoredBits[15])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
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