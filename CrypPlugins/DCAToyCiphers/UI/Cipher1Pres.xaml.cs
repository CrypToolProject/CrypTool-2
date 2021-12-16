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
    /// Interaktionslogik für Cipher1Pres.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("DCAToyCiphers.Properties.Resources")]
    public partial class Cipher1Pres : UserControl, INotifyPropertyChanged
    {
        private ObservableCollection<TableMapping> sboxData;
        private int[] _keys;
        private string _currentK0;
        private string _currentK1;

        /// <summary>
        /// Constructor
        /// </summary>
        public Cipher1Pres()
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

            CurrentK0 = "0000000000000000";
            CurrentK0 = CurrentK0.Insert(8, " ");
            CurrentK1 = "0000000000000000";
            CurrentK1 = CurrentK1.Insert(8, " ");

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
