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
using System.Windows;
using System.Windows.Controls;

namespace DCAToyCiphers.UI
{
    /// <summary>
    /// Interaktionslogik für Cipher4Pres.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("DCAToyCiphers.Properties.Resources")]
    public partial class Cipher4Pres : UserControl, INotifyPropertyChanged
    {
        private ObservableCollection<TableMapping> sboxData;
        private int[] _keys;
        private string _currentK0;
        private string _currentK1;
        private string _currentK2;
        private string _currentK3;

        /// <summary>
        /// Constructor
        /// </summary>
        public Cipher4Pres()
        {
            sboxData = new ObservableCollection<TableMapping>
            {
                new TableMapping()
                {
                    Direction = DCAToyCiphers.Properties.Resources.Input,
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
                    Direction = DCAToyCiphers.Properties.Resources.Output,
                    ZeroOutput = 10,
                    OneOutput = 0,
                    TwoOutput = 9,
                    ThreeOutput = 14,
                    FourOutput = 6,
                    FiveOutput = 3,
                    SixOutput = 15,
                    SevenOutput = 5,
                    EightOutput = 1,
                    NineOutput = 13,
                    TenOutput = 12,
                    ElevenOutput = 7,
                    TwelveOutput = 11,
                    ThirteenOutput = 4,
                    FourteenOutput = 2,
                    FifteenOutput = 8
                }
            };



            DataContext = this;
            InitializeComponent();
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
        /// Property for the keys
        /// </summary>
        public int[] Keys
        {
            get => _keys;
            set
            {
                _keys = value;
                ushort keyTemp = Convert.ToUInt16(_keys[0]);

                CurrentK0 = Convert.ToString(keyTemp, 2).PadLeft(4, '0');

                keyTemp = Convert.ToUInt16(_keys[1]);
                CurrentK1 = Convert.ToString(keyTemp, 2).PadLeft(4, '0');

                keyTemp = Convert.ToUInt16(_keys[2]);
                CurrentK2 = Convert.ToString(keyTemp, 2).PadLeft(4, '0');

                keyTemp = Convert.ToUInt16(_keys[3]);
                CurrentK3 = Convert.ToString(keyTemp, 2).PadLeft(4, '0');

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
        /// Toggles the view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToggleViewClicked(object sender, RoutedEventArgs e)
        {
            if (CipherGrid.Visibility == Visibility.Hidden)
            {
                CipherGrid.Visibility = Visibility.Visible;
                LabelGrid.Visibility = Visibility.Hidden;
            }
            else
            {
                CipherGrid.Visibility = Visibility.Hidden;
                LabelGrid.Visibility = Visibility.Visible;
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
