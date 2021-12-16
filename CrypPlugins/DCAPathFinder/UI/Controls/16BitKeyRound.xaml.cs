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
    public partial class _16BitKeyRound : UserControl, INotifyPropertyChanged
    {
        private string _inputColor;
        private string _outputColor;

        /// <summary>
        /// Constructor
        /// </summary>
        public _16BitKeyRound()
        {
            InputColor = "Black";
            OutputColor = "Black";

            DataContext = this;
            InitializeComponent();
        }

        /// <summary>
        /// Property for input color
        /// </summary>
        public string InputColor
        {
            get => _inputColor;
            set
            {
                _inputColor = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for output color
        /// </summary>
        public string OutputColor
        {
            get => _outputColor;
            set
            {
                _outputColor = value;
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