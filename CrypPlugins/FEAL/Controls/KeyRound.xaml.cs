/*
   Copyright 2019 Nils Kopal <Nils.Kopal<at>CrypTool.org

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
using System.Windows.Controls;

namespace CrypTool.Plugins.FEAL.Controls
{
    /// <summary>
    /// Interaktionslogik für KeyRound.xaml
    /// </summary>
    public partial class KeyRound : UserControl, INotifyPropertyChanged
    {
        private bool _firstRound = false;
        private bool _lastRound = false;
        private string _roundKeyNames = "(Kx,Ky)";
        private string _roundKey = "00000000";

        public KeyRound()
        {
            InitializeComponent();
            DataContext = this;
        }

        /// <summary>
        /// Sets, if this round is the first round
        /// </summary>
        public bool FirstRound
        {

            get => _firstRound;

            set
            {
                _firstRound = value;
                NotifyPropertyChanged("FirstRound");
            }
        }

        /// <summary>
        /// Sets, if this round is the last round
        /// </summary>
        public bool LastRound
        {

            get => _lastRound;

            set
            {
                _lastRound = value;
                NotifyPropertyChanged("LastRound");
            }
        }

        /// <summary>
        /// Sets the display name of the round keys
        /// </summary>
        public string RoundKeyNames
        {

            get => _roundKeyNames;

            set
            {
                _roundKeyNames = value;
                NotifyPropertyChanged("RoundKeyNames");
            }
        }

        /// <summary>
        /// Sets the display of the round key
        /// </summary>
        public string RoundKey
        {

            get => _roundKey;

            set
            {
                _roundKey = value;
                NotifyPropertyChanged("RoundKey");
            }
        }

        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
