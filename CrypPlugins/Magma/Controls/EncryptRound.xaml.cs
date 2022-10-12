/*
   Copyright 2022 Nils Kopal, CrypTool project

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

namespace CrypTool.Plugins.Magma.Controls
{
    /// <summary>
    /// Interaktionslogik für CryptRound.xaml
    /// </summary>
    public partial class EncryptRound : UserControl, INotifyPropertyChanged
    {
        private bool _lastRound = false;
        private string _roundName = string.Empty;
        private string _roundKey = string.Empty;

        private string _leftIn = string.Empty;
        private string _leftOut = string.Empty;
        private string _rightIn = string.Empty;
        private string _rightOut = string.Empty;

        public EncryptRound()
        {
            InitializeComponent();
            DataContext = this;
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
                NotifyPropertyChanged(nameof(LastRound));
            }
        }

        /// <summary>
        /// Sets the display name of the round keys
        /// </summary>
        public string RoundName
        {

            get => _roundName;

            set
            {
                _roundName = value;
                NotifyPropertyChanged(nameof(RoundName));
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
                NotifyPropertyChanged(nameof(RoundKey));
            }
        }

        /// <summary>
        /// Sets the display of the left in value
        /// </summary>
        public string LeftIn
        {

            get => _leftIn;

            set
            {
                _leftIn = value;
                NotifyPropertyChanged(nameof(LeftIn));
            }
        }

        /// <summary>
        /// Sets the display of the left out value
        /// </summary>
        public string LeftOut
        {

            get => _leftOut;

            set
            {
                _leftOut = value;
                NotifyPropertyChanged(nameof(LeftOut));
            }
        }

        /// <summary>
        /// Sets the display of the right in value
        /// </summary>
        public string RightIn
        {

            get => _rightIn;

            set
            {
                _rightIn = value;
                NotifyPropertyChanged(nameof(RightIn));
            }
        }

        /// <summary>
        /// Sets the display of the right out value
        /// </summary>
        public string RightOut
        {

            get => _rightOut;

            set
            {
                _rightOut = value;
                NotifyPropertyChanged(nameof(RightOut));
            }
        }

        private void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
