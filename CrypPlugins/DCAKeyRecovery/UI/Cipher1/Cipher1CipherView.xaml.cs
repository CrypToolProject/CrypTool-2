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

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace DCAKeyRecovery.UI.Cipher1
{
    /// <summary>
    /// Interaktionslogik für Cipher1Attack.xaml
    /// </summary>
    public partial class Cipher1CipherView : UserControl, INotifyPropertyChanged
    {
        private string _m1xorm2 = "";
        private string _c1xorc2 = "";
        private string _uBits = "";
        private string _SBoxU = "";
        private string _vBits = "";
        private string _k1Bits = "";
        private ObservableCollection<int> _keyCandidates;

        public Cipher1CipherView()
        {
            KeyCandidates = new ObservableCollection<int>();
            DataContext = this;
            InitializeComponent();
        }

        /// <summary>
        /// Property for key candidate list
        /// </summary>
        public ObservableCollection<int> KeyCandidates
        {
            get => _keyCandidates;
            set
            {
                _keyCandidates = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for plaintext difference
        /// </summary>
        public string M1XORM2
        {
            get => _m1xorm2;
            set
            {
                _m1xorm2 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for ciphertext difference
        /// </summary>
        public string C1XORC2
        {
            get => _c1xorc2;
            set
            {
                _c1xorc2 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for binary u
        /// </summary>
        public string UBits
        {
            get => _uBits;
            set
            {
                _uBits = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for binary SBoxed u
        /// </summary>
        public string SBoxU
        {
            get => _SBoxU;
            set
            {
                _SBoxU = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for binary v
        /// </summary>
        public string VBits
        {
            get => _vBits;
            set
            {
                _vBits = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for k1 candidate
        /// </summary>
        public string K1Bits
        {
            get => _k1Bits;
            set
            {
                _k1Bits = value;
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
