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

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace DCAToyCiphers.UI
{
    /// <summary>
    /// Interaktionslogik für Cipher3Pres.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("DCAToyCiphers.Properties.Resources")]
    public partial class Cipher3Pres : UserControl, INotifyPropertyChanged
    {
        private ObservableCollection<TableMapping> sboxData;
        private ObservableCollection<TableMapping> permutationData;
        private int[] _keys;
        private string _currentK0;
        private string _currentK1;
        private string _currentK2;
        private string _currentK3;
        private string _currentK4;
        private string _currentK5;

        /// <summary>
        /// Constructor
        /// </summary>
        public Cipher3Pres()
        {
            sboxData = new ObservableCollection<TableMapping>
            {
                new TableMapping()
                {
                    Direction = Properties.Resources.Input,
                    ZeroOutput = 0,
                    OneOutput = 1,
                    TwoOutput = 2,
                    ThreeOutput = 3,
                    FourOutput = 4,
                    FiveOutput = 5,
                    SixOutput = 6,
                    SevenOutput = 7,
                    EightOutput = 8,
                    NineOutput = 9,
                    TenOutput = 10,
                    ElevenOutput = 11,
                    TwelveOutput = 12,
                    ThirteenOutput = 13,
                    FourteenOutput = 14,
                    FifteenOutput = 15
                },
                new TableMapping()
                {
                    Direction = Properties.Resources.Output,
                    ZeroOutput = 6,
                    OneOutput = 4,
                    TwoOutput = 12,
                    ThreeOutput = 5,
                    FourOutput = 0,
                    FiveOutput = 7,
                    SixOutput = 2,
                    SevenOutput = 14,
                    EightOutput = 1,
                    NineOutput = 15,
                    TenOutput = 3,
                    ElevenOutput = 13,
                    TwelveOutput = 8,
                    ThirteenOutput = 10,
                    FourteenOutput = 9,
                    FifteenOutput = 11
                }
            };

            permutationData = new ObservableCollection<TableMapping>
            {
                new TableMapping()
                {
                    Direction = Properties.Resources.TablePermutationInput,
                    ZeroOutput = 0,
                    OneOutput = 1,
                    TwoOutput = 2,
                    ThreeOutput = 3,
                    FourOutput = 4,
                    FiveOutput = 5,
                    SixOutput = 6,
                    SevenOutput = 7,
                    EightOutput = 8,
                    NineOutput = 9,
                    TenOutput = 10,
                    ElevenOutput = 11,
                    TwelveOutput = 12,
                    ThirteenOutput = 13,
                    FourteenOutput = 14,
                    FifteenOutput = 15
                },
                new TableMapping()
                {
                    Direction = Properties.Resources.TablePermutationOutput,
                    ZeroOutput = 0,
                    OneOutput = 4,
                    TwoOutput = 8,
                    ThreeOutput = 12,
                    FourOutput = 1,
                    FiveOutput = 5,
                    SixOutput = 9,
                    SevenOutput = 13,
                    EightOutput = 2,
                    NineOutput = 6,
                    TenOutput = 10,
                    ElevenOutput = 14,
                    TwelveOutput = 3,
                    ThirteenOutput = 7,
                    FourteenOutput = 11,
                    FifteenOutput = 15
                }
            };

            CurrentK0 = "0000000000000000";
            CurrentK0 = CurrentK0.Insert(8, " ");
            CurrentK1 = "0000000000000000";
            CurrentK1 = CurrentK1.Insert(8, " ");
            CurrentK2 = "0000000000000000";
            CurrentK2 = CurrentK2.Insert(8, " ");
            CurrentK3 = "0000000000000000";
            CurrentK3 = CurrentK3.Insert(8, " ");
            CurrentK4 = "0000000000000000";
            CurrentK4 = CurrentK4.Insert(8, " ");
            CurrentK5 = "0000000000000000";
            CurrentK5 = CurrentK5.Insert(8, " ");

            DataContext = this;
            InitializeComponent();
        }

        /// <summary>
        /// Property for the keys
        /// </summary>
        public int[] Keys
        {
            get => _keys;
            set
            {
                _keys = value;
                ushort keyTemp = Convert.ToUInt16(_keys[0]);

                _currentK0 = Convert.ToString(keyTemp, 2).PadLeft(16, '0');
                CurrentK0 = _currentK0.Insert(8, " ");

                keyTemp = Convert.ToUInt16(_keys[1]);
                _currentK1 = Convert.ToString(keyTemp, 2).PadLeft(16, '0');
                CurrentK1 = _currentK1.Insert(8, " ");

                keyTemp = Convert.ToUInt16(_keys[2]);
                _currentK2 = Convert.ToString(keyTemp, 2).PadLeft(16, '0');
                CurrentK2 = _currentK2.Insert(8, " ");

                keyTemp = Convert.ToUInt16(_keys[3]);
                _currentK3 = Convert.ToString(keyTemp, 2).PadLeft(16, '0');
                CurrentK3 = _currentK3.Insert(8, " ");

                keyTemp = Convert.ToUInt16(_keys[4]);
                _currentK4 = Convert.ToString(keyTemp, 2).PadLeft(16, '0');
                CurrentK4 = _currentK4.Insert(8, " ");

                keyTemp = Convert.ToUInt16(_keys[5]);
                _currentK5 = Convert.ToString(keyTemp, 2).PadLeft(16, '0');
                CurrentK5 = _currentK5.Insert(8, " ");

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for binding K0
        /// </summary>
        public string CurrentK0
        {
            get => _currentK0;
            set
            {
                _currentK0 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for binding K1
        /// </summary>
        public string CurrentK1
        {
            get => _currentK1;
            set
            {
                _currentK1 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for binding K2
        /// </summary>
        public string CurrentK2
        {
            get => _currentK2;
            set
            {
                _currentK2 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for binding K3
        /// </summary>
        public string CurrentK3
        {
            get => _currentK3;
            set
            {
                _currentK3 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for binding K4
        /// </summary>
        public string CurrentK4
        {
            get => _currentK4;
            set
            {
                _currentK4 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for binding K5
        /// </summary>
        public string CurrentK5
        {
            get => _currentK5;
            set
            {
                _currentK5 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for binding the sboxData
        /// </summary>
        public ObservableCollection<TableMapping> SBoxData
        {
            get => sboxData;
            set
            {
                sboxData = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for binding the permutationData
        /// </summary>
        public ObservableCollection<TableMapping> PermutationData
        {
            get => permutationData;
            set
            {
                permutationData = value;
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
