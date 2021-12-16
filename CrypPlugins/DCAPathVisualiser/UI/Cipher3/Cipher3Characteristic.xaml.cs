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

namespace DCAPathVisualiser.UI.Cipher3
{
    /// <summary>
    /// Interaktionslogik für Cipher3Characteristic.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("DCAPathVisualiser.Properties.Resources")]
    public partial class Cipher3Characteristic : UserControl, INotifyPropertyChanged
    {
        private string _inputDiff;

        private bool[] _bitsKeyRoundOne;
        private bool[] _permutationBitsRoundOne;

        private bool[] _bitsKeyRoundTwo;
        private bool[] _permutationBitsRoundTwo;

        private bool[] _bitsKeyRoundThree;
        private bool[] _permutationBitsRoundThree;

        private bool[] _bitsKeyRoundFour;
        private bool[] _permutationBitsRoundFour;

        private bool[] _bitsKeyRoundFive;
        private bool[] _bitsKeyRoundSix;

        private string _round1InputDiff;
        private string _round1OutputDiff;

        private string _round2InputDiff;
        private string _round2OutputDiff;

        private string _round3InputDiff;
        private string _round3OutputDiff;

        private string _round4InputDiff;
        private string _round4OutputDiff;

        private string _round5InputDiff;

        private bool[] _activeSBoxes;

        private int _round;

        /// <summary>
        /// Constructor
        /// </summary>
        public Cipher3Characteristic()
        {
            _bitsKeyRoundOne = new bool[16];
            _permutationBitsRoundOne = new bool[16];

            _bitsKeyRoundTwo = new bool[16];
            _permutationBitsRoundTwo = new bool[16];

            _bitsKeyRoundThree = new bool[16];
            _permutationBitsRoundThree = new bool[16];

            _bitsKeyRoundFour = new bool[16];
            _permutationBitsRoundFour = new bool[16];

            _bitsKeyRoundFive = new bool[16];
            _bitsKeyRoundSix = new bool[16];

            _activeSBoxes = new bool[4];

            DataContext = this;
            InitializeComponent();
        }

        /// <summary>
        /// Renders the view
        /// </summary>
        private void RenderView()
        {
            KeyRound1.ColoredBits = BitsKeyRoundOne;
            PermutationRound1.ColoredBits = PermutationBitsRoundOne;

            KeyRound2.ColoredBits = BitsKeyRoundTwo;
            PermutationRound2.ColoredBits = PermutationBitsRoundTwo;

            KeyRound3.ColoredBits = BitsKeyRoundThree;
            PermutationRound3.ColoredBits = PermutationBitsRoundThree;

            KeyRound4.ColoredBits = BitsKeyRoundFour;
            PermutationRound4.ColoredBits = PermutationBitsRoundFour;

            KeyRound5.ColoredBits = BitsKeyRoundFive;
            KeyRound6.ColoredBits = BitsKeyRoundSix;

            //set active SBoxes: an active SBox is red colored
            switch (Round)
            {
                case 1:
                    {
                    }
                    break;
                case 2:
                    {
                        SBox1Round2.IsActive = ActiveSBoxes[0];
                        SBox2Round2.IsActive = ActiveSBoxes[1];
                        SBox3Round2.IsActive = ActiveSBoxes[2];
                        SBox4Round2.IsActive = ActiveSBoxes[3];

                        SBox1Round3.IsActive = false;
                        SBox2Round3.IsActive = false;
                        SBox3Round3.IsActive = false;
                        SBox4Round3.IsActive = false;

                        SBox1Round4.IsActive = false;
                        SBox2Round4.IsActive = false;
                        SBox3Round4.IsActive = false;
                        SBox4Round4.IsActive = false;

                        SBox1Round5.IsActive = false;
                        SBox2Round5.IsActive = false;
                        SBox3Round5.IsActive = false;
                        SBox4Round5.IsActive = false;
                    }
                    break;
                case 3:
                    {
                        SBox1Round2.IsActive = false;
                        SBox2Round2.IsActive = false;
                        SBox3Round2.IsActive = false;
                        SBox4Round2.IsActive = false;

                        SBox1Round3.IsActive = ActiveSBoxes[0];
                        SBox2Round3.IsActive = ActiveSBoxes[1];
                        SBox3Round3.IsActive = ActiveSBoxes[2];
                        SBox4Round3.IsActive = ActiveSBoxes[3];

                        SBox1Round4.IsActive = false;
                        SBox2Round4.IsActive = false;
                        SBox3Round4.IsActive = false;
                        SBox4Round4.IsActive = false;

                        SBox1Round5.IsActive = false;
                        SBox2Round5.IsActive = false;
                        SBox3Round5.IsActive = false;
                        SBox4Round5.IsActive = false;
                    }
                    break;
                case 4:
                    {
                        SBox1Round2.IsActive = false;
                        SBox2Round2.IsActive = false;
                        SBox3Round2.IsActive = false;
                        SBox4Round2.IsActive = false;

                        SBox1Round3.IsActive = false;
                        SBox2Round3.IsActive = false;
                        SBox3Round3.IsActive = false;
                        SBox4Round3.IsActive = false;

                        SBox1Round4.IsActive = ActiveSBoxes[0];
                        SBox2Round4.IsActive = ActiveSBoxes[1];
                        SBox3Round4.IsActive = ActiveSBoxes[2];
                        SBox4Round4.IsActive = ActiveSBoxes[3];

                        SBox1Round5.IsActive = false;
                        SBox2Round5.IsActive = false;
                        SBox3Round5.IsActive = false;
                        SBox4Round5.IsActive = false;
                    }
                    break;
                case 5:
                    {
                        SBox1Round2.IsActive = false;
                        SBox2Round2.IsActive = false;
                        SBox3Round2.IsActive = false;
                        SBox4Round2.IsActive = false;

                        SBox1Round3.IsActive = false;
                        SBox2Round3.IsActive = false;
                        SBox3Round3.IsActive = false;
                        SBox4Round3.IsActive = false;

                        SBox1Round4.IsActive = false;
                        SBox2Round4.IsActive = false;
                        SBox3Round4.IsActive = false;
                        SBox4Round4.IsActive = false;

                        SBox1Round5.IsActive = ActiveSBoxes[0];
                        SBox2Round5.IsActive = ActiveSBoxes[1];
                        SBox3Round5.IsActive = ActiveSBoxes[2];
                        SBox4Round5.IsActive = ActiveSBoxes[3];
                    }
                    break;
            }
        }

        /// <summary>
        /// Property for Round
        /// </summary>
        public int Round
        {
            get => _round;
            set
            {
                _round = value;
                OnPropertyChanged();
                RenderView();
            }
        }

        /// <summary>
        /// Property for ActiveSBoxes
        /// </summary>
        public bool[] ActiveSBoxes
        {
            get => _activeSBoxes;
            set
            {
                _activeSBoxes = value;
                OnPropertyChanged();
                RenderView();
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
        /// Property for _round1InputDiff
        /// </summary>
        public string Round1InputDiff
        {
            get => _round1InputDiff;
            set
            {
                _round1InputDiff = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _round1OutputDiff
        /// </summary>
        public string Round1OutputDiff
        {
            get => _round1OutputDiff;
            set
            {
                _round1OutputDiff = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _round2InputDiff
        /// </summary>
        public string Round2InputDiff
        {
            get => _round2InputDiff;
            set
            {
                _round2InputDiff = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _round2OutputDiff
        /// </summary>
        public string Round2OutputDiff
        {
            get => _round2OutputDiff;
            set
            {
                _round2OutputDiff = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _round3InputDiff
        /// </summary>
        public string Round3InputDiff
        {
            get => _round3InputDiff;
            set
            {
                _round3InputDiff = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _round3OutputDiff
        /// </summary>
        public string Round3OutputDiff
        {
            get => _round3OutputDiff;
            set
            {
                _round3OutputDiff = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _round4InputDiff
        /// </summary>
        public string Round4InputDiff
        {
            get => _round4InputDiff;
            set
            {
                _round4InputDiff = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _round4OutputDiff
        /// </summary>
        public string Round4OutputDiff
        {
            get => _round4OutputDiff;
            set
            {
                _round4OutputDiff = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _round5InputDiff
        /// </summary>
        public string Round5InputDiff
        {
            get => _round5InputDiff;
            set
            {
                _round5InputDiff = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _bitsKeyRoundOne
        /// </summary>
        public bool[] BitsKeyRoundOne
        {
            get => _bitsKeyRoundOne;
            set
            {
                _bitsKeyRoundOne = value;
                OnPropertyChanged();
                RenderView();
            }
        }

        /// <summary>
        /// Property for _permutationBitsRoundOne
        /// </summary>
        public bool[] PermutationBitsRoundOne
        {
            get => _permutationBitsRoundOne;
            set
            {
                _permutationBitsRoundOne = value;
                OnPropertyChanged();
                RenderView();
            }
        }

        /// <summary>
        /// Property for _bitsKeyRoundTwo
        /// </summary>
        public bool[] BitsKeyRoundTwo
        {
            get => _bitsKeyRoundTwo;
            set
            {
                _bitsKeyRoundTwo = value;
                OnPropertyChanged();
                RenderView();
            }
        }

        /// <summary>
        /// Property for _permutationBitsRoundTwo
        /// </summary>
        public bool[] PermutationBitsRoundTwo
        {
            get => _permutationBitsRoundTwo;
            set
            {
                _permutationBitsRoundTwo = value;
                OnPropertyChanged();
                RenderView();
            }
        }

        /// <summary>
        /// Property for _bitsKeyRoundThree
        /// </summary>
        public bool[] BitsKeyRoundThree
        {
            get => _bitsKeyRoundThree;
            set
            {
                _bitsKeyRoundThree = value;
                OnPropertyChanged();
                RenderView();
            }
        }

        /// <summary>
        /// Property for _permutationBitsRoundThree
        /// </summary>
        public bool[] PermutationBitsRoundThree
        {
            get => _permutationBitsRoundThree;
            set
            {
                _permutationBitsRoundThree = value;
                OnPropertyChanged();
                RenderView();
            }
        }

        /// <summary>
        /// Property for _bitsKeyRoundFour
        /// </summary>
        public bool[] BitsKeyRoundFour
        {
            get => _bitsKeyRoundFour;
            set
            {
                _bitsKeyRoundFour = value;
                OnPropertyChanged();
                RenderView();
            }
        }

        /// <summary>
        /// Property for _permutationBitsRoundFour
        /// </summary>
        public bool[] PermutationBitsRoundFour
        {
            get => _permutationBitsRoundFour;
            set
            {
                _permutationBitsRoundFour = value;
                OnPropertyChanged();
                RenderView();
            }
        }

        /// <summary>
        /// Property for _bitsKeyRoundFive
        /// </summary>
        public bool[] BitsKeyRoundFive
        {
            get => _bitsKeyRoundFive;
            set
            {
                _bitsKeyRoundFive = value;
                OnPropertyChanged();
                RenderView();
            }
        }

        /// <summary>
        /// Property for _bitsKeyRoundSix
        /// </summary>
        public bool[] BitsKeyRoundSix
        {
            get => _bitsKeyRoundSix;
            set
            {
                _bitsKeyRoundSix = value;
                OnPropertyChanged();
                RenderView();
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